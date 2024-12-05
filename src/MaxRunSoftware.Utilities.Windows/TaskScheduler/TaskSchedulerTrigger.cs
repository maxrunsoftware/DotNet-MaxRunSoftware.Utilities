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

using Microsoft.Win32.TaskScheduler;

namespace MaxRunSoftware.Utilities.Windows;

[PublicAPI]
public static class TaskSchedulerTrigger
{
    private static DateTime GetStartBoundary(int hour, int minute)
    {
        var startBoundary = DateTime.Today + TimeSpan.FromHours(hour) + TimeSpan.FromMinutes(minute);

        // Fix DST issue
        startBoundary = DateTime.SpecifyKind(startBoundary, DateTimeKind.Unspecified);

        return startBoundary;
    }

    public static IEnumerable<Trigger> CreateCron(string cron)
    {
        var cronString = cron.CheckNotNullTrimmed();

        return Trigger.FromCronFormat(cronString)!;
    }

    public static IEnumerable<Trigger> CreateHourly(int minute)
    {
        minute.CheckMin(0).CheckMax(59);

        var interval = TimeSpan.FromMinutes(minute);

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

    public static IEnumerable<Trigger> CreateDaily(int hour, int minute)
    {
        hour.CheckMin(0).CheckMax(23);
        minute.CheckMin(0).CheckMax(59);

        var trigger = new DailyTrigger();
        trigger.StartBoundary = GetStartBoundary(hour, minute);
        return trigger.Yield();
    }

    public static IEnumerable<Trigger> CreateWeekly(IEnumerable<DayOfWeek> daysOfWeek, int hour, int minute)
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

        hour.CheckMin(0).CheckMax(23);
        minute.CheckMin(0).CheckMax(59);

        var daysOfWeekSet = new SortedSet<DayOfWeek>(daysOfWeek);
        if (daysOfWeekSet.Count < 1) return Enumerable.Empty<Trigger>();

        DaysOfTheWeek? daysOfTheWeek = null;
        if (daysOfWeekSet.Count == 7)
        {
            daysOfTheWeek = DaysOfTheWeek.AllDays;
        }
        else
        {
            foreach (var dow in daysOfWeekSet.Select(ConvertDayOfWeek))
            {
                if (daysOfTheWeek == null)
                {
                    daysOfTheWeek = dow;
                }
                else
                {
                    daysOfTheWeek = daysOfTheWeek.Value | dow;
                }
            }
        }

        var trigger = new WeeklyTrigger();
        trigger.StartBoundary = GetStartBoundary(hour, minute);
        trigger.DaysOfWeek = daysOfTheWeek.CheckNotNull();
        return trigger.Yield();
    }
}
