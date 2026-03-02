using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Users.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(
    IAppDbContext db, IEmailService emailService, IConfiguration configuration)
    : IRequestHandler<UpdateProfileCommand, ProfileDto>
{
    private readonly string _frontendBaseUrl =
        configuration["FrontendBaseUrl"] ?? "http://localhost:4200";

    public async Task<ProfileDto> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        // Capture consent BEFORE applying changes to detect the transition true → false
        bool wasConsented = user.MarketingConsent;

        // PATCH semantics — only update fields that were explicitly provided
        if (request.DisplayName != null)   user.DisplayName = request.DisplayName.Trim();
        if (request.Country != null)       user.Country = request.Country.ToUpper();
        if (request.Bio != null)           user.Bio = request.Bio.Trim();
        if (request.Phone != null)         user.Phone = request.Phone.Trim();
        if (request.BirthDate.HasValue)    user.BirthDate = request.BirthDate;
        if (request.City != null)          user.City = request.City.Trim();
        if (request.GymName != null)       user.GymName = request.GymName.Trim();
        if (request.Units != null)         user.Units = request.Units;
        if (request.OneRmMethod != null)   user.OneRmMethod = request.OneRmMethod;
        if (request.Visibility != null)    user.Visibility = request.Visibility;
        if (request.MarketingConsent.HasValue) user.MarketingConsent = request.MarketingConsent.Value;

        await db.SaveChangesAsync(ct);

        // Send unsubscribe confirmation when marketing consent changes true → false
        if (wasConsented && !user.MarketingConsent)
        {
            var firstName = user.DisplayName.Split(' ')[0];
            var resubUrl = $"{_frontendBaseUrl}/profile";
            _ = emailService.SendUnsubscribeConfirmationAsync(user.Email, firstName, resubUrl, ct);
        }

        var totalWorkouts = await db.Workouts.CountAsync(w => w.UserId == request.UserId, ct);
        var totalPrs      = await db.Workouts.CountAsync(w => w.UserId == request.UserId && w.IsPR, ct);
        var leagueCount   = await db.LeagueMembers.CountAsync(m => m.UserId == request.UserId, ct);

        return new ProfileDto(
            user.Id, user.Email, user.DisplayName, user.AvatarUrl,
            user.Country, user.Bio, user.CreatedAt,
            new UserStatsDto(totalWorkouts, totalPrs, leagueCount),
            user.Phone, user.BirthDate, user.City, user.GymName,
            user.Units, user.OneRmMethod, user.Visibility, user.MarketingConsent
        );
    }
}
