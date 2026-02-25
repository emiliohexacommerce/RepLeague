namespace RepLeague.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendLeagueInvitationAsync(string toEmail, string leagueName, string joinUrl, CancellationToken ct = default);
}
