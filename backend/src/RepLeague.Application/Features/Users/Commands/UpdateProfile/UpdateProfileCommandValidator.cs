using FluentValidation;

namespace RepLeague.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .MinimumLength(2).MaximumLength(100)
            .When(x => x.DisplayName != null);

        RuleFor(x => x.Country)
            .Length(2, 3)
            .When(x => x.Country != null);

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => x.Bio != null);

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9 \-()]{7,20}$")
            .WithMessage("Phone must be a valid international number (7–20 digits).")
            .When(x => x.Phone != null);

        RuleFor(x => x.BirthDate)
            .Must(d => !d.HasValue || d.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-13)))
            .WithMessage("You must be at least 13 years old.")
            .When(x => x.BirthDate.HasValue);

        RuleFor(x => x.City).MaximumLength(100).When(x => x.City != null);
        RuleFor(x => x.GymName).MaximumLength(80).When(x => x.GymName != null);

        RuleFor(x => x.Units)
            .Must(u => u == "kg" || u == "lb")
            .WithMessage("Units must be 'kg' or 'lb'.")
            .When(x => x.Units != null);

        RuleFor(x => x.OneRmMethod)
            .Must(m => m == "Epley" || m == "Brzycki")
            .WithMessage("1RM method must be 'Epley' or 'Brzycki'.")
            .When(x => x.OneRmMethod != null);

        RuleFor(x => x.Visibility)
            .Must(v => new[] { "private", "leagues", "public" }.Contains(v))
            .WithMessage("Visibility must be 'private', 'leagues', or 'public'.")
            .When(x => x.Visibility != null);
    }
}
