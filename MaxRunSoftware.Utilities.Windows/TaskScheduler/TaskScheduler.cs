// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Windows;

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

    public TaskScheduler(ILoggerProvider loggerProvider, string host, string? username, string? password, bool forceV1 = false)
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


    public Task TaskAdd(TaskSchedulerTask task)
    {
        var path = task.Path.CheckNotNull();
        var pathParent = path.Parent.CheckNotNull();
        var taskName = path.Name.CheckNotNullTrimmed();

        var dir = CreateTaskFolder(pathParent);

        var taskServiceTask = TaskService.NewTask().CheckNotNull();

        if (taskServiceTask.RegistrationInfo != null)
        {
            taskServiceTask.RegistrationInfo.Description = task.Description.TrimOrNull();
            taskServiceTask.RegistrationInfo.Documentation = task.Documentation.TrimOrNull();
        }

        var username = task.Username ??= Constant.Windows_User_System;
        var password = task.Password;

        var taskLogonType = TaskLogonType.Password;
        foreach (var sc in Constant.StringComparers)
        {
            if (username.In(sc, Constant.Windows_User_System, Constant.Windows_User_LocalService, Constant.Windows_User_NetworkService))
            {
                taskLogonType = TaskLogonType.ServiceAccount;
                username = username.ToUpper();
                password = null;
                break;
            }
        }

        taskServiceTask.Principal.LogonType = taskLogonType;
        taskServiceTask.Principal.UserId = username;
        taskServiceTask.Settings.Hidden = false;
        foreach (var trigger in task.Triggers)
        {
            foreach (var trigger2 in trigger.CreateTriggers())
            {
                log.LogTraceMethod(new(task), "Created trigger: {Trigger}", trigger2);
                taskServiceTask.Triggers.Add(trigger2);
            }
        }

        foreach (var filePath in task.FilePaths)
        {
            var execAction = new ExecAction(filePath, task.Arguments!, task.WorkingDirectory!);
            taskServiceTask.Actions.Add(execAction);
        }

        var t = dir.RegisterTaskDefinition(
            taskName,
            taskServiceTask,
            TaskCreation.CreateOrUpdate,
            username,
            password!,
            taskLogonType
        );

        return t!;
    }

    public bool TaskDelete(Task task)
    {
        var taskName = task.Name;
        if (task.State.In(TaskState.Queued, TaskState.Running, TaskState.Unknown))
        {
            try
            {
                log.LogDebugMethod(new(task), "Stopping task {TaskName} in state {TaskState}", taskName, task.State);
                task.Stop();
            }
            catch (Exception e)
            {
                log.LogWarningMethod(new(task), e, "Error stopping task {TaskName}", taskName);
            }
        }

        if (task.Enabled)
        {
            try
            {
                log.LogDebugMethod(new(task), "Disabling task {TaskName}", taskName);
                task.Enabled = false;
            }
            catch (Exception e)
            {
                log.LogWarningMethod(new(task), e, "Error disabling task {TaskName}", taskName);
            }
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

    public Task? GetTask(TaskSchedulerPath path)
    {
        log.LogDebugMethod(new(path), "Getting Task: {Path}", path);
        foreach (var task in GetTasks())
        {
            if (path.Equals(task.GetPath()))
            {
                return task;
            }
        }

        return null;
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
        log.LogTraceMethod(new(path), "Creating TaskFolder {Path}", path);
        var existingFolder = GetTaskFolder(path);
        if (existingFolder != null) return existingFolder;

        var parent = path.Parent.CheckNotNull();
        var parentFolder = GetTaskFolder(parent) ?? CreateTaskFolder(parent);
        log.LogDebugMethod(new(path), "Actually Creating TaskFolder: {Path}", path);
        return parentFolder.CreateFolder(path.Name)!;
    }

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
}
