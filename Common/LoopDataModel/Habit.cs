namespace LoopToIosConverter.Common;

public partial class Habit
{
    public Habit()
    {
        Repetitions = new HashSet<Repetition>();
    }

    public long Id { get; set; }
    public int Archived { get; set; }
    public LoopColor Color { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? FreqDen { get; set; }
    public int? FreqNum { get; set; }
    public int Highlight { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public long Position { get; set; }
    public LoopWeekdays ReminderDays { get; set; }
    public int? ReminderHour { get; set; }
    public int? ReminderMin { get; set; }
    public LoopHabitType Type { get; set; }
    public LoopTargetType TargetType { get; set; }
    public decimal TargetValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string? Uuid { get; set; }

    public virtual ICollection<Repetition> Repetitions { get; set; }
}