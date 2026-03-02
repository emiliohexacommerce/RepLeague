using Microsoft.Extensions.Configuration;
using RepLeague.Application.Common.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace RepLeague.Infrastructure.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly string _apiKey = configuration["SendGrid:ApiKey"]
        ?? throw new InvalidOperationException("SendGrid:ApiKey not configured.");
    private readonly string _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@repleague.com";
    private readonly string _fromName = configuration["SendGrid:FromName"] ?? "RepLeague";
    private readonly string _frontendBaseUrl = configuration["FrontendBaseUrl"] ?? "http://localhost:4200";

    public async Task SendWelcomeEmailAsync(string toEmail, string firstName, CancellationToken ct = default)
    {
        var client = new SendGridClient(_apiKey);
        var profileUrl = $"{_frontendBaseUrl}/profile";

        var msg = new SendGridMessage
        {
            From = new EmailAddress(_fromEmail, _fromName),
            Subject = $"¡Bienvenido/a a RepLeague, {firstName}! 🏆",
            HtmlContent = $"""
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1.0"></head>
                <body style="margin:0;padding:0;background-color:#F2F2F2;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#F2F2F2;">
                    <tr><td align="center" style="padding:32px 16px;">
                      <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:600px;">
                        <tr><td style="background-color:#FF7A1A;padding:24px 32px;border-radius:12px 12px 0 0;">
                          <span style="font-size:24px;font-weight:800;color:#FFFFFF;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;">Rep<span style="color:#121212;">League</span></span>
                        </td></tr>
                        <tr><td style="background-color:#E15E0A;padding:32px;text-align:center;">
                          <p style="margin:0 0 8px;font-size:40px;">🏆</p>
                          <h1 style="margin:0;font-size:26px;font-weight:900;color:#FFFFFF;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;">¡Bienvenido/a, {firstName}!</h1>
                          <p style="margin:8px 0 0;font-size:15px;color:#FFE4CC;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;">Tu cuenta en RepLeague está lista</p>
                        </td></tr>
                        <tr><td style="background-color:#FFFFFF;padding:40px 32px;">
                          <p style="margin:0 0 24px;font-size:16px;color:#2F2F2F;line-height:1.6;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;">
                            Estás a un paso de unirte a la competencia. Completa tu perfil, únete a una liga y empieza a registrar tus entrenamientos.
                          </p>
                          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:24px;">
                            <tr>
                              <td style="width:33%;padding:8px 6px;" valign="top">
                                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#FFF7ED;border-radius:10px;text-align:center;">
                                  <tr><td style="padding:20px 12px;">
                                    <p style="margin:0 0 8px;font-size:28px;">💪</p>
                                    <p style="margin:0;font-size:13px;font-weight:700;color:#92400E;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;">Registra tus entrenamientos</p>
                                  </td></tr>
                                </table>
                              </td>
                              <td style="width:33%;padding:8px 6px;" valign="top">
                                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#F0FDF4;border-radius:10px;text-align:center;">
                                  <tr><td style="padding:20px 12px;">
                                    <p style="margin:0 0 8px;font-size:28px;">🏆</p>
                                    <p style="margin:0;font-size:13px;font-weight:700;color:#166534;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;">Consigue PRs y sube el ranking</p>
                                  </td></tr>
                                </table>
                              </td>
                              <td style="width:33%;padding:8px 6px;" valign="top">
                                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#EFF6FF;border-radius:10px;text-align:center;">
                                  <tr><td style="padding:20px 12px;">
                                    <p style="margin:0 0 8px;font-size:28px;">⚡</p>
                                    <p style="margin:0;font-size:13px;font-weight:700;color:#1e40af;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;">Compite con tu liga</p>
                                  </td></tr>
                                </table>
                              </td>
                            </tr>
                          </table>
                          <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 auto 16px;">
                            <tr><td style="border-radius:8px;background-color:#FF7A1A;">
                              <a href="{profileUrl}" target="_blank"
                                 style="display:inline-block;padding:14px 36px;font-size:16px;font-weight:700;color:#FFFFFF;text-decoration:none;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;border-radius:8px;">
                                Completar mi perfil →
                              </a>
                            </td></tr>
                          </table>
                          <p style="margin:0;text-align:center;font-size:13px;color:#6B7280;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;">
                            <a href="{profileUrl}" style="color:#FF7A1A;">{profileUrl}</a>
                          </p>
                        </td></tr>
                        <tr><td style="background-color:#F2F2F2;padding:20px 32px;border-radius:0 0 12px 12px;border-top:1px solid #E5E7EB;text-align:center;">
                          <p style="margin:0;font-size:12px;color:#9CA3AF;font-family:-apple-system,'Segoe UI',Roboto,Arial,sans-serif;">© 2026 RepLeague · Santiago, Chile</p>
                        </td></tr>
                      </table>
                    </td></tr>
                  </table>
                </body>
                </html>
                """,
            PlainTextContent = $"""
                ¡Bienvenido/a a RepLeague, {firstName}!

                Tu cuenta está lista. Aquí tienes 3 pasos para empezar:
                  1. 💪 Registra tus entrenamientos
                  2. 🏆 Consigue PRs y sube el ranking
                  3. ⚡ Compite con tu liga

                COMPLETAR MI PERFIL
                {profileUrl}

                © 2026 RepLeague · Santiago, Chile
                """
        };

        msg.AddTo(new EmailAddress(toEmail));
        await client.SendEmailAsync(msg, ct);
    }

    public async Task SendLeagueInvitationAsync(
        string toEmail, string leagueName, string joinUrl,
        string inviterName, string firstName, CancellationToken ct = default)
    {
        // Build absolute join URL
        var fullJoinUrl = joinUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? joinUrl
            : $"{_frontendBaseUrl}{joinUrl}";

        var supportUrl = $"{_frontendBaseUrl}/support";
        var preferencesUrl = $"{_frontendBaseUrl}/profile";

        // Load the 04-invitacion-liga HTML template
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, "email-templates", "04-invitacion-liga", "template.html");

        var htmlContent = File.Exists(templatePath)
            ? await File.ReadAllTextAsync(templatePath, ct)
            : FallbackInvitationHtml(inviterName, firstName, leagueName, fullJoinUrl);

        htmlContent = htmlContent
            .Replace("{{ inviter_name }}", inviterName)
            .Replace("{{ first_name }}", firstName)
            .Replace("{{ league_name }}", leagueName)
            .Replace("{{ join_url }}", fullJoinUrl)
            .Replace("{{ support_url }}", supportUrl)
            .Replace("{{ preferences_url }}", preferencesUrl);

        var client = new SendGridClient(_apiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_fromEmail, _fromName),
            Subject = $"{inviterName} te invita a unirte a {leagueName} en RepLeague",
            HtmlContent = htmlContent,
            PlainTextContent = $"""
                {inviterName} te invita a unirte a {leagueName}.

                Hola {firstName}, tienes un cupo reservado en {leagueName}. Acepta la invitación y empieza a competir.

                UNIRSE A LA LIGA:
                {fullJoinUrl}

                Esta invitación expira en 7 días.

                © 2026 RepLeague · Santiago, Chile
                """
        };

        msg.AddTo(new EmailAddress(toEmail));
        await client.SendEmailAsync(msg, ct);
    }

    public async Task SendInvitationAcceptedEmailAsync(
        string ownerEmail, string ownerFirstName,
        string newMemberName, string leagueName, string leagueUrl, CancellationToken ct = default)
    {
        var fullLeagueUrl = leagueUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? leagueUrl : $"{_frontendBaseUrl}{leagueUrl}";

        var supportUrl = $"{_frontendBaseUrl}/support";
        var preferencesUrl = $"{_frontendBaseUrl}/profile";

        var templatePath = Path.Combine(
            AppContext.BaseDirectory, "email-templates", "05-aceptacion-invitacion", "template.html");

        var htmlContent = File.Exists(templatePath)
            ? await File.ReadAllTextAsync(templatePath, ct)
            : $"""
               <!DOCTYPE html><html lang="es"><body style="font-family:sans-serif;padding:32px;">
               <h2>¡{newMemberName} se unió a <strong>{leagueName}</strong>!</h2>
               <p>Hola {ownerFirstName}, un nuevo atleta se unió a tu liga. ¡A competir!</p>
               <p><a href="{fullLeagueUrl}" style="background:#FF7A1A;color:#fff;padding:12px 24px;text-decoration:none;border-radius:8px;font-weight:bold;">Ver la liga</a></p>
               </body></html>
               """;

        htmlContent = htmlContent
            .Replace("{{ inviter_name }}", ownerFirstName)
            .Replace("{{ new_member_name }}", newMemberName)
            .Replace("{{ league_name }}", leagueName)
            .Replace("{{ action_url }}", fullLeagueUrl)
            .Replace("{{ support_url }}", supportUrl)
            .Replace("{{ preferences_url }}", preferencesUrl);

        var client = new SendGridClient(_apiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_fromEmail, _fromName),
            Subject = $"¡{newMemberName} se unió a {leagueName}! 🎉",
            HtmlContent = htmlContent,
            PlainTextContent = $"""
                ¡Nuevo miembro en tu liga!

                Hola {ownerFirstName}, {newMemberName} aceptó la invitación y se unió a {leagueName}.

                VER LA LIGA: {fullLeagueUrl}

                © 2026 RepLeague · Santiago, Chile
                """
        };

        msg.AddTo(new EmailAddress(ownerEmail));
        await client.SendEmailAsync(msg, ct);
    }

    public async Task SendNewPrEmailAsync(
        string userEmail, string firstName,
        string movementName, string previousValue, string newValue, CancellationToken ct = default)
    {
        var shareUrl = $"{_frontendBaseUrl}/dashboard";
        var supportUrl = $"{_frontendBaseUrl}/support";
        var preferencesUrl = $"{_frontendBaseUrl}/profile";

        var templatePath = Path.Combine(
            AppContext.BaseDirectory, "email-templates", "07-nuevo-pr", "template.html");

        var htmlContent = File.Exists(templatePath)
            ? await File.ReadAllTextAsync(templatePath, ct)
            : $"""
               <!DOCTYPE html><html lang="es"><body style="font-family:sans-serif;padding:32px;">
               <h2>🏆 ¡Nuevo récord personal, {firstName}!</h2>
               <p><strong>{movementName}</strong>: {previousValue} → <span style="color:#16a34a;font-weight:bold;">{newValue}</span></p>
               <p><a href="{shareUrl}" style="background:#FF7A1A;color:#fff;padding:12px 24px;text-decoration:none;border-radius:8px;font-weight:bold;">Ver mi progreso</a></p>
               </body></html>
               """;

        htmlContent = htmlContent
            .Replace("{{ first_name }}", firstName)
            .Replace("{{ movement_name }}", movementName)
            .Replace("{{ previous_pr_value }}", previousValue)
            .Replace("{{ pr_value }}", newValue)
            .Replace("{{ share_url }}", shareUrl)
            .Replace("{{ support_url }}", supportUrl)
            .Replace("{{ preferences_url }}", preferencesUrl);

        var client = new SendGridClient(_apiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_fromEmail, _fromName),
            Subject = $"🏆 ¡Nuevo PR en {movementName}! {newValue}",
            HtmlContent = htmlContent,
            PlainTextContent = $"""
                ¡Nuevo récord personal, {firstName}!

                {movementName}: {previousValue} → {newValue}

                ¡Sigue así! Cada kilo extra es la prueba de que no te rendiste.

                VER MI PROGRESO: {shareUrl}

                © 2026 RepLeague · Santiago, Chile
                """
        };

        msg.AddTo(new EmailAddress(userEmail));
        await client.SendEmailAsync(msg, ct);
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail, string firstName, string resetUrl, int expiryMinutes, CancellationToken ct = default)
    {
        var supportUrl = $"{_frontendBaseUrl}/support";
        var preferencesUrl = $"{_frontendBaseUrl}/profile";

        var templatePath = Path.Combine(
            AppContext.BaseDirectory, "email-templates", "03-restablecer-contrasena", "template.html");

        var htmlContent = File.Exists(templatePath)
            ? await File.ReadAllTextAsync(templatePath, ct)
            : $"""
               <!DOCTYPE html><html lang="es"><body style="font-family:sans-serif;padding:32px;">
               <h2>Restablecer contraseña</h2>
               <p>Hola {firstName}, haz clic en el enlace para restablecer tu contraseña. Expira en {expiryMinutes} minutos.</p>
               <p><a href="{resetUrl}" style="background:#FF7A1A;color:#fff;padding:12px 24px;text-decoration:none;border-radius:8px;font-weight:bold;">Restablecer contraseña</a></p>
               <p style="color:#6b7280;font-size:12px">Si no solicitaste este cambio, ignora este correo.</p>
               </body></html>
               """;

        htmlContent = htmlContent
            .Replace("{{ first_name }}", firstName)
            .Replace("{{ reset_url }}", resetUrl)
            .Replace("{{ token_exp_minutes }}", expiryMinutes.ToString())
            .Replace("{{ support_url }}", supportUrl)
            .Replace("{{ preferences_url }}", preferencesUrl);

        var client = new SendGridClient(_apiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_fromEmail, _fromName),
            Subject = "Restablece tu contraseña de RepLeague",
            HtmlContent = htmlContent,
            PlainTextContent = $"""
                Restablece tu contraseña, {firstName}.

                Haz clic en el enlace para crear una nueva contraseña. Expira en {expiryMinutes} minutos.

                RESTABLECER CONTRASEÑA: {resetUrl}

                Si no solicitaste este cambio, ignora este correo.

                © 2026 RepLeague · Santiago, Chile
                """
        };

        msg.AddTo(new EmailAddress(toEmail));
        await client.SendEmailAsync(msg, ct);
    }

    public async Task SendEmailVerificationAsync(
        string toEmail, string firstName,
        string verificationCode, string verificationUrl, CancellationToken ct = default)
    {
        var supportUrl = $"{_frontendBaseUrl}/support";
        var preferencesUrl = $"{_frontendBaseUrl}/profile";

        var templatePath = Path.Combine(
            AppContext.BaseDirectory, "email-templates", "01-verificacion", "template.html");

        var htmlContent = File.Exists(templatePath)
            ? await File.ReadAllTextAsync(templatePath, ct)
            : $"""
               <!DOCTYPE html><html lang="es"><body style="font-family:sans-serif;padding:32px;">
               <h2>Confirma tu email, {firstName}</h2>
               <p>Usa este código o haz clic en el enlace para activar tu cuenta.</p>
               <p style="font-size:36px;font-weight:bold;letter-spacing:8px;text-align:center;">{verificationCode}</p>
               <p><a href="{verificationUrl}" style="background:#FF7A1A;color:#fff;padding:12px 24px;text-decoration:none;border-radius:8px;font-weight:bold;">Verificar email</a></p>
               <p style="color:#6b7280;font-size:12px">Este código expira en 24 horas.</p>
               </body></html>
               """;

        htmlContent = htmlContent
            .Replace("{{ first_name }}", firstName)
            .Replace("{{ verification_code }}", verificationCode)
            .Replace("{{ verification_url }}", verificationUrl)
            .Replace("{{ support_url }}", supportUrl)
            .Replace("{{ preferences_url }}", preferencesUrl);

        var client = new SendGridClient(_apiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_fromEmail, _fromName),
            Subject = $"Confirma tu email en RepLeague — código {verificationCode}",
            HtmlContent = htmlContent,
            PlainTextContent = $"""
                Confirma tu email, {firstName}.

                Tu código de verificación: {verificationCode}

                O usa este enlace directo: {verificationUrl}

                Este código expira en 24 horas.

                © 2026 RepLeague · Santiago, Chile
                """
        };

        msg.AddTo(new EmailAddress(toEmail));
        await client.SendEmailAsync(msg, ct);
    }

    public async Task SendUnsubscribeConfirmationAsync(
        string toEmail, string firstName, string resubscribeUrl, CancellationToken ct = default)
    {
        var supportUrl = $"{_frontendBaseUrl}/support";
        var preferencesUrl = $"{_frontendBaseUrl}/profile";

        var templatePath = Path.Combine(
            AppContext.BaseDirectory, "email-templates", "12-desuscripcion", "template.html");

        var htmlContent = File.Exists(templatePath)
            ? await File.ReadAllTextAsync(templatePath, ct)
            : $"""
               <!DOCTYPE html><html lang="es"><body style="font-family:sans-serif;padding:32px;">
               <h2>Desuscripción confirmada</h2>
               <p>Hola {firstName}, ya no recibirás correos de RepLeague Noticias.</p>
               <p>Siempre puedes volver: <a href="{resubscribeUrl}">reactivar notificaciones</a></p>
               </body></html>
               """;

        htmlContent = htmlContent
            .Replace("{{ first_name }}", firstName)
            .Replace("{{ list_name }}", "RepLeague Noticias")
            .Replace("{{ resubscribe_url }}", resubscribeUrl)
            .Replace("{{ support_url }}", supportUrl)
            .Replace("{{ preferences_url }}", preferencesUrl);

        var client = new SendGridClient(_apiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_fromEmail, _fromName),
            Subject = "Desuscripción confirmada — RepLeague",
            HtmlContent = htmlContent,
            PlainTextContent = $"""
                Hola {firstName},

                Hemos confirmado tu desuscripción de RepLeague Noticias.
                Ya no recibirás correos de marketing.

                Si cambias de opinión, puedes reactivar las notificaciones aquí:
                {resubscribeUrl}

                © 2026 RepLeague · Santiago, Chile
                """
        };

        msg.AddTo(new EmailAddress(toEmail));
        await client.SendEmailAsync(msg, ct);
    }

    private static string FallbackInvitationHtml(
        string inviterName, string firstName, string leagueName, string joinUrl) => $"""
        <!DOCTYPE html><html lang="es"><body style="font-family:sans-serif;padding:32px;">
          <h2><span style="color:#FF7A1A;">{inviterName}</span> te invita a <strong>{leagueName}</strong></h2>
          <p>Hola {firstName}, tienes un cupo reservado. Acepta la invitación y empieza a competir.</p>
          <p><a href="{joinUrl}" style="background:#FF7A1A;color:#fff;padding:12px 24px;text-decoration:none;border-radius:8px;font-weight:bold;">
            Unirse a {leagueName}
          </a></p>
          <p style="color:#6b7280;font-size:12px">Esta invitación expira en 7 días.</p>
        </body></html>
        """;
}
