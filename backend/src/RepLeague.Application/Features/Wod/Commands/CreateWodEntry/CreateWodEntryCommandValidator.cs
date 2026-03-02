using FluentValidation;

namespace RepLeague.Application.Features.Wod.Commands.CreateWodEntry;

public class CreateWodEntryCommandValidator : AbstractValidator<CreateWodEntryCommand>
{
    private static readonly string[] ValidTypes = ["ForTime", "AMRAP", "EMOM", "Chipper", "Intervals"];
    private static readonly string[] ValidMovementTypes = ["barbell", "kb", "bodyweight", "gymnastic", "cardio", "other"];
    private static readonly string[] ValidLoadUnits = ["kg", "lb", "cal", "m", "reps"];

    public CreateWodEntryCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => ValidTypes.Contains(t))
            .WithMessage($"Type must be one of: {string.Join(", ", ValidTypes)}.");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("Date cannot be in the future.");

        RuleFor(x => x.Title).MaximumLength(150).When(x => x.Title != null);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes != null);

        RuleFor(x => x.Exercises)
            .NotEmpty().WithMessage("At least one exercise is required.");

        RuleForEach(x => x.Exercises).ChildRules(e =>
        {
            e.RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            e.RuleFor(x => x.MovementType)
                .Must(m => ValidMovementTypes.Contains(m))
                .WithMessage($"MovementType must be one of: {string.Join(", ", ValidMovementTypes)}.");
            e.RuleFor(x => x.LoadValue).GreaterThanOrEqualTo(0).When(x => x.LoadValue.HasValue);
            e.RuleFor(x => x.LoadUnit)
                .Must(u => ValidLoadUnits.Contains(u!))
                .When(x => x.LoadUnit != null)
                .WithMessage($"LoadUnit must be one of: {string.Join(", ", ValidLoadUnits)}.");
            e.RuleFor(x => x.Reps).GreaterThan(0).When(x => x.Reps.HasValue);
        });

        When(x => x.Type == "AMRAP", () =>
        {
            RuleFor(x => x.AmrapResult)
                .NotNull().WithMessage("AMRAP result is required for AMRAP WODs.");
            RuleFor(x => x.AmrapResult!.RoundsCompleted).GreaterThanOrEqualTo(0);
            RuleFor(x => x.AmrapResult!.ExtraReps).GreaterThanOrEqualTo(0);
        });

        When(x => x.Type == "EMOM", () =>
        {
            RuleFor(x => x.EmomResult)
                .NotNull().WithMessage("EMOM result is required for EMOM WODs.");
            RuleFor(x => x.EmomResult!.TotalMinutes).GreaterThan(0);
            RuleFor(x => x.EmomResult!.IntervalsDone).GreaterThanOrEqualTo(0);
        });

        When(x => x.Type == "ForTime" || x.Type == "Chipper" || x.Type == "Intervals", () =>
        {
            RuleFor(x => x.ElapsedTime)
                .NotEmpty().WithMessage("ElapsedTime is required for time-based WODs.");
        });
    }
}
