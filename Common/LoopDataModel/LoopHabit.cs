namespace LoopToIosConverter.Common;

public enum LoopColor : long
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

public enum LoopTargetType : long
{
    AtLeast,
    AtMost
}

public enum LoopHabitType : long
{
    YesNo,
    Numerical
}

[Flags]
public enum LoopWeekdays : long
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