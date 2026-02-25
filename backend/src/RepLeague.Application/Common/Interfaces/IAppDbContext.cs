using Microsoft.EntityFrameworkCore;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Workout> Workouts { get; }
    DbSet<WorkoutExercise> WorkoutExercises { get; }
    DbSet<WorkoutWod> WorkoutWods { get; }
    DbSet<League> Leagues { get; }
    DbSet<LeagueMember> LeagueMembers { get; }
    DbSet<Invitation> Invitations { get; }
    DbSet<RankingEntry> RankingEntries { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
