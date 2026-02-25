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
    }
}
