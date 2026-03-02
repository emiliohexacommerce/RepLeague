using FluentValidation;

namespace RepLeague.Application.Features.Strength.Commands.CreateLiftSession;

public class CreateLiftSessionCommandValidator : AbstractValidator<CreateLiftSessionCommand>
{
    public CreateLiftSessionCommandValidator()
    {
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("Date cannot be in the future.");

        RuleFor(x => x.Title).MaximumLength(150).When(x => x.Title != null);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes != null);

        RuleFor(x => x.Sets)
            .NotEmpty().WithMessage("At least one set is required.");

        RuleForEach(x => x.Sets).ChildRules(s =>
        {
            s.RuleFor(x => x.ExerciseName).NotEmpty().MaximumLength(100);
            s.RuleFor(x => x.SetNumber).GreaterThan(0);
            s.RuleFor(x => x.Reps).GreaterThan(0);
            s.RuleFor(x => x.WeightKg).GreaterThanOrEqualTo(0);
        });
    }
}
