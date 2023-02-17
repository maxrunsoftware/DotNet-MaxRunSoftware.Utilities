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

public static class TaskSchedulerExtensions
{
    public static TaskSchedulerPath GetPath(this Task task) => new(task);

    public static TaskSchedulerPath GetPath(this TaskFolder folder) => new(folder);

    public static DaysOfTheWeek ToDaysOfTheWeek(this DayOfWeek dayOfWeek) =>
        dayOfWeek switch
        {
            DayOfWeek.Sunday => DaysOfTheWeek.Sunday,
            DayOfWeek.Monday => DaysOfTheWeek.Monday,
            DayOfWeek.Tuesday => DaysOfTheWeek.Tuesday,
            DayOfWeek.Wednesday => DaysOfTheWeek.Wednesday,
            DayOfWeek.Thursday => DaysOfTheWeek.Thursday,
            DayOfWeek.Friday => DaysOfTheWeek.Friday,
            DayOfWeek.Saturday => DaysOfTheWeek.Saturday,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
        };

    public static bool TaskDelete(this TaskScheduler taskScheduler, TaskSchedulerPath path)
    {
        var t = taskScheduler.GetTask(path);
        if (t == null) return false;
        return taskScheduler.TaskDelete(t);
    }

    public static Dictionary<TaskSchedulerPath, List<Task>> GetTasksByFolder(this TaskScheduler taskScheduler)
    {
        var d = new Dictionary<TaskSchedulerPath, List<Task>>();
        foreach (var taskFolder in taskScheduler.GetTaskFolders())
        {
            d.AddToList(taskFolder.GetPath(), taskFolder.Tasks.ToArray());
        }

        return d;
    }

    public static Task? GetTask(this TaskScheduler taskScheduler, string path) => taskScheduler.GetTask(new(path));
    public static TaskFolder? GetTaskFolder(this TaskScheduler taskScheduler, string path) => taskScheduler.GetTaskFolder(new(path));
}
