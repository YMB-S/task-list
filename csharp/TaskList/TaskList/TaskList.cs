using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using TaskList.Core;

namespace TaskList
{
	public sealed class TaskList
	{
		private const string CommandUsedToQuitApplication = "quit";
		public static readonly string startupText = "Welcome to TaskList! Type 'help' for available commands.";

		private readonly IConsole console;
        private readonly ITaskListService taskListService;

		public TaskList(IConsole console, ITaskListService taskListService)
		{
			this.console = console ?? throw new ArgumentNullException(nameof(console));
			this.taskListService = taskListService ?? throw new ArgumentNullException(nameof(taskListService));
		}

		public void Run()
		{
            console.WriteLine(startupText);

            while (true)
			{
				console.Write("> ");
				var command = console.ReadLine();
				if (command == CommandUsedToQuitApplication)
				{
					break;
				}
				Execute(command);
			}
		}

		private void Execute(string commandLine)
		{
			var splitCommandLine = commandLine.Split(" ".ToCharArray());
			var command = splitCommandLine[0];

			switch (command) {
				case "show":
					Show(onlyShowTasksWithDeadlineOfToday: false);
					break;
				case "add":
					Add(splitCommandLine);
					break;
				case "check":
					Check(splitCommandLine[1]);
					break;
				case "uncheck":
					Uncheck(splitCommandLine[1]);
					break;
				case "help":
					Help();
					break;
				case "deadline":
					AddDeadline(splitCommandLine);
					break;
				case "today":
					ShowTasksWithDeadlineOfToday();
					break;
				case "view-by-deadline":
					ShowTasksGroupedByDeadline();
					break;
                default:
					Error($"I don't know what the command {command} is.");
					break;
			}
		}

		private void Show(bool onlyShowTasksWithDeadlineOfToday)
		{
            const int formattingIdSpace = -5;
            const int formattingDescriptionSpace = -35;
            const int formattingDoneSpace = -15;
            const int formattingDeadlineSpace = -20;

            string header = $"{"Id",formattingIdSpace} {"Description",formattingDescriptionSpace} {"Done",formattingDoneSpace} {"Deadline",formattingDeadlineSpace}";

			int amountOfTasksShown = 0;
            
			foreach (var project in taskListService.GetAllProjects())
			{
                List<Task> tasksToShow = new();

				if (onlyShowTasksWithDeadlineOfToday)
				{
					var currentDate = DateOnly.FromDateTime(DateTime.Now);
                    tasksToShow = project.Value.Where(task => task.Deadline == currentDate).Select(task => task).ToList();
                }
				else
				{
                    tasksToShow = project.Value.Select(task => task).ToList();
                }

				if (tasksToShow.Count == 0)
				{
					continue;
				}

                console.WriteLine(Environment.NewLine);
                console.WriteLine($"Tasks for project: {project.Key}");

                console.WriteLine(new string('-', header.Length));
                console.WriteLine(header);
                console.WriteLine(new string('-', header.Length));

                foreach (var task in tasksToShow)
                {
					console.WriteLine($"{task.Id, formattingIdSpace} {task.Description, formattingDescriptionSpace} {(task.Done ? "Yes" : "No"), formattingDoneSpace} {task.Deadline, formattingDeadlineSpace}");
					amountOfTasksShown++;
                }
			}

            if (amountOfTasksShown == 0)
			{
				if (onlyShowTasksWithDeadlineOfToday)
				{
					console.WriteLine("There are no tasks with a deadline of today.");
				}
				else
				{
                    console.WriteLine("There are no tasks to show.");
                }
            }

            console.WriteLine(Environment.NewLine);
        }

		private void ShowTasksWithDeadlineOfToday()
		{
			Show(onlyShowTasksWithDeadlineOfToday: true);
		}

		private void ShowTasksGroupedByDeadline()
		{
			List<Task> allTasks = GetAllTasks();

            var tasksGroupedByDeadline = allTasks
            .Where(t => t.Deadline.HasValue)
            .OrderBy(t => t.Deadline!.Value)
            .GroupBy(t => t.Deadline!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

            var tasksWithoutDeadline = allTasks
                .Where(t => !t.Deadline.HasValue)
                .ToList();

			console.WriteLine(Environment.NewLine);
            foreach (var group in tasksGroupedByDeadline)
            {
                console.WriteLine($"{group.Key.ToShortDateString()}:");
                foreach (var task in group.Value)
                {
                    console.WriteLine($"\t{task.Id}: {task.Description}");
                }
            }

            console.WriteLine(Environment.NewLine);
            console.WriteLine("No deadline:");
            foreach (var task in tasksWithoutDeadline)
            {
                console.WriteLine($"\t{task.Description}");
            }
        }

		private void Add(string[] splitCommandLine)
		{
			var subcommand = splitCommandLine[1];
			if (subcommand == "project")
			{
				string newProjectName = splitCommandLine[2];
                AddProject(newProjectName);
				return;
			}

			if (subcommand == "task")
			{
				AddTask(splitCommandLine[2], splitCommandLine[3]);
			}
		}

		private void AddProject(string name)
		{
			taskListService.AddProject(name);
		}

		private void AddTask(string projectName, string description)
		{
			try
			{
				taskListService.AddTask(projectName, description);
			}
            catch (KeyNotFoundException e)
            {
                console.WriteLine(e.ToString());
            }
        }

		private Task? GetTaskById(int id)
		{
			try
			{
				return taskListService.GetTaskById(id);
			}
			catch (KeyNotFoundException e)
			{
				console.WriteLine(e.ToString());
			}

			return null;
        }

		private List<Task> GetAllTasks()
		{
			return taskListService.GetAllTasks();
        }

		private void AddDeadline(string[] splitCommandLine)
		{
			string taskId = splitCommandLine[1];
			string deadlineDateString = splitCommandLine[2];

            try
			{
				taskListService.SetDeadline(int.Parse(taskId), DateOnly.Parse(splitCommandLine[2]));
            }
			catch (Exception e)
			{
				Error($"Error occurred during setting deadline: {e}");
			}
		}

		private void Check(string idString)
		{
            taskListService.CheckTask(int.Parse(idString));
        }

        private void Uncheck(string idString)
		{
			taskListService.UncheckTask(int.Parse(idString));
        }

		private void Help()
		{
			console.WriteLine("Commands:");
			console.WriteLine("  show");
			console.WriteLine("  today");
			console.WriteLine("  add project <project name>");
			console.WriteLine("  add task <project name> <task description>");
			console.WriteLine("  check <task ID>");
			console.WriteLine("  uncheck <task ID>");
            console.WriteLine("  deadline <task ID> <deadline date>");
            console.WriteLine("  view-by-deadline");
            console.WriteLine();
		}

		private void Error(string errorMessage)
		{
			console.WriteLine(errorMessage);
		}
	}
}
