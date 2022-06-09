using Microsoft.EntityFrameworkCore;

namespace LoopToIosConverter.Common;

public sealed class LoopToIosConverter : IDisposable
{
    private LoopHabitsBackupContext _loopDatabaseContext;

    public LoopToIosConverter(string loopDatabasePath)
    {
        var contextOptions = new DbContextOptionsBuilder<LoopHabitsBackupContext>().UseSqlite($"data source=\"{loopDatabasePath}\"").Options;

        _loopDatabaseContext = new LoopHabitsBackupContext(contextOptions);
    }

    public void Dispose() => 
        _loopDatabaseContext.Dispose();

    public async Task ConvertAsync(string csvPath)
    {
        var iosHabitList = new List<IosHabit>();

        foreach (var loopHabit in _loopDatabaseContext.Habits)
        {
            IosHabit iosHabit;
            if (loopHabit.Type == LoopHabitType.YesNo)
            {
                iosHabit = new IosBasicHabit(
                    (int)loopHabit.Id, loopHabit.Name, loopHabit.Question, GetColor(loopHabit), GetCreationTime(loopHabit), GetFrequency(loopHabit), GetNotificationSettings(loopHabit), GetCompletionDates(loopHabit));
            }
            else
            {
                if (loopHabit.TargetType == LoopTargetType.AtMost) throw new FormatException($"{nameof(LoopTargetType.AtMost)} habits are not supported");
                iosHabit = new IosProgressiveHabit((int)loopHabit.Id, loopHabit.Name, loopHabit.Question, GetColor(loopHabit), GetCreationTime(loopHabit), GetFrequency(loopHabit), GetNotificationSettings(loopHabit), loopHabit.TargetValue, loopHabit.Unit, GetDateToValues(loopHabit));
            }

            iosHabitList.Add(iosHabit);
        }

        using var file = new StreamWriter(csvPath);
        foreach (var habit in iosHabitList)
        {
            await file.WriteLineAsync(IosHabitFormatter.ToCsvString(habit));
        }
    }

    private static IosColor GetColor(Habit loopHabit) =>  // TODO
        IosColor.Red;

    private static DateTime GetCreationTime(Habit loopHabit) => // TODO: habit with no repetitions
        GetCompletionDates(loopHabit).Min().ToDateTime(new TimeOnly()).Subtract(TimeSpan.FromDays(1));

    private static IosFrequency GetFrequency(Habit loopHabit) =>
        (loopHabit.FreqNum, loopHabit.FreqDen) switch
        {
            (1, 1) => new IosFrequency(IosFrequencyType.Daily),
            (var daysInWeek, 7) when daysInWeek is not null => new IosFrequency(IosFrequencyType.XDaysPerWeek, daysInWeek.Value),
            (1, var period) when period is not null => new IosFrequency(IosFrequencyType.EveryXDays, period.Value),
            _ => throw new FormatException()
        };

    private static IosNotificationSettings? GetNotificationSettings(Habit loopHabit)
    {
        var reminderDaysIntValue = (int)loopHabit.ReminderDays;
        if (reminderDaysIntValue > 0 && loopHabit.ReminderHour.HasValue && loopHabit.ReminderMin.HasValue)
        {
            var days = new bool[7];
            for (var i = 0; i < 7; i++)
            {
                var flag = 1 << i;
                days[i] = (reminderDaysIntValue & flag) > 0;
            }

            return new IosNotificationSettings(new TimeOnly(loopHabit.ReminderHour.Value, loopHabit.ReminderMin.Value), days);
        }

        return null;
    }

    private static IReadOnlyCollection<IosDateAndValue> GetDateToValues(Habit loopHabit) =>
        loopHabit.Repetitions.
        Select(repetitionLong => new IosDateAndValue(
            DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(repetitionLong.Timestamp).UtcDateTime),
            repetitionLong.Value)).
        ToList();

    private static IReadOnlyCollection<DateOnly> GetCompletionDates(Habit loopHabit) =>
        GetDateToValues(loopHabit).
        Select(dateAndValue => dateAndValue.Date).
        ToList();
}