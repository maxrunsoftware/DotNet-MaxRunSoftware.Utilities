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

using Cronos;

namespace MaxRunSoftware.Utilities.Scheduler;

public class CronScheduler : IDisposable
{
    public CronScheduler(ILogger log)
    {
        //Thread t = new Thread()
    }
    
    public void Dispose() => throw new NotImplementedException();
}

public class CronSchedulerExpressionCollection
{
    public void Add(CronExpression cronExpression)
    {
        
    }
}

public class CronSchedulerThread(ILogger log, TimeProvider timeProvider) : ThreadBase(log)
{
    private readonly SortedDictionary<DateTimeOffset, ISet<CronExpression>> nextEvents = new();
    private DateTimeOffset timeLast = timeProvider.GetUtcNow();
    private static readonly TimeSpan forever = TimeSpan.MaxValue;
    private static readonly TimeSpan second = TimeSpan.FromSeconds(1);
    private readonly object lockerMonitor = new();
    
    private TimeSpan CalculateSleep(DateTimeOffset timeCurrent, DateTimeOffset timeNext)
    {
        if (timeNext == DateTimeOffset.MaxValue) return TimeSpan.MaxValue;
        var ticksDiff = (timeNext - timeCurrent).Ticks;
        if (ticksDiff <= 0L) return TimeSpan.Zero;
        const double ticksSleepMultiplier = 0.9d;
        var ticksSleep = ticksDiff * ticksSleepMultiplier;
        var timeSleep = new TimeSpan((long)ticksSleep);
        if (timeSleep < second) timeSleep = second;
        return timeSleep;
    }
    
    private DateTimeOffset GetNextTriggerTime()
    {
        return nextEvents.Count == 0 ? DateTimeOffset.MaxValue : nextEvents.Keys.First();
    }
    
    private void Sleep()
    {
        var timeCurrent = timeProvider.GetUtcNow();
        var timeNext = GetNextTriggerTime();
        var timeSleep = CalculateSleep(timeCurrent, timeNext);
        
        log.LogDebug("Sleeping...    Current:{TimeCurrent}    Next:{TimeNext}    Sleep:{TimeSleep}",
            timeCurrent.ToString("o"),
            timeNext.ToString("o"),
            timeSleep == forever ? "forever" : (Math.Floor(timeSleep.TotalHours) + timeSleep.ToString("'h 'm'm 's's'"))
        );
        
        if (timeSleep > TimeSpan.Zero)
        {
            Monitor.Wait(lockerMonitor, timeSleep);
        }
    }
    
    private ISet<CronExpression> RemoveAndRecalculate(DateTimeOffset entry, DateTimeOffset now)
    {
        var set = nextEvents[entry];
        nextEvents.Remove(entry);
        
        var future = now.AddHours(3);
        foreach (var cronExpression in set)
        {
            foreach (var nextFireTime in cronExpression.GetOccurrences(now, future, TimeZoneInfo.Utc, fromInclusive: true, toInclusive: true))
            {
                if (!nextEvents.TryGetValue(nextFireTime, out var nextFireTimeItems))
                {
                    nextFireTimeItems = new HashSet<CronExpression>();
                    nextEvents.Add(nextFireTime, nextFireTimeItems);
                }
                
                nextFireTimeItems.Add(cronExpression);
            }
        }
        
        return set;
    }
    
    protected override void Work()
    {
        while (!IsDisposed)
        {
            // https://stackoverflow.com/a/7448607
            lock (lockerMonitor)
            {
                if (IsDisposed) break;
                Sleep();
                if (IsDisposed) break;
                
                var timeCurrent = timeProvider.GetUtcNow();
                
                // find all entries that have elapsed
                var entriesExpired = new List<Tuple<DateTimeOffset, ISet<CronExpression>>>();
                foreach (var kvp in nextEvents)
                {
                    if (timeCurrent > kvp.Key)
                    {
                        entriesExpired.Add(Tuple.Create(kvp.Key, kvp.Value));
                    }
                    else
                    {
                        // since our dictionary is ordered, as soon as we encounter something later than current datetime we can break
                        break;
                    }
                }
                
                // remove entries that have elapsed and calculate their new next fire time
                foreach (var entryExpired in entriesExpired)
                {
                    // remove from schedule
                    nextEvents.Remove(entryExpired.Item1);
                    foreach (var ce in entryExpired.Item2)
                    {
                        ce.GetOccurrences(timeCurrent, timeCurrent.AddHours(1d), TimeZoneInfo.Utc, fromInclusive: true, toInclusive: true);
                    }
                }
                var timeNext = GetNextTriggerTime();
                if (timeCurrent > timeNext)
                {
                    var now = DateTimeOffset.Now;
                    var next = now + TimeSpan.FromHours(1);
                    var diff = next - now;
                    var diffs = new TimeSpan((long) (((double)diff.Ticks) / (5d/4d)));
                    
                    Console.WriteLine("now  : " + now);
                    Console.WriteLine("next : " + next);
                    Console.WriteLine("diff : " + diff);
                    Console.WriteLine("diffs: " + diffs);
                }
                
            }
        }
        
        
    }
    
}
