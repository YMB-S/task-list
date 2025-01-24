using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TaskList
{
	public sealed class TaskList
	{
		private const string CommandUsedToQuitApplication = "quit";
		public static readonly string startupText = "Welcome to TaskList! Type 'help' for available commands.";

		private readonly IDictionary<string, IList<Task>> projects = new Dictionary<string, IList<Task>>();
		private readonly IConsole console;

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
			console.WriteLine(startupText);

			Execute("add project self-improvement");
			Execute("add task self-improvement do-thing");
			Execute("add task self-improvement do-other-thing");

            Execute("add project learn-more");
            Execute("add task learn-more read-book");
            Execute("add task learn-more read-another-book");
            Execute("deadline 3 01/01/2026");
            //Execute("");

            while (true) {
				console.Write("> ");
				var command = console.ReadLine();
				if (command == CommandUsedToQuitApplication) {
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
					Show();
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
				default:
					Error($"I don't know what the command {command} is.");
					break;
			}
		}

		private void Show()
		{
			foreach (var project in projects)
			{
				const int formattingIdSpace = -5;
                const int formattingDescriptionSpace = -25;
                const int formattingDoneSpace = -15;
                const int formattingDeadlineSpace = -20;

                string header = $"{"Id",formattingIdSpace} {"Description",formattingDescriptionSpace} {"Done",formattingDoneSpace} {"Deadline",formattingDeadlineSpace}";

                console.WriteLine(Environment.NewLine);
                console.WriteLine($"Tasks for project: {project.Key}");

                console.WriteLine(new string('-', header.Length));
                console.WriteLine(header);
                console.WriteLine(new string('-', header.Length));

                foreach (var task in project.Value)
                {
                    console.WriteLine($"{task.Id,formattingIdSpace} {task.Description,formattingDescriptionSpace} {(task.Done ? "Yes" : "No"), formattingDoneSpace} {task.Deadline,formattingDeadlineSpace}");
                }   
			}
            console.WriteLine(Environment.NewLine);
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
				//var projectTask = splitCommandLine[1].Split(" ".ToCharArray(), 2);
				//AddTask(projectTask[0], projectTask[1]);

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
