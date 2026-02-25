using RepLeague.Domain.Entities;

namespace RepLeague.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    int AccessTokenExpirationMinutes { get; }
}
