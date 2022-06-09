namespace LoopToIosConverter.Common;

public record LoopHabit(
    long? Id,
    string Name,
    string Description,
    string Question,
    int? FrequencyNumerator,
    int? FrequencyDenominator,
    LoopColor Color,
    int Position,
    int? ReminderHour,
    int? ReminderMinute,
    int? ReminderDays,
    bool IsArchived,
    LoopTargetType TargetType,
    decimal? TargetValue,
    string Unit,
    string? Uuid)
{

}

public enum LoopColor
{
    Red,
    DeepOrange,
    Orange,
    Amber,
    Yellow,
    Lime,
    LightGreen,
    Green,
    Teal,
    Cyan,
    LightBlue,
    Blue,
    Indigo,
    DeepPurple,
    Purple,
    Pink,
    Brown,
    DarkGrey,
    Grey,
    LightGrey
}

public enum LoopTargetType
{
    AtLeast,
    AtMost
}

public enum LoopHabitType
{
    YesNo,
    Numerical
}

[Flags]
public enum LoopWeekdays
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}

public sealed record LoopFrequency(int Numerator, int Denominator);