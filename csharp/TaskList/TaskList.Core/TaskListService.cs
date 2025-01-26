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
            AddProject("home-improvement");
            AddTask("home-improvement", "paint-wall");
            AddTask("home-improvement", "vacuum-hallway");
            AddTask("home-improvement", "replace-lightbulb");
            CheckTask(2);
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

        private long GetNextAvailableTaskId()
        {
            return ++lastTaskIdUsed;
        }
    }
}
