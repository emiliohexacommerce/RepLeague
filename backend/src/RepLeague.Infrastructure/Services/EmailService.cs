using Microsoft.Extensions.Configuration;
using RepLeague.Application.Common.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace RepLeague.Infrastructure.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly string _apiKey = configuration["SendGrid:ApiKey"]
        ?? throw new InvalidOperationException("SendGrid:ApiKey not configured.");
    private readonly string _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@replague.com";
    private readonly string _fromName = configuration["SendGrid:FromName"] ?? "RepLeague";

    public async Task SendLeagueInvitationAsync(
        string toEmail, string leagueName, string joinUrl, CancellationToken ct = default)
    {
        var client = new SendGridClient(_apiKey);

        var msg = new SendGridMessage
        {
            From = new EmailAddress(_fromEmail, _fromName),
            Subject = $"You've been invited to join {leagueName} on RepLeague",
            HtmlContent = $"""
                <h2>You're invited to join <strong>{leagueName}</strong>!</h2>
                <p>Click the button below to accept your invitation and join the competition.</p>
                <p>
                  <a href="{joinUrl}"
                     style="background:#0ea5e9;color:white;padding:12px 24px;
                            text-decoration:none;border-radius:8px;font-weight:bold">
                    Join League
                  </a>
                </p>
                <p style="color:#6b7280;font-size:12px">
                  This invitation expires in 7 days.
                </p>
                """
        };

        msg.AddTo(new EmailAddress(toEmail));
        await client.SendEmailAsync(msg, ct);
    }
}
