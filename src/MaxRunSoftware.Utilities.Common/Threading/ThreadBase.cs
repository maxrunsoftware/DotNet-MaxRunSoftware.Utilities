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

namespace MaxRunSoftware.Utilities.Common;

/// <summary>
/// Base class for implementing custom threads
/// </summary>
public abstract class ThreadBase : IDisposable
{
    private readonly Thread thread;
    //protected object Locker { get; } = new();

    private readonly SingleUse isStarted;
    public bool IsStarted => isStarted.IsUsed;
    private readonly SingleUse isDisposed;
    public bool IsDisposed => isDisposed.IsUsed;
    public string Name => thread.Name ?? nameDefault;
    private readonly string nameDefault;

    //public int DisposeTimeoutSeconds { get; set; } = 10;
    public bool IsRunning => thread.IsAlive;

    public ThreadState ThreadState => thread.ThreadState;
    public Exception? Exception { get; protected set; }

    protected readonly ILogger log;
    protected ThreadBase(ILogger log)
    {
        this.log = log;
        isStarted = new();
        isDisposed = new();
        thread = new(WorkPrivate);
        nameDefault = GetType().FullNameFormatted();
    }

    private void WorkPrivate()
    {
        try { Work(); }
        catch (Exception e) { Exception = e; }

        try { Dispose(); }
        catch (Exception e)
        {
            if (Exception == null) { Exception = e; }
            else
            {
                Console.Error.Write("Exception encountered while trying to dispose. ");
                Console.Error.WriteLine(e);
                log.LogError(e, "Exception encountered while trying to dispose");
            }
        }
    }

    /// <summary>
    /// Perform work. When this method returns Dispose() is automatically called and the thread shuts down.
    /// </summary>
    protected abstract void Work();

    /// <summary>
    /// Dispose of any resources used. Guaranteed to be only called once.
    /// </summary>
    protected virtual void DisposeInternally() { }

    protected void Join() => thread.Join();

    protected void Join(TimeSpan timeout) => thread.Join(timeout);

    

    public void Start(bool isBackgroundThread = true, string? name = null)
    {
        if (IsDisposed) throw new ObjectDisposedException(GetType().FullNameFormatted());

        if (!isStarted.TryUse()) throw new InvalidOperationException("Start() already called");

        thread.IsBackground = isBackgroundThread;
        thread.Name = name ?? nameDefault;
        log.LogDebugMethod(new(isBackgroundThread, name), "Starting thread '{ThreadName}' with IsBackground={ThreadIsBackground} of type '{FullNameFormatted}'", thread.Name, thread.IsBackground, GetType().FullNameFormatted());

        thread.Start();
    }
    
    private void ReleaseUnmanagedResources()
    {
        if (!isDisposed.TryUse()) return;

        log.LogDebugMethod(new(), "Disposing thread '{ThreadName}' with IsBackground={ThreadIsBackground} of type '{Type}'", thread.Name, thread.IsBackground, GetType().FullNameFormatted());

        DisposeInternally();
    }
    
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
    
    ~ThreadBase() => ReleaseUnmanagedResources();
}
