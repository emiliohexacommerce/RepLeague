using MediatR;
using RepLeague.Application.Features.Users.DTOs;

namespace RepLeague.Application.Features.Users.Queries.GetProfileSummary;

public record GetProfileSummaryQuery(Guid UserId) : IRequest<ProfileSummaryDto>;
