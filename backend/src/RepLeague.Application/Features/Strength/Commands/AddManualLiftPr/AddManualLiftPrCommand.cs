using MediatR;

namespace RepLeague.Application.Features.Strength.Commands.AddManualLiftPr;

public record ManualLiftPrDto(
    Guid Id,
    string ExerciseName,
    decimal WeightKg,
    string? Notes,
    DateOnly AchievedAt,
    DateTime CreatedAt
);

public record AddManualLiftPrCommand(
    Guid UserId,
    string ExerciseName,
    decimal WeightKg,
    string? Notes,
    DateOnly AchievedAt
) : IRequest<ManualLiftPrDto>;
