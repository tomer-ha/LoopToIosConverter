using Microsoft.EntityFrameworkCore;

namespace LoopToIosConverter.Common;

public sealed class IosToLoopConverter
{
    private readonly string _csvFilePath;

    public IosToLoopConverter(string csvFilePath)
    {
        _csvFilePath = csvFilePath;
    }

    public async Task ConvertAsync(string dbFilePath, bool keepOrder = false)
    {
        var contextOptions = new DbContextOptionsBuilder<LoopHabitsBackupContext>().UseSqlite($"data source={dbFilePath}").Options;

        await using var loopDatabaseContext = new LoopHabitsBackupContext(contextOptions);
        await ConvertAsync(loopDatabaseContext, keepOrder);
    }

    public async Task ConvertAsync(LoopHabitsBackupContext loopDatabaseContext, bool keepOrder = false)
    {
        var loopHabitList = new List<Habit>();

        using var fileReader = new StreamReader(_csvFilePath);
        string? line;
        var lineIndex = 0;

        while ((line = await fileReader.ReadLineAsync()) != null)
        {
            var iosHabit = IosHabitFormatter.FromCsvString(line);
            var (frequencyNumerator, frequencyDenominator) = GetFrequency(iosHabit);
            var (reminderDays, reminderHour, reminderMinutes) = GetReminder(iosHabit);
            var (position, name) = keepOrder ? InferPosition(iosHabit) : (lineIndex, iosHabit.Title);
            var habit = new Habit
            {
                Id = iosHabit.Id,
                Color = GetColor(iosHabit),
                FreqNum = frequencyNumerator,
                FreqDen = frequencyDenominator,
                Name = name,
                Position = position,
                ReminderDays = reminderDays,
                ReminderHour = reminderHour,
                ReminderMin = reminderMinutes,
                Type = GetType(iosHabit),
                TargetType = LoopTargetType.AtLeast,
                TargetValue = GetTargetValue(iosHabit),
                Unit = GetUnit(iosHabit),
                Question = iosHabit.Question ?? string.Empty,
                Uuid = Guid.NewGuid().ToString("N"),
                Repetitions = GetRepetitions(iosHabit)
            };

            loopHabitList.Add(habit);
            lineIndex++;
        }

        await loopDatabaseContext.Database.EnsureCreatedAsync();
        await loopDatabaseContext.Database.ExecuteSqlRawAsync("PRAGMA user_version = 24");

        loopDatabaseContext.Habits.AddRange(loopHabitList);

        await loopDatabaseContext.SaveChangesAsync();
    }

    private static (int, string) InferPosition(IosHabit iosHabit)
    {
        var splitTitle = iosHabit.Title.Split('.', 2);

        if (splitTitle.Length != 2)
        {
            throw new FormatException();
        }

        if (!int.TryParse(splitTitle[0], out var result))
        {
            throw new FormatException();
        }

        return (result - 1, splitTitle[1]);
    }

    private static LoopColor GetColor(IosHabit iosHabit) =>
        iosHabit.Color switch
        {
            IosColor.Red => LoopColor.Red,
            IosColor.DeepOrange => LoopColor.DeepOrange,
            IosColor.Indigo => LoopColor.Indigo,
            IosColor.Pink => LoopColor.Pink,
            IosColor.DeepPurple => LoopColor.DeepPurple,
            IosColor.DarkBlue => LoopColor.LightGrey,
            IosColor.LightBlue => LoopColor.LightBlue,
            IosColor.Purple => LoopColor.Purple,
            IosColor.Blue => LoopColor.Blue,
            IosColor.Teal => LoopColor.Teal,
            IosColor.Cyan => LoopColor.Cyan,
            IosColor.LightGreen => LoopColor.LightGreen,
            IosColor.Green => LoopColor.Green,
            IosColor.DarkGreen => LoopColor.DarkGrey,
            IosColor.Lime => LoopColor.Lime,
            IosColor.Yellow => LoopColor.Yellow,
            IosColor.Orange => LoopColor.Orange,
            IosColor.DarkOrange => LoopColor.Amber,
            IosColor.Brown => LoopColor.Brown,
            IosColor.Grey => LoopColor.Grey,
            _ => throw new NotImplementedException(),
        };

    private static (LoopWeekdays, int?, int?) GetReminder(IosHabit iosHabit)
    {
        if (iosHabit.NotificationSettings is null)
        {
            return (0, null, null);
        }

        var weekdays = 0;
        var flag = 1;
        for (var i = 0; i < 7; i++)
        {
            if (iosHabit.NotificationSettings.DaysOfWeek[(i + 5) % 7])
            {
                weekdays |= flag;
            }
            flag <<= 1;
        }

        return ((LoopWeekdays)weekdays, iosHabit.NotificationSettings.TimeOfDay.Hour, iosHabit.NotificationSettings.TimeOfDay.Minute);
    }

    private static (int, int) GetFrequency(IosHabit iosHabit) =>
        iosHabit.Frequency.Type switch
        {
            IosFrequencyType.Daily => (1, 1),
            IosFrequencyType.EveryXDays => (1, iosHabit.Frequency.CustomizableInt),
            IosFrequencyType.XDaysPerWeek => (iosHabit.Frequency.CustomizableInt, 7),
            IosFrequencyType.OnWorkDaysOnly => (5, 7),
            IosFrequencyType.OnWeekendsOnly => (2, 7),
            _ => throw new NotImplementedException()
        };

    private static LoopHabitType GetType(IosHabit iosHabit) =>
        iosHabit switch
        {
            IosProgressiveHabit => LoopHabitType.Numerical,
            IosBasicHabit => LoopHabitType.YesNo,
            _ => throw new NotImplementedException()
        };

    private static ICollection<Repetition> GetRepetitions(IosHabit iosHabit) =>
        iosHabit switch
        {
            IosBasicHabit iosBasicHabit => iosBasicHabit.CompletedDates.
            Select(completedDate => new Repetition
            {
                Timestamp = new DateTimeOffset(completedDate.ToDateTime(new TimeOnly(), DateTimeKind.Utc)).ToUnixTimeMilliseconds(),
                Value = 2
            }).
            ToList(),
            IosProgressiveHabit iosProgressiveHabit => iosProgressiveHabit.DateAndValues.Select(dateAndValue => new Repetition
            {
                Timestamp = new DateTimeOffset(dateAndValue.Date.ToDateTime(new TimeOnly(), DateTimeKind.Utc)).ToUnixTimeMilliseconds(),
                Value = (int)(dateAndValue.Value * 1000)
            }).
            ToList(),
            _ => throw new NotImplementedException()
        };

    private static decimal GetTargetValue(IosHabit iosHabit) =>
        iosHabit switch
        {
            IosProgressiveHabit iosProgressiveHabit => iosProgressiveHabit.Target,
            _ => 0
        };

    private static string GetUnit(IosHabit iosHabit) =>
        iosHabit switch
        {
            IosProgressiveHabit iosProgressiveHabit => iosProgressiveHabit.Units,
            _ => string.Empty
        };
}