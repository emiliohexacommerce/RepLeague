using MediatR;

namespace RepLeague.Application.Features.Strength.Queries.GetLiftPrs;

public record LiftPrDto(
    string ExerciseName,
    decimal BestWeightKg,
    int BestReps,
    decimal? Best1RmKg,
    DateTime AchievedAt
);

public record GetLiftPrsQuery(Guid UserId) : IRequest<List<LiftPrDto>>;
