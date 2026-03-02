using MediatR;
using RepLeague.Application.Features.Strength.DTOs;

namespace RepLeague.Application.Features.Strength.Commands.CreateLiftSession;

public record CreateLiftSessionCommand(
    Guid UserId,
    DateOnly Date,
    string? Title,
    string? Notes,
    List<CreateStrengthSetDto> Sets
) : IRequest<LiftSessionDto>;

public record CreateStrengthSetDto(
    string ExerciseName,
    int SetNumber,
    int Reps,
    decimal WeightKg,
    bool IsWarmup,
    string? Notes
);
