namespace TaskList.Core
{
    public class TaskListService : ITaskListService
    {
        public TaskListService()
        {

        }

        public void Help()
        {
            Console.WriteLine("this is a help message");
        }
    }
}
