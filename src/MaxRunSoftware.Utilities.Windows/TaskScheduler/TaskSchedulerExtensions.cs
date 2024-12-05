using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace MaxRunSoftware.Utilities.Windows;

public static class TaskSchedulerExtensions
{
    public static Task? GetTask(this TaskScheduler taskScheduler, string path) => taskScheduler.GetTask(new(path));

    public static TaskFolder? GetTaskFolder(this TaskScheduler taskScheduler, string path) => taskScheduler.GetTaskFolder(new(path));

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
