namespace MaxRunSoftware.Utilities.Common;

public sealed class MutexLockTimeoutException(
    string mutexName, 
    TimeSpan timeout
    ) : WaitHandleCannotBeOpenedException($"Failed to acquire mutex [{mutexName}] after waiting {timeout.ToStringTotalSeconds(3)}s")
{
    public string MutexName { get; } = mutexName;
    public TimeSpan Timeout { get; } = timeout;
}
