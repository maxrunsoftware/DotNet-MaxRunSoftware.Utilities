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

using System.Collections.Concurrent;

namespace MaxRunSoftware.Utilities.Common;

public class TaskRunner<T>
{
    private readonly List<Task> tasks = new();
    private volatile int taskCountRunning;
    public int TaskCountRunning => taskCountRunning;
    private readonly BlockingCollection<Wrapper?> queue = new();
    private readonly ILogger log;
    private readonly CancellationToken cancellationToken;
    public int TaskCount { get; }
    private readonly Action<T> consume;
    

    private sealed class Wrapper(T item)
    {
        public T Item { get; } = item;
    }
    
    public TaskRunner(ILogger log, Action<T> consume, int? taskCount = null, CancellationToken cancellationToken = default, TaskFactory? taskFactory = null)
    {
        // https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/blockingcollection-overview
        
        this.log = log;
        this.consume = consume;
        TaskCount = taskCount ?? Environment.ProcessorCount;
        this.cancellationToken = cancellationToken;
        
        taskCountRunning = taskCount ?? Environment.ProcessorCount;
        for (var i = 0; i < TaskCount; i++)
        {
            Task t;
            if (taskFactory == null)
            {
                t = Task.Run(Consume, cancellationToken);
            }
            else
            {
                t = taskFactory.StartNew(Consume, cancellationToken);
            }

            tasks.Add(t);
            Interlocked.Increment(ref taskCountRunning);
        }
    }

    private void Consume()
    {
        // https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/blockingcollection-overview
        try
        {
            while (true)
            {
                if (queue.IsCompleted) break;
                if (cancellationToken.IsCancellationRequested) break;
                
                Wrapper? wrapper = null;
                try
                {
                    wrapper = queue.Take(cancellationToken);
                }
                catch (InvalidOperationException e)
                {
                    log.LogTrace(e, "{Message}", e.Message);
                }
                catch (TaskCanceledException e)
                {
                    log.LogTrace(e, "{Message}", e.Message);
                }

                if (wrapper == null) continue;
                
                var item = wrapper.Item;
                try
                {
                    consume(item);

                }
                catch (TaskCanceledException e)
                {
                    log.LogTrace(e, "Task cancelled for item: {Item}", item);
                }
                catch (Exception e)
                {
                    log.LogError(e, "Consume(item) threw exception for item: {Item}", item);
                }
            }
        }
        catch (Exception e)
        {
            log.LogError(e, "Task encountered exception and is shutting down");
        }
        finally
        {
            Interlocked.Decrement(ref taskCountRunning);
            if (taskCountRunning == 0)
            {
                foreach (var t in tasks)
                {
                    t.DisposeSafely(log);
                }
                tasks.Clear();
            }
        }

        
    }

    public void Add(T item) => queue.Add(new(item), cancellationToken);

    public void AddComplete()
    {
        queue.CompleteAdding();
    }
}
