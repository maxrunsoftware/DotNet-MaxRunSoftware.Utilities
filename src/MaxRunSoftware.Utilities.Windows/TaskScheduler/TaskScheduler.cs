// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace MaxRunSoftware.Utilities.Windows;

[PublicAPI]
public class TaskScheduler : IDisposable
{
    private readonly ILogger log;
    private readonly object locker = new();
    private TaskService? taskService;

    public TaskService TaskService
    {
        get
        {
            lock (locker)
            {
                if (taskService != null) return taskService;
                throw new ObjectDisposedException(GetType().FullNameFormatted());
            }
        }
    }

    #region Constructor
    
    public TaskScheduler(ILoggerFactory loggerProvider, string host, string? username, string? password, bool forceV1 = false)
    {
        log = loggerProvider.CreateLogger<TaskScheduler>();
        string? accountDomain = null;
        if (username != null)
        {
            var parts = username.Split('\\').TrimOrNull().WhereNotNull().ToArray();
            if (parts.Length > 1)
            {
                accountDomain = parts[0];
                username = parts[1];
            }
        }

        accountDomain ??= host;

        log.LogDebugMethod(new(host, username, forceV1, accountDomain), "Creating new {Type}", typeof(TaskService).FullNameFormatted());
        taskService = new(host, username!, accountDomain, password!, forceV1);
    }
    
    #endregion Constructor

    #region Task

    public Task TaskAdd(TaskSchedulerTask taskSchedulerTask)
    {
        var path = taskSchedulerTask.Path.CheckNotNull();
        var pathParent = path.Parent.CheckNotNull();
        var taskName = path.Name.CheckNotNullTrimmed();

        var dir = CreateTaskFolder(pathParent);

        var task = TaskService.NewTask().CheckNotNull();

        if (task.RegistrationInfo != null)
        {
            task.RegistrationInfo.Description = taskSchedulerTask.Description.TrimOrNull() ?? string.Empty;
            task.RegistrationInfo.Documentation = taskSchedulerTask.Documentation.TrimOrNull() ?? string.Empty;
        }

        var username = taskSchedulerTask.Username ??= Constant.Windows_User_System;
        var password = taskSchedulerTask.Password;

        var taskLogonType = TaskLogonType.Password;
        if (IsServiceAccount(username))
        {
            taskLogonType = TaskLogonType.ServiceAccount;
            username = username.ToUpper();
            password = null;
        }
        
        task.Principal.LogonType = taskLogonType;
        task.Principal.UserId = username;
        task.Settings.Hidden = false;
        
        foreach (var trigger in taskSchedulerTask.Triggers)
        {
            log.LogTraceMethod(new(taskSchedulerTask), "Created trigger: {Trigger}", trigger);
            task.Triggers.Add(trigger);
        }

        foreach (var filePath in taskSchedulerTask.FilePaths)
        {
            log.LogTraceMethod(new(taskSchedulerTask), "Created action: {Action}", filePath);
            var execAction = new ExecAction(
                filePath, 
                taskSchedulerTask.Arguments, 
                taskSchedulerTask.WorkingDirectory
                );
            task.Actions.Add(execAction);
        }

        var t = dir.RegisterTaskDefinition(
            taskName,
            task,
            TaskCreation.CreateOrUpdate,
            username,
            password!,
            taskLogonType
        );

        return t.CheckNotNull();
        
        static bool IsServiceAccount(string username)
        {
            return Enumerable.Any(
                Constant.StringComparers,
                sc => username.In(
                    sc,
                    Constant.Windows_User_System,
                    Constant.Windows_User_LocalService,
                    Constant.Windows_User_NetworkService
                )
            );
        }
    }

    public RunningTask? TaskStart(Task task, bool force = false)
    {
        if (!force)
        {
            if (task.State.NotIn(TaskState.Running))
            {
                log.LogDebugMethod(new(task), "Skipped start for task {TaskName} in state {TaskState} because it is currently running", task.Name, task.State);
                return null;
            }

            if (task.State.NotIn(TaskState.Queued))
            {
                log.LogDebugMethod(new(task), "Skipped start for task {TaskName} in state {TaskState} because it is currently queued", task.Name, task.State);
                return null;
            }
        }

        log.LogDebugMethod(new(task), "Starting task {TaskName} in state {TaskState}", task.Name, task.State);
        return task.Run();
    }

    public void TaskStop(Task task, bool force = false)
    {
        if (!force && task.State.NotIn(TaskState.Queued, TaskState.Running, TaskState.Unknown))
        {
            log.LogDebugMethod(new(task), "Skipped stop for task {TaskName} in state {TaskState}", task.Name, task.State);
            return;
        }

        log.LogDebugMethod(new(task), "Stopping task {TaskName} in state {TaskState}", task.Name, task.State);
        task.Stop();
    }

    public void TaskEnable(Task task, bool force = false)
    {
        if (!force && task.Enabled)
        {
            log.LogDebugMethod(new(task), "Skipped enable for task {TaskName} with Enabled={TaskEnabled}", task.Name, task.Enabled);
            return;
        }

        log.LogDebugMethod(new(task), "Disabling task {TaskName} with Enabled={TaskEnabled}", task.Name, task.Enabled);
        task.Enabled = true;
    }

    public void TaskDisable(Task task, bool force = false)
    {
        if (!force && !task.Enabled)
        {
            log.LogDebugMethod(new(task), "Skipped disable for task {TaskName} with Enabled={TaskEnabled}", task.Name, task.Enabled);
            return;
        }

        log.LogDebugMethod(new(task), "Disabling task {TaskName} with Enabled={TaskEnabled}", task.Name, task.Enabled);
        task.Enabled = false;
    }

    public bool TaskDelete(Task task)
    {
        var taskName = task.Name;
        try
        {
            TaskStop(task);
        }
        catch (Exception e)
        {
            log.LogWarningMethod(new(task), e, "Error stopping task {TaskName}", taskName);
        }

        try
        {
            TaskDisable(task);
        }
        catch (Exception e)
        {
            log.LogWarningMethod(new(task), e, "Error disabling task {TaskName}", taskName);
        }

        var result = false;
        log.LogDebugMethod(new(task), "Deleting task {TaskName}", taskName);
        try
        {
            task.Folder.DeleteTask(taskName);
            result = true;
        }
        catch (Exception e)
        {
            log.LogErrorMethod(new(task), e, "Error deleting task {TaskName}", taskName);
        }

        return result;
    }

    public IEnumerable<Task> GetTasks()
    {
        foreach (var f in GetTaskFolders())
        {
            foreach (var t in f.Tasks)
            {
                log.LogTraceMethod(new(), "Found Task: {TaskPath}", t.GetPath());
                yield return t;
            }
        }
    }
    
    public Task? GetTask(TaskSchedulerPath path)
    {
        log.LogDebugMethod(new(path), "Getting Task: {Path}", path);
        return GetTasks().FirstOrDefault(task => path.Equals(task.GetPath()));
    }
    
    #endregion Task
    
    #region TaskFolder
    
    public IEnumerable<TaskFolder> GetTaskFolders()
    {
        log.LogTraceMethod(new(), "Getting task folders");
        var rootFolder = TaskService.RootFolder;
        if (rootFolder == null) throw new InvalidOperationException("Could not retrieve root folder");

        var queue = new Queue<TaskFolder>();
        var alreadyCheckedPaths = new HashSet<TaskSchedulerPath>();

        queue.Enqueue(rootFolder);
        while (queue.Count > 0)
        {
            var currentFolder = queue.Dequeue();
            if (!alreadyCheckedPaths.Add(currentFolder.GetPath())) continue;

            foreach (var subfolder in currentFolder.SubFolders)
            {
                queue.Enqueue(subfolder);
            }

            log.LogTraceMethod(new(), "Found TaskFolder: {TaskFolder}", currentFolder.GetPath());
            yield return currentFolder;
        }
    }
    
    public TaskFolder? GetTaskFolder(TaskSchedulerPath path)
    {
        log.LogDebugMethod(new(path), "Getting TaskFolder: {Path}", path);
        if (path.IsRoot) return TaskService.RootFolder;

        foreach (var folder in GetTaskFolders())
        {
            if (path.Equals(folder.GetPath()))
            {
                return folder;
            }
        }

        return null;
    }

    public TaskFolder CreateTaskFolder(TaskSchedulerPath path)
    {
        log.LogTraceMethod(new(path), "Attempting to create TaskFolder {Path}", path);
        var existingFolder = GetTaskFolder(path);
        if (existingFolder != null)
        {
            log.LogDebugMethod(new(path), "Skipping TaskFolder creation because folder already exists {Path}", path);
            return existingFolder;
        }

        var parent = path.Parent.CheckNotNull();
        var parentFolder = GetTaskFolder(parent);
        if (parentFolder == null)
        {
            log.LogTraceMethod(new(path), "Parent folder {ParentFolder} does not exist so creating it", parent);
            parentFolder = CreateTaskFolder(parent);
        }

        log.LogDebugMethod(new(path), "Creating TaskFolder: {Path}", path);
        return parentFolder.CreateFolder(path.Name)!;
    }
    
    #endregion TaskFolder
    
    #region IDisposable
    
    public void Dispose()
    {
        TaskService? ts;
        lock (locker)
        {
            ts = taskService;
            taskService = null;
        }

        if (ts != null)
        {
            log.LogDebugMethod(new(), nameof(Dispose) + "() called, disposing of {Type}", typeof(TaskService).FullNameFormatted());
            ts.DisposeSafely(log);
        }
    }
    
    #endregion IDisposable
}
