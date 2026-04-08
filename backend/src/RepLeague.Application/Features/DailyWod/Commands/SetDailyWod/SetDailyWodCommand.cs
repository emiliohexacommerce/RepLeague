using MediatR;
using RepLeague.Application.Features.DailyWod.DTOs;

namespace RepLeague.Application.Features.DailyWod.Commands.SetDailyWod;

public record SetDailyWodCommand(
    Guid LeagueId,
    Guid UserId,
    string Type,
    string Title,
    int? TimeCapSeconds,
    string? Notes,
    List<DailyWodExerciseRequest> Exercises
) : IRequest<DailyWodDto>;
