namespace LoopToIosConverter.Common;

public sealed class Repetition
{
    public long Id { get; set; }
    public long Habit { get; set; }
    public long Timestamp { get; set; }
    public decimal Value { get; set; }

    public Habit HabitNavigation { get; set; } = null!;
}