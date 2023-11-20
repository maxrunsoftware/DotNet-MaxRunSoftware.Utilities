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

using System.Collections.Concurrent;
using System.Diagnostics;

namespace MaxRunSoftware.Utilities.Common;

public abstract class ConsumerThreadBase<T> : ThreadBase
{
    // ReSharper disable once StaticMemberInGenericType
    private readonly BlockingCollection<T> queue;
    private readonly object locker = new();
    private readonly CancellationTokenSource cancellation = new();
    private readonly bool cancelAfterCurrent = false;
    private volatile int itemsCompleted;
    public ConsumerThreadState ConsumerThreadState { get; private set; }

    public bool IsCancelled { get; private set; }

    public int ItemsCompleted => itemsCompleted;

    protected ConsumerThreadBase(BlockingCollection<T> queue, ILoggerProvider loggerProvider) : base(loggerProvider)
    {
        this.queue = queue.CheckNotNull(nameof(queue));
    }

    private bool ShouldExitWorkLoop()
    {
        lock (locker)
        {
            if (cancelAfterCurrent || IsDisposed || cancellation.IsCancellationRequested || IsCancelled || queue.IsCompleted)
            {
                log.LogDebug("Exiting work loop for thread {ThreadName}  cancelAfterCurrent={CancelAfterCurrent}, IsDisposed={IsDisposed}, IsCancellationRequested={IsCancellationRequested}, IsCancelled={IsCancelled}, IsCompleted={IsCompleted}", Name, cancelAfterCurrent, IsDisposed, cancellation.IsCancellationRequested, IsCancelled, queue.IsCompleted);
                Cancel();
                ConsumerThreadState = ConsumerThreadState.Stopped;
                return true;
            }

            return false;
        }
    }

    protected virtual void CancelInternal() { }

    /// <summary>
    /// Do some work on this item
    /// </summary>
    /// <param name="item">item</param>
    protected abstract void WorkConsume(T item);

    protected override void DisposeInternally()
    {
        Cancel();
        base.DisposeInternally();
    }

    protected override void Work()
    {
        var stopwatch = new Stopwatch();
        while (true)
        {
            if (ShouldExitWorkLoop()) return;

            T? t = default!;
            try
            {
                stopwatch.Restart();
                ConsumerThreadState = ConsumerThreadState.Waiting;
                var result = queue.TryTake(out t, -1, cancellation.Token);
                stopwatch.Stop();
                log.LogTrace("BlockingQueue.TryTake() time spent waiting {TimeSpentWaiting}s", stopwatch.Elapsed.ToStringTotalSeconds(3));

                if (!result)
                {
                    log.LogDebug("BlockingQueue.TryTake() returned false, cancelling thread {ThreadName}", Name);
                    Cancel();
                }
            }
            catch (OperationCanceledException)
            {
                // TODO: Figure out property nameof for logging
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                log.LogDebug($"Received {nameof(OperationCanceledException)}, cancelling thread {Name}");
                Cancel();
            }
            catch (Exception e)
            {
                log.LogWarning(e, "Received error requesting next item, cancelling thread {ThreadName}", Name);
                Cancel();
            }

            if (ShouldExitWorkLoop()) return;

            try
            {
                stopwatch.Restart();
                ConsumerThreadState = ConsumerThreadState.Working;
                if (t == null) throw new NullReferenceException("Should not happen");
                WorkConsume(t);

                //itemsCompleted++;
                Interlocked.Increment(ref itemsCompleted);

                stopwatch.Stop();
                var timeSpent = stopwatch.Elapsed;
                log.LogTrace("WorkConsume() time spent processing {TimeSpentProcessing}s", timeSpent.ToStringTotalSeconds(3));
            }
            catch (Exception e)
            {
                log.LogWarning(e, "Received error processing item {Item}", t.ToStringGuessFormat());
                Cancel();
            }
        }
    }

    public void Cancel()
    {
        lock (locker)
        {
            if (IsCancelled) return;

            IsCancelled = true;
        }

        try { cancellation.Cancel(); }
        catch (Exception e) { log.LogWarning(e, "CancellationTokenSource.Cancel() request threw exception"); }

        try { CancelInternal(); }
        catch (Exception e) { log.LogWarning(e, "CancelInternal() request threw exception"); }
    }
}
