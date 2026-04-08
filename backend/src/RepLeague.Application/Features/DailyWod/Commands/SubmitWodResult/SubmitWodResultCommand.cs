using MediatR;
using RepLeague.Application.Features.DailyWod.DTOs;

namespace RepLeague.Application.Features.DailyWod.Commands.SubmitWodResult;

public record SubmitWodResultCommand(
    Guid LeagueId,
    Guid UserId,
    int? ElapsedSeconds,
    int? RoundsCompleted,
    int? TotalReps,
    bool IsRx,
    bool DidNotFinish,
    string? Notes,
    List<WodResultExerciseRequest> ExerciseDetails
) : IRequest<DailyWodResultDto>;
