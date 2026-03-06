using MediatR;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Strength.Commands.AddManualLiftPr;

public class AddManualLiftPrCommandHandler(IAppDbContext db)
    : IRequestHandler<AddManualLiftPrCommand, ManualLiftPrDto>
{
    public async Task<ManualLiftPrDto> Handle(AddManualLiftPrCommand request, CancellationToken ct)
    {
        var entry = new ManualLiftPr
        {
            Id          = Guid.NewGuid(),
            UserId      = request.UserId,
            ExerciseName = request.ExerciseName.Trim(),
            WeightKg    = request.WeightKg,
            Notes       = request.Notes?.Trim(),
            AchievedAt  = request.AchievedAt,
            CreatedAt   = DateTime.UtcNow,
            IsDeleted   = false
        };

        db.ManualLiftPrs.Add(entry);
        await db.SaveChangesAsync(ct);

        return ToDto(entry);
    }

    private static ManualLiftPrDto ToDto(ManualLiftPr p) =>
        new(p.Id, p.ExerciseName, p.WeightKg, p.Notes, p.AchievedAt, p.CreatedAt);
}
