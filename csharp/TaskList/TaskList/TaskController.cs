using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TaskList.Core;

namespace TaskList
{
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskListService taskListService;

        public TasksController(ITaskListService taskListService)
        {
            this.taskListService = taskListService ?? throw new ArgumentNullException(nameof(taskListService));
        }

        [HttpGet]
        [Route("Tasks")]
        public IActionResult GetTasks()
        {
            return new JsonResult(taskListService.GetAllTasks());
        }

        [HttpGet]
        [Route("Projects")]
        public IActionResult GetProjects()
        {
            return new JsonResult(taskListService.GetAllProjects());
        }

        [HttpPost]
        [Route("Projects")]
        public IActionResult AddProject(string projectName)
        {
            taskListService.AddProject(projectName);
            return CreatedAtAction("AddProject", projectName);
        }

        [HttpPost]
        [Route("Projects/AddTask")]
        public IActionResult AddTask(string projectName, string taskDescription)
        {
            taskListService.AddTask(projectName, taskDescription);
            return CreatedAtAction("AddTask", $"{projectName}: {taskDescription}");
        }

        [HttpPut]
        [Route("Tasks/SetDeadline")]
        public IActionResult SetDeadline(int taskId, string deadlineDateString)
        {
            taskListService.SetDeadline(taskId, DateOnly.Parse(deadlineDateString));
            return CreatedAtAction("SetDeadline", $"{taskId}: {deadlineDateString}");
        }

        [HttpGet]
        [Route("Projects/ViewByDeadline")]
        public IActionResult GetProjectsByDeadline()
        {
            return new JsonResult(taskListService.GetTasksGroupedByDeadline());
        }
    }
}