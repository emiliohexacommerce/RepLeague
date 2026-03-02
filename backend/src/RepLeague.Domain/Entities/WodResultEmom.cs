namespace RepLeague.Domain.Entities;

public class WodResultEmom
{
    public Guid WodEntryId { get; set; }
    public int TotalMinutes { get; set; }
    public int IntervalsDone { get; set; }

    public WodEntry WodEntry { get; set; } = null!;
}
