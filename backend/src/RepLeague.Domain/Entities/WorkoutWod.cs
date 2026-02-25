namespace RepLeague.Domain.Entities;

public class WorkoutWod
{
    public Guid Id { get; set; }
    public Guid WorkoutId { get; set; }
    public string WodName { get; set; } = string.Empty;
    public TimeSpan? Duration { get; set; }
    public int? Rounds { get; set; }
    public int? TotalReps { get; set; }

    public Workout Workout { get; set; } = null!;
}
