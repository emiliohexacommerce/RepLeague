namespace RepLeague.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string firstName, CancellationToken ct = default);
    Task SendLeagueInvitationAsync(string toEmail, string leagueName, string joinUrl,
        string inviterName, string firstName, CancellationToken ct = default);
    Task SendInvitationAcceptedEmailAsync(string ownerEmail, string ownerFirstName,
        string newMemberName, string leagueName, string leagueUrl, CancellationToken ct = default);
    Task SendNewPrEmailAsync(string userEmail, string firstName,
        string movementName, string previousValue, string newValue, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(string toEmail, string firstName,
        string resetUrl, int expiryMinutes, CancellationToken ct = default);
    Task SendEmailVerificationAsync(string toEmail, string firstName,
        string verificationCode, string verificationUrl, CancellationToken ct = default);
    Task SendUnsubscribeConfirmationAsync(string toEmail, string firstName,
        string resubscribeUrl, CancellationToken ct = default);
}
