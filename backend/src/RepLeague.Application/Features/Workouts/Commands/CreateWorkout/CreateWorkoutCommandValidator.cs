using FluentValidation;
using RepLeague.Domain.Enums;

namespace RepLeague.Application.Features.Workouts.Commands.CreateWorkout;

public class CreateWorkoutCommandValidator : AbstractValidator<CreateWorkoutCommand>
{
    public CreateWorkoutCommandValidator()
    {
        RuleFor(x => x.Type).IsInEnum();

        When(x => x.Type == WorkoutType.Strength, () =>
        {
            RuleFor(x => x.Exercises)
                .NotEmpty().WithMessage("A strength workout must have at least one exercise.");

            RuleForEach(x => x.Exercises).ChildRules(e =>
            {
                e.RuleFor(x => x.ExerciseName).NotEmpty().MaximumLength(100);
                e.RuleFor(x => x.Sets).GreaterThan(0);
                e.RuleFor(x => x.Reps).GreaterThan(0);
                e.RuleFor(x => x.WeightKg).GreaterThanOrEqualTo(0);
            });
        });

        When(x => x.Type == WorkoutType.WOD, () =>
        {
            RuleFor(x => x.Wod)
                .NotNull().WithMessage("A WOD workout must include WOD details.");

            RuleFor(x => x.Wod!.WodName).NotEmpty().MaximumLength(100);
        });

        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes != null);
    }
}
