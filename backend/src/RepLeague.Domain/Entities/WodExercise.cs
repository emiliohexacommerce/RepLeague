namespace RepLeague.Domain.Entities;

public class WodExercise
{
    public Guid Id { get; set; }
    public Guid WodEntryId { get; set; }
    public int OrderIndex { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;  // barbell|kb|bodyweight|gymnastic|cardio|other
    public decimal? LoadValue { get; set; }
    public string? LoadUnit { get; set; }                      // kg|lb|cal|m|reps
    public int? Reps { get; set; }
    public string? Notes { get; set; }

    public WodEntry WodEntry { get; set; } = null!;
}
