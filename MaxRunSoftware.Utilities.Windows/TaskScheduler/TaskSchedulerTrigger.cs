// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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
public interface ITaskSchedulerTrigger
{
    IEnumerable<Trigger> CreateTriggers();
}


[PublicAPI]
public abstract class TaskSchedulerTrigger : ITaskSchedulerTrigger
{
    public abstract IEnumerable<Trigger> CreateTriggers();

    public static ITaskSchedulerTrigger CreateFromTriggers(params Trigger[] triggers) => new TaskSchedulerTriggerWrapper(triggers);

    private sealed class TaskSchedulerTriggerWrapper : ITaskSchedulerTrigger
    {
        private readonly ImmutableArray<Trigger> triggers;
        public TaskSchedulerTriggerWrapper(params Trigger[] triggers) => this.triggers = triggers.OrEmpty().WhereNotNull().ToImmutableArray();
        public IEnumerable<Trigger> CreateTriggers() => triggers;
    }
}

[PublicAPI]
public class TaskSchedulerTriggerCron : TaskSchedulerTrigger
{
    public string? CronString { get; set; }
    public override IEnumerable<Trigger> CreateTriggers()
    {
        var cronString = CronString.CheckNotNullTrimmed();

        return Trigger.FromCronFormat(cronString)!;
    }
}

[PublicAPI]
public abstract class TaskSchedulerTriggerMinute : TaskSchedulerTrigger
{
    private int minute;
    public int Minute
    {
        get => minute.CheckMin(0).CheckMax(59);
        set => minute = value.CheckMin(0).CheckMax(59);
    }
}

[PublicAPI]
public abstract class TaskSchedulerTriggerHourMinute : TaskSchedulerTriggerMinute
{
    private int hour;
    public int Hour
    {
        get => hour.CheckMin(0).CheckMax(59);
        set => hour = value.CheckMin(0).CheckMax(59);
    }

    protected virtual DateTime StartBoundary
    {
        get
        {
            var startBoundary = DateTime.Today + TimeSpan.FromHours(Hour) + TimeSpan.FromMinutes(Minute);

            // Fix DST issue
            startBoundary = DateTime.SpecifyKind(startBoundary, DateTimeKind.Unspecified);

            return startBoundary;
        }
    }
}

[PublicAPI]
public class TaskSchedulerTriggerHourly : TaskSchedulerTriggerMinute
{
    public override IEnumerable<Trigger> CreateTriggers()
    {
        var interval = TimeSpan.FromMinutes(Minute);

        var now = DateTime.Now;

        // remove everything but hour
        var nowHour = now.AddMinutes(now.Minute * -1).AddSeconds(now.Second * -1).AddMilliseconds(now.Millisecond * -1);

        // if it is 6:45 right now but our nowHour (6:00) + interval (22) has already passed then add an hour (7:00)
        if (now > nowHour + interval) nowHour = nowHour.AddHours(1);

        // Fix DST issue
        nowHour = DateTime.SpecifyKind(nowHour, DateTimeKind.Unspecified);

        var trigger = new TimeTrigger();
        trigger.StartBoundary = nowHour;
        trigger.Repetition!.Interval = interval;
        return trigger.Yield();
    }
}

[PublicAPI]
public class TaskSchedulerTriggerDaily : TaskSchedulerTriggerHourMinute
{
    public override IEnumerable<Trigger> CreateTriggers()
    {
        var trigger = new DailyTrigger();
        trigger.StartBoundary = StartBoundary;
        return trigger.Yield();
    }
}

[PublicAPI]
public class TaskSchedulerTriggerMonthly : TaskSchedulerTriggerHourMinute
{
    public ISet<int> DaysOfMonth { get; } = new SortedSet<int>();

    public override IEnumerable<Trigger> CreateTriggers()
    {
        var trigger = new MonthlyTrigger(monthsOfYear: MonthsOfTheYear.AllMonths);
        trigger.StartBoundary = StartBoundary;
        trigger.DaysOfMonth = DaysOfMonth.ToArray();
        return trigger.Yield();
    }
}

[PublicAPI]
public class TaskSchedulerTriggerWeekly : TaskSchedulerTriggerHourMinute
{
    public ISet<DayOfWeek> DaysOfWeek { get; } = new SortedSet<DayOfWeek>();

    public override IEnumerable<Trigger> CreateTriggers()
    {
        DaysOfWeek.Count.CheckMin(1);
        DaysOfTheWeek? daysOfTheWeek = null;
        if (DaysOfWeek.Count == 7)
        {
            daysOfTheWeek = DaysOfTheWeek.AllDays;
        }
        else
        {
            static DaysOfTheWeek ConvertDayOfWeek(DayOfWeek dayOfWeek) => dayOfWeek switch
            {
                DayOfWeek.Sunday => DaysOfTheWeek.Sunday,
                DayOfWeek.Monday => DaysOfTheWeek.Monday,
                DayOfWeek.Tuesday => DaysOfTheWeek.Tuesday,
                DayOfWeek.Wednesday => DaysOfTheWeek.Wednesday,
                DayOfWeek.Thursday => DaysOfTheWeek.Thursday,
                DayOfWeek.Friday => DaysOfTheWeek.Friday,
                DayOfWeek.Saturday => DaysOfTheWeek.Saturday,
                _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, $"Invalid day of the week {dayOfWeek}"),
            };
            foreach (var dayOfWeek in DaysOfWeek)
            {
                var dow = ConvertDayOfWeek(dayOfWeek);
                if (daysOfTheWeek == null) daysOfTheWeek = dow;
                else daysOfTheWeek = daysOfTheWeek.Value | dow;
            }
        }

        var trigger = new WeeklyTrigger();
        trigger.StartBoundary = StartBoundary;
        trigger.DaysOfWeek = daysOfTheWeek.CheckNotNull();
        return trigger.Yield();
    }
}
