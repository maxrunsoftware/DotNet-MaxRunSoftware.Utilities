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

[PublicAPI]
public class TaskSchedulerPath : ComparableClass<TaskSchedulerPath, TaskSchedulerPath.Comparer>
{

    // ReSharper disable once InconsistentNaming
    private static readonly string PATH_DELIMITER_TaskScheduler = PATH_DELIMITER_TaskScheduler_Build();
    private static string PATH_DELIMITER_TaskScheduler_Build()
    {
        var delim = @"\";
        try
        {
            var ts = typeof(TaskFolder).ToTypeSlim();
            var f = ts.GetFieldSlim("rootString", BindingFlags.NonPublic | BindingFlags.Static)
                    ?? ts.GetFieldSlim("rootString", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (f != null)
            {
                var ds = f.GetValue(null)?.ToString();
                if (ds != null) delim = ds;
            }
        } catch (Exception) {}

        return delim;
    }

    public static readonly char PATH_DELIMITER = '/';
    private static readonly ImmutableArray<char> PATH_DELIMITERS = ImmutableArray.Create('/', '\\');

    public static readonly TaskSchedulerPath ROOT = new(string.Empty);
    public IReadOnlyList<string> PathParts { get; }
    public string Path { get; }
    public string GetWin32TaskSchedulerPath() => Path.Replace(PATH_DELIMITER.ToString(), PATH_DELIMITER_TaskScheduler);
    public string Name { get; }
    public TaskSchedulerPath? Parent { get; }
    public bool IsRoot => Parent == null;
    public TaskSchedulerPath(Task task) : this(task.Folder.Path + PATH_DELIMITER + task.Name) { }
    public TaskSchedulerPath(TaskFolder folder) : this(folder.Path) { }
    public TaskSchedulerPath(string? path) : base(Comparer.Instance)
    {
        static List<string> PathParse(string? path)
        {
            if (path == null || string.IsNullOrWhiteSpace(path)) return new();
            var pathDelimiterCorrect = PATH_DELIMITER.ToString();
            foreach (var pathDelimiter in PATH_DELIMITERS.Select(o => o.ToString()))
            {
                var doublePathDelimiter = pathDelimiter + pathDelimiter;
                while (path.Contains(doublePathDelimiter)) path = path.Replace(doublePathDelimiter, pathDelimiter);
                if (pathDelimiter != pathDelimiterCorrect) path = path.Replace(pathDelimiter, pathDelimiterCorrect);
            }

            return path.Split(pathDelimiterCorrect).WhereNotNull().Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
        }

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
    }

    public override string ToString() => Path;

    public sealed class Comparer : ComparerBaseClass<TaskSchedulerPath>
    {
        public static Comparer Instance { get; } = new();
        protected override bool EqualsInternal(TaskSchedulerPath x, TaskSchedulerPath y) => EqualsStruct(x.GetHashCode(), y.GetHashCode()) && EqualsOrdinalIgnoreCase(x.Path, y.Path);
        protected override int GetHashCodeInternal(TaskSchedulerPath obj) => HashOrdinalIgnoreCase(obj.Path);
        protected override int CompareInternal(TaskSchedulerPath x, TaskSchedulerPath y) => CompareOrdinalIgnoreCaseThenOrdinal(x.PathParts, y.PathParts) ?? 0;
    }
}
