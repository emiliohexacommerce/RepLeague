using FluentValidation;

namespace RepLeague.Application.Features.Leagues.Commands.CreateLeague;

public class CreateLeagueCommandValidator : AbstractValidator<CreateLeagueCommand>
{
    public CreateLeagueCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}
