namespace TaskList.Core
{
    public interface ITaskListService
    {
        public List<Task> GetAllTasks();
        public IDictionary<string, IList<Task>> GetAllProjects();

        public Task? GetTaskById(int id);

        public void AddProject(string projectName);

        public void AddTask(string projectName, string taskDescription);

    }
}
