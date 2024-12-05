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
