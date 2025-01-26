using System.Xml.Linq;

namespace TaskList.Core
{
    public class TaskListService : ITaskListService
    {
        private readonly IDictionary<string, IList<Task>> projects;

        private long lastTaskIdUsed = 0;

        public TaskListService()
        {
            projects = new Dictionary<string, IList<Task>>();
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

            projects[projectName].Add(new Task { Id = GetNextAvailableId(), Description = taskDescription, Done = false });
        }

        private long GetNextAvailableId()
        {
            return ++lastTaskIdUsed;
        }
    }
}
