namespace RepLeague.Application.Features.Prs.DTOs;

public record PrDto(
    string Name,
    string Type,
    decimal? WeightKg,
    string? Duration,
    int? Sets,
    int? Reps,
    DateTime AchievedAt
);
