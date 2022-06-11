using System.Globalization;

namespace LoopToIosConverter.Common;

public static class IosHabitFormatter
{
    private const string _creationTimeFormat = "yyyy-MM-d-H:m:ss.ffff";
    private const string _repetitionDateFormat = "yyyyMMdd";

    public static IosHabit FromCsvString(string csvLine)
    {
        var split = csvLine.Split(',');
        if (split.Length != 12) throw new FormatException();

        var id = int.Parse(split[0]);
        var title = split[1].Trim('"');
        var question = split[3].Trim('"');
        var color = (IosColor)int.Parse(split[4]);
        var createdTime = DateTime.ParseExact(split[5], _creationTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        var frequency = new IosFrequency(split[6].Trim('"'));
        var unparsedNotificationSettings = split[7].Trim('"');
        var notificationSettings = string.IsNullOrEmpty(unparsedNotificationSettings) ? null : new IosNotificationSettings(unparsedNotificationSettings);
        var dateAndValues = ParseDateAndValues(split[10]);

        return (split[2] == "0") ? CreateBasicHabit() : CreateProgressiveHabit();

        IosBasicHabit CreateBasicHabit() =>
            new(id, title, question, color, createdTime, frequency, notificationSettings, dateAndValues.Select(dateAndValue => dateAndValue.Date).ToList());

        IosProgressiveHabit CreateProgressiveHabit() =>
            new(id, title, question, color, createdTime, frequency, notificationSettings, decimal.Parse(split[8]), split[9], dateAndValues);
    }

    public static string ToCsvString(IosHabit habit)
    {
        var baseString = string.Join(",", 
            habit.Id.ToString(),
            Quoted(habit.Title),
            habit is IosBasicHabit ? "0" : "1",
            Quoted(habit.Question),
            habit.Color.ToString("D"),
            habit.CreationTime.ToString(_creationTimeFormat),
            Quoted(habit.Frequency.ToCsvString()),
            Quoted(habit.NotificationSettings?.ToCsvString() ?? string.Empty),
            habit.Target.ToString("F1"));

        var suffixString = habit switch
        {
            IosBasicHabit basicHabit => GetBasicSuffix(basicHabit),
            IosProgressiveHabit progressiveHabit => GetProgressiveSuffix(progressiveHabit),
            _ => throw new NotImplementedException()
        };

        return string.Join(",", baseString, suffixString);

        static string Quoted(string? str) => 
            "\"" + str + "\"";

        static string GetBasicSuffix(IosBasicHabit basicHabit) =>
            string.Join(",", string.Empty, basicHabit.CompletedDates.ToIosDateAndValue().ToCsvFormat(), "0");

        static string GetProgressiveSuffix(IosProgressiveHabit progressiveHabit) =>
            string.Join(",", progressiveHabit.Units, progressiveHabit.DateAndValues.ToCsvFormat(), "1");
    }

    private static IReadOnlyCollection<IosDateAndValue> ParseDateAndValues(string csvFormat)
    {
        if (string.IsNullOrEmpty(csvFormat))
        {
            return Array.Empty<IosDateAndValue>();
        }

        return csvFormat.Split('-').
            Select(csvDateAndValue => csvDateAndValue.Split(':')).
            Select(splitCsvDateAndValue => new IosDateAndValue(
                DateOnly.ParseExact(splitCsvDateAndValue[0], _repetitionDateFormat, CultureInfo.InvariantCulture),
                decimal.Parse(splitCsvDateAndValue[1]))).
            ToList();
    }

    private static string ToCsvFormat(this IReadOnlyCollection<IosDateAndValue> dateAndValues) =>
        string.Join("-", dateAndValues.Select(dateAndValue => $"{dateAndValue.Date.ToString(_repetitionDateFormat)}:{dateAndValue.Value}").ToArray());

    private static IReadOnlyCollection<IosDateAndValue> ToIosDateAndValue(this IReadOnlyCollection<DateOnly> completedDates) =>
        completedDates.Select(date => new IosDateAndValue(date, 1)).ToList();
}

public abstract record IosHabit(
    int Id,
    string Title,
    string? Question,
    IosColor Color,
    DateTime CreationTime,
    IosFrequency Frequency,
    IosNotificationSettings? NotificationSettings,
    decimal Target)
{
}

public sealed record IosBasicHabit(
    int Id,
    string Title,
    string? Question,
    IosColor Color,
    DateTime CreationTime,
    IosFrequency Frequency,
    IosNotificationSettings? NotificationSettings,
    IReadOnlyCollection<DateOnly> CompletedDates) : IosHabit(
        Id, Title, Question, Color, CreationTime, Frequency, NotificationSettings, 1)
{

}

public sealed record IosProgressiveHabit(
    int Id,
    string Title,
    string? Question,
    IosColor Color,
    DateTime CreationTime,
    IosFrequency Frequency,
    IosNotificationSettings? NotificationSettings,
    decimal Target,
    string Units,
    IReadOnlyCollection<IosDateAndValue> DateAndValues) : IosHabit(
        Id, Title, Question, Color, CreationTime, Frequency, NotificationSettings, Target)
{

}

public sealed record IosDateAndValue(DateOnly Date, decimal Value);

public enum IosColor
{
    Red,
    DeepOrange,
    Indigo,
    Pink,
    DeepPurple,

    DarkBlue,
    LightBlue,
    Purple,
    Blue,
    Teal,
    
    Cyan,
    LightGreen,
    Green,
    DarkGreen,
    Lime,

    Yellow,
    Orange,
    DarkOrange,
    Brown,
    Grey
}

public class IosFrequency
{
    public IosFrequencyType Type { get; }
    public int CustomizableInt { get; }

    public IosFrequency(string csvFormat)
    {
        if (int.TryParse(csvFormat, out var result))
        {
            Type = (IosFrequencyType)result;
        }
        else
        {
            var split = csvFormat.Split(':');
            if (split.Length != 2) throw new FormatException();
            var part1 = int.Parse(split[0]);
            var part2 = int.Parse(split[1]);

            Type = (IosFrequencyType)part1;
            CustomizableInt = part2;
        }
    }

    public IosFrequency(IosFrequencyType type, int customizableInt = 0)
    {
        Type = type;
        CustomizableInt = customizableInt;
    }

    public string ToCsvString() =>
        Type switch
        {
            IosFrequencyType.Daily => "0",
            IosFrequencyType.EveryXDays => $"1:{CustomizableInt}",
            IosFrequencyType.XDaysPerWeek => $"2:{CustomizableInt}",
            IosFrequencyType.OnWorkDaysOnly => "3",
            IosFrequencyType.OnWeekendsOnly => "4",
            _ => throw new NotImplementedException()
        };
}

public enum IosFrequencyType
{
    Daily = 0,
    EveryXDays = 1,
    XDaysPerWeek = 2,
    OnWorkDaysOnly = 3,
    OnWeekendsOnly = 4
}

public class IosNotificationSettings
{
    public TimeOnly TimeOfDay { get; }
    public bool[] DaysOfWeek { get; } = new bool[7];

    public IosNotificationSettings(string csvFormat)
    {
        var split = csvFormat.Split(":");
        if (split.Length != 3) throw new FormatException();
        if (split[0].Length != 7) throw new FormatException();

        for (int i = 0; i < 7; i++)
        {
            DaysOfWeek[i] = split[0][i] == '1';
        }

        TimeOfDay = TimeOnly.Parse($"{split[1]}:{split[2]}");
    }

    public IosNotificationSettings(TimeOnly timeOfDay, bool[] daysOfWeek)
    {
        TimeOfDay = timeOfDay;
        DaysOfWeek = daysOfWeek;
    }

    public string ToCsvString()
    {
        var days = new string(DaysOfWeek.Select(day => day ? '1' : '0').ToArray());
        return $"{days}:{TimeOfDay.ToString("H:m")}";
    }
}