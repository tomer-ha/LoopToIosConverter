using Microsoft.EntityFrameworkCore;

namespace LoopToIosConverter.Common;

public partial class LoopHabitsBackupContext : DbContext
{
    public LoopHabitsBackupContext()
    {
    }

    public LoopHabitsBackupContext(DbContextOptions<LoopHabitsBackupContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Habit> Habits { get; set; } = null!;
    public virtual DbSet<Repetition> Repetitions { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
            optionsBuilder.UseSqlite("data source=\"C:\\Users\\tomer\\Downloads\\Loop Habits Backup 2022-03-26 165755.db\"");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habit>(entity =>
        {
            entity.Property(e => e.Archived).HasColumnName("archived");

            entity.Property(e => e.Color).HasColumnName("color");

            entity.Property(e => e.Description).HasColumnName("description");

            entity.Property(e => e.FreqDen).HasColumnName("freq_den");

            entity.Property(e => e.FreqNum).HasColumnName("freq_num");

            entity.Property(e => e.Highlight).HasColumnName("highlight");

            entity.Property(e => e.Name).HasColumnName("name");

            entity.Property(e => e.Position).HasColumnName("position");

            entity.Property(e => e.Question)
                .HasColumnType("text")
                .HasColumnName("question");

            entity.Property(e => e.ReminderDays).HasColumnName("reminder_days");

            entity.Property(e => e.ReminderHour).HasColumnName("reminder_hour");

            entity.Property(e => e.ReminderMin).HasColumnName("reminder_min");

            entity.Property(e => e.TargetType)
                .HasColumnType("integer")
                .HasColumnName("target_type");

            entity.Property(e => e.TargetValue)
                .HasColumnType("real")
                .HasColumnName("target_value");

            entity.Property(e => e.Type)
                .HasColumnType("integer")
                .HasColumnName("type");

            entity.Property(e => e.Unit)
                .HasColumnType("text")
                .HasColumnName("unit");

            entity.Property(e => e.Uuid)
                .HasColumnType("text")
                .HasColumnName("uuid");
        });

        modelBuilder.Entity<Repetition>(entity =>
        {
            entity.HasIndex(e => new { e.Habit, e.Timestamp }, "idx_repetitions_habit_timestamp")
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("integer")
                .HasColumnName("id");

            entity.Property(e => e.Habit)
                .HasColumnType("integer")
                .HasColumnName("habit");

            entity.Property(e => e.Timestamp)
                .HasColumnType("integer")
                .HasColumnName("timestamp");

            entity.Property(e => e.Value)
                .HasColumnType("integer")
                .HasColumnName("value");

            entity.HasOne(d => d.HabitNavigation)
                .WithMany(p => p.Repetitions)
                .HasForeignKey(d => d.Habit)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
