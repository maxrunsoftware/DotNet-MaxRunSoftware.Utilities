// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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

namespace MaxRunSoftware.Utilities.Windows;

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
