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
        [Route("tasks")]
        public IActionResult GetTasks()
        {
            return new JsonResult(taskListService.GetAllTasks());
        }

        [HttpGet]
        [Route("projects")]
        public IActionResult GetProjects()
        {
            return new JsonResult(taskListService.GetAllProjects());
        }

        [HttpPost]
        [Route("projects")]
        public IActionResult AddProject(string projectName)
        {
            taskListService.AddProject(projectName);
            return CreatedAtAction("AddProject", projectName);
        }

        [HttpPost]
        [Route("projects/{project_name}/tasks")]
        public IActionResult AddTask(string projectName, string taskDescription)
        {
            taskListService.AddTask(projectName, taskDescription);
            return CreatedAtAction("AddTask", $"{projectName}: {taskDescription}");
        }
    }
}