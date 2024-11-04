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
public class TaskSchedulerPath : ComparableBase<TaskSchedulerPath, TaskSchedulerPathComparer>
{
    // ReSharper disable once InconsistentNaming
    private static readonly string PATH_DELIMITER_TaskScheduler = PATH_DELIMITER_TaskScheduler_Build();
    private static string PATH_DELIMITER_TaskScheduler_Build()
    {
        var delim = @"\";
        try
        {
            var ts = typeof(TaskFolder);
            var f = ts.GetField("rootString", BindingFlags.NonPublic | BindingFlags.Static)
                    ?? ts.GetField("rootString", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (f != null)
            {
                var ds = f.GetValue(null)?.ToString();
                if (ds != null) delim = ds;
            }
        }
        catch (Exception)
        {
            // ignored
        }
        
        return delim;
    }

    public static readonly char PATH_DELIMITER = '/';
    private static readonly ImmutableArray<char> PATH_DELIMITERS = ['/', '\\'];

    public static readonly TaskSchedulerPath ROOT = new(string.Empty);
    public IReadOnlyList<string> PathParts { get; }
    public string Path { get; }
    public string GetWin32TaskSchedulerPath() => Path.Replace(PATH_DELIMITER.ToString(), PATH_DELIMITER_TaskScheduler);
    public string Name { get; }
    public TaskSchedulerPath? Parent { get; }
    public bool IsRoot => Parent == null;
    public TaskSchedulerPath(Task task) : this(task.Folder.Path + PATH_DELIMITER + task.Name) { }
    public TaskSchedulerPath(TaskFolder folder) : this(folder.Path) { }
    public TaskSchedulerPath(string? path) : base(TaskSchedulerPathComparer.Default)
    {
        var pathParts = PathParse(path);
        if (pathParts.Count == 0)
        {
            PathParts = ImmutableArray<string>.Empty;
            Name = PATH_DELIMITER.ToString();
            Parent = null;
            Path = PATH_DELIMITER.ToString();
        }
        else
        {
            PathParts = pathParts.ToImmutableArray();
            Name = pathParts.PopTail();
            Parent = pathParts.Count == 0 ? ROOT : new(pathParts.ToStringDelimited(PATH_DELIMITER));
            Path = PATH_DELIMITER + PathParts.ToStringDelimited(PATH_DELIMITER);
        }
        
        return;
        
        static List<string> PathParse(string? path)
        {
            if (path == null || string.IsNullOrWhiteSpace(path)) return new();
            var pathDelimiterCorrect = PATH_DELIMITER.ToString();
            foreach (var pathDelimiter in PATH_DELIMITERS.Select(o => o.ToString()))
            {
                var doublePathDelimiter = pathDelimiter + pathDelimiter;
                while (path.Contains(doublePathDelimiter))
                {
                    path = path.Replace(doublePathDelimiter, pathDelimiter);
                }
                
                if (pathDelimiter != pathDelimiterCorrect) path = path.Replace(pathDelimiter, pathDelimiterCorrect);
            }
            
            return path.Split(pathDelimiterCorrect).WhereNotNull().Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
        }
    }

    public override string ToString() => Path;
}

public sealed class TaskSchedulerPathComparer : ComparerBaseDefault<TaskSchedulerPath, TaskSchedulerPathComparer>
{
    protected override bool Equals_Internal(TaskSchedulerPath x, TaskSchedulerPath y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);

    protected override int GetHashCode_Internal(TaskSchedulerPath obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Path);

    protected override int Compare_Internal(TaskSchedulerPath x, TaskSchedulerPath y)
    {
        var xPathParts = x.PathParts;
        var yPathParts = y.PathParts;

        var xLen = xPathParts.Count;
        var yLen = yPathParts.Count;
            
        var len = Math.Min(xLen, yLen);
        for (var i = 0; i < len; i++)
        {
            var c = xPathParts[i].CompareToOrdinalIgnoreCaseThenOrdinal(yPathParts[i]);
            if (c != 0) return c;
        }
            
        return xLen.CompareTo(yLen);
    }
}

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
