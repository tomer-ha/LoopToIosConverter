namespace LoopToIosConverter.Common;

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
    public bool[] DaysOfWeek { get; } = new bool[7]; // Starts in Monday

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
        return $"{days}:{TimeOfDay:H:m}";
    }
}