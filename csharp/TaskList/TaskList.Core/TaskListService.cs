using System;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskList.Core
{
    public class TaskListService : ITaskListService
    {
        private readonly IDictionary<string, IList<Task>> projects;

        private long lastTaskIdUsed = 0;

        public TaskListService()
        {
            projects = new Dictionary<string, IList<Task>>();
            AddDummyData();
        }

        private void AddDummyData()
        {
            AddProject("ortec-interview");
            AddTask("ortec-interview", "finish-tasklist-application");
            AddTask("ortec-interview", "attend-second-interview");
            SetDeadline(1, DateOnly.Parse("27-01-2025"));
            SetDeadline(2, DateOnly.Parse("28-01-2025"));

            AddProject("home-improvement");
            AddTask("home-improvement", "paint-wall");
            AddTask("home-improvement", "vacuum-hallway");
            AddTask("home-improvement", "replace-lightbulb");
            CheckTask(2);
        }

        public void ClearDummyData()
        {
            projects.Clear();
            lastTaskIdUsed = 0;
        }

        public Task GetTaskById(int id)
        {
            foreach (var tasks in projects.Values)
            {
                foreach (var task in tasks)
                {
                    if (task.Id == id)
                    {
                        return task;
                    }
                }
            }
            throw new KeyNotFoundException($"No task found for id {id}");
        }

        public List<Task> GetAllTasks()
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

        public IDictionary<string, IList<Task>> GetAllProjects()
        {
            return projects;
        }

        public void AddProject(string projectName)
        {
            projects[projectName] = new List<Task>();
        }

        public void AddTask(string projectName, string taskDescription)
        {
            if (!projects.ContainsKey(projectName))
            {
                throw new KeyNotFoundException($"No project named {projectName} was found.");
            }

            projects[projectName].Add(new Task { Id = GetNextAvailableTaskId(), Description = taskDescription, Done = false });
        }

        public void CheckTask(int id)
        {
            SetDone(id, true);
        }

        public void UncheckTask(int id)
        {
            SetDone(id, false);
        }

        private void SetDone(int id, bool done)
        {
            var identifiedTask = projects
                .Select(project => project.Value.FirstOrDefault(task => task.Id == id))
                .Where(task => task != null)
                .FirstOrDefault();

            if (identifiedTask == null)
            {
                throw new KeyNotFoundException($"No task was found for id {id}.");
            }

            identifiedTask.Done = done;
        }

        public void SetDeadline(int taskId, DateOnly deadlineDate)
        {
            Task taskToAddDeadlineTo = GetTaskById(taskId) ?? throw new KeyNotFoundException($"Task not found for id {taskId}");
            taskToAddDeadlineTo.Deadline = deadlineDate;
        }

        public Dictionary<string, List<Task>> GetTasksGroupedByDeadline()
        {
            var allTasks = GetAllTasks();

            var tasksGroupedByDeadline = allTasks
            .Where(t => t.Deadline.HasValue)
            .OrderBy(t => t.Deadline!.Value)
            .GroupBy(t => t.Deadline!.Value)
            .ToDictionary(g => g.Key.ToString(), g => g.ToList());

            var tasksWithoutDeadline = allTasks
                .Where(t => !t.Deadline.HasValue)
                .ToList();

            if (tasksWithoutDeadline.Count > 0)
            {
                tasksGroupedByDeadline.Add("No deadline", tasksWithoutDeadline);
            }

            return tasksGroupedByDeadline;
        }

        private long GetNextAvailableTaskId()
        {
            return ++lastTaskIdUsed;
        }
    }
}
