namespace RepLeague.Domain.Entities;

public class WodResultAmrap
{
    public Guid WodEntryId { get; set; }
    public int RoundsCompleted { get; set; }
    public int ExtraReps { get; set; }

    public WodEntry WodEntry { get; set; } = null!;
}
