using MediatR;

namespace RepLeague.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token) : IRequest;
