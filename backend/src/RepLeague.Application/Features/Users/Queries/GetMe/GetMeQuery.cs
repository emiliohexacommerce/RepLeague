using MediatR;
using RepLeague.Application.Features.Users.DTOs;

namespace RepLeague.Application.Features.Users.Queries.GetMe;

public record GetMeQuery(Guid UserId) : IRequest<ProfileDto>;
