using MediatR;
using RepLeague.Application.Features.Strength.Commands.AddManualLiftPr;

namespace RepLeague.Application.Features.Strength.Queries.GetManualLiftPrs;

/// <summary>
/// Returns manual PRs grouped by exercise.
/// Each group contains:
///   - The current best (highest WeightKg)
///   - Full history sorted by AchievedAt descending
/// </summary>
public record ManualLiftPrHistoryItem(
    Guid Id,
    decimal WeightKg,
    string? Notes,
    DateOnly AchievedAt
);

public record ManualLiftPrGroupDto(
    string ExerciseName,
    decimal BestWeightKg,
    DateOnly BestAchievedAt,
    IReadOnlyList<ManualLiftPrHistoryItem> History
);

public record GetManualLiftPrsQuery(Guid UserId) : IRequest<List<ManualLiftPrGroupDto>>;
