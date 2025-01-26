namespace TaskList.Core
{
    public interface ITaskListService
    {
        public Task? GetTaskById(int id);

        public List<Task> GetAllTasks();

        public IDictionary<string, IList<Task>> GetAllProjects();

        public void AddProject(string projectName);

        public void AddTask(string projectName, string taskDescription);

        public void CheckTask(int id);

        public void UncheckTask(int id);

        public void SetDeadline(int taskId, DateOnly deadlineDate);
    }
}
