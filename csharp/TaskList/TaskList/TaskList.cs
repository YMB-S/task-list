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

		private readonly IDictionary<string, IList<Task>> projects = new Dictionary<string, IList<Task>>();
		private readonly IConsole console;

		ITaskListService service;

		private long lastId = 0;

		public static void Main(string[] args)
		{
			new TaskList(new RealConsole()).Run();
		}

		public TaskList(IConsole console)
		{
			this.console = console;
			// add a logger
		}

		public void Run()
		{
			AddDummyData();
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

		private void AddDummyData()
		{
            Execute("add project ortec-interview");
            Execute("add task ortec-interview finish-tasklist-application");
            Execute("add task ortec-interview attend-second-interview");
            Execute("deadline 1 27-01-2025");
            Execute("deadline 2 28-01-2025");

            Execute("add project learn-more");
            Execute("add task learn-more read-book");
            Execute("add task learn-more read-another-book");
            Execute("deadline 3 01-01-2026");
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
            
			foreach (var project in projects)
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
			projects[name] = new List<Task>();
		}

		private void AddTask(string projectName, string description)
		{
			if (!projects.TryGetValue(projectName, out IList<Task> projectTasks))
			{
				Console.WriteLine("Could not find a project with the name \"{0}\".", projectName);
				return;
			}
			projectTasks.Add(new Task { Id = NextId(), Description = description, Done = false });
		}

		private Task? GetTaskById(int id)
		{
            return projects
                .Select(project => project.Value.FirstOrDefault(task => task.Id == id))
                .Where(task => task != null)
                .First();
        }

		private List<Task> GetAllTasks()
		{
            List<Task> allTasks = new();

            foreach (var listOfTasks in projects.Values)
            {
                foreach (var task in listOfTasks)
                {
                    allTasks.Add(task);
                }
            }

			return allTasks;
        }

		private void AddDeadline(string[] splitCommandLine)
		{
			string taskId = splitCommandLine[1];
			string deadlineDateString = splitCommandLine[2];

            try
			{
                DateOnly deadlineDate = DateOnly.Parse(splitCommandLine[2]);
				Task taskToAddDeadlineTo = GetTaskById(int.Parse(taskId)) ?? throw new FormatException($"Unable to find task for id {taskId}");
				taskToAddDeadlineTo.Deadline = deadlineDate;
            }
			catch (Exception e)
			{
				Error($"Error occurred during parsing: {e}");
			}
		}

		private void Check(string idString)
		{
			SetDone(idString, true);
		}

		private void Uncheck(string idString)
		{
			SetDone(idString, false);
        }

		private void SetDone(string idString, bool done)
		{
			int id = int.Parse(idString);
			var identifiedTask = projects
				.Select(project => project.Value.FirstOrDefault(task => task.Id == id))
				.Where(task => task != null)
				.FirstOrDefault();
			if (identifiedTask == null) {
				console.WriteLine("Could not find a task with an ID of {0}.", id);
				return;
			}

			identifiedTask.Done = done;
		}

		private void Help()
		{
			console.WriteLine("Commands:");
			console.WriteLine("  show");
			console.WriteLine("  add project <project name>");
			console.WriteLine("  add task <project name> <task description>");
			console.WriteLine("  check <task ID>");
			console.WriteLine("  uncheck <task ID>");
			console.WriteLine();
		}

		private void Error(string errorMessage)
		{
			console.WriteLine(errorMessage);
		}

		private long NextId()
		{
			return ++lastId;
		}
	}
}
