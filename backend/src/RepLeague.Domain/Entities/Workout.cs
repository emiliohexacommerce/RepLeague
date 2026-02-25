using RepLeague.Domain.Enums;

namespace RepLeague.Domain.Entities;

public class Workout
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public WorkoutType Type { get; set; }
    public bool IsPR { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<WorkoutExercise> Exercises { get; set; } = [];
    public WorkoutWod? Wod { get; set; }
}
