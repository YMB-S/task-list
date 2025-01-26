using Microsoft.AspNetCore.Mvc;
using TaskList;
using TaskList.Core;

namespace Tasks
{
	[TestFixture]
	public sealed class ApplicationTest
	{
		private FakeConsole console;
        private ITaskListService taskListService;
        private System.Threading.Thread applicationThread;

		[SetUp]
		public void StartTheApplication()
		{
			console = new FakeConsole();
			taskListService = new TaskListService();
			
			var taskList = new TaskList.TaskList(console, taskListService);

            taskListService.ClearDummyData();
            
			this.applicationThread = new System.Threading.Thread(() => taskList.Run());
			applicationThread.Start();
			ReadLines(TaskList.TaskList.startupText);
		}


		[Test, Timeout(1000)]
		public void ItWorks()
		{
			taskListService.AddProject("Ortec-interview");
			taskListService.AddTask("Ortec-interview", "attend-second-interview");
			taskListService.SetDeadline(1, DateOnly.Parse("28-01-2025"));

            taskListService.AddTask("Ortec-interview", "finish-tasklist-application");
            taskListService.SetDeadline(2, DateOnly.Parse("26-01-2025"));
            taskListService.CheckTask(2);

            var allProjects = taskListService.GetAllProjects();

            Assert.That(allProjects["Ortec-interview"].Where(
				task => task.Description == "attend-second-interview" &&
				task.Deadline.Equals(DateOnly.Parse("28-01-2025")))
				.Any());

			Assert.IsTrue(allProjects["Ortec-interview"].Where(
				task => task.Description == "finish-tasklist-application" &&
				task.Done)
				.Any());
        }

		private void Read(string expectedOutput)
		{
			var length = expectedOutput.Length;
			var actualOutput = console.RetrieveOutput(expectedOutput.Length);
			Assert.AreEqual(expectedOutput, actualOutput);
		}

		private void ReadLines(params string[] expectedOutput)
		{
			foreach (var line in expectedOutput)
			{
				Read(line + Environment.NewLine);
			}
		}
	}
}
