namespace LoopToIosConverter.Common;

public partial class Repetition
{
    public long Id { get; set; }
    public long Habit { get; set; }
    public long Timestamp { get; set; }
    public int Value { get; set; }

    public virtual Habit HabitNavigation { get; set; } = null!;
}