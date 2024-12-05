using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace MaxRunSoftware.Utilities.Windows;

public static class TaskSchedulerPathExtensions
{
    public static TaskSchedulerPath GetPath(this Task task) => new(task);

    public static TaskSchedulerPath GetPath(this TaskFolder folder) => new(folder);

    public static Dictionary<TaskSchedulerPath, List<Task>> GetTasksByFolder(this TaskScheduler taskScheduler)
    {
        var d = new Dictionary<TaskSchedulerPath, List<Task>>();

        foreach (var taskFolder in taskScheduler.GetTaskFolders())
        {
            d.AddToList(taskFolder.GetPath(), taskFolder.Tasks.ToArray());
        }

        return d;
    }
}
