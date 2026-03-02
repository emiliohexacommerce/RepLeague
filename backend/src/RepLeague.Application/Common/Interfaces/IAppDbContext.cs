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
    DbSet<WodEntry> WodEntries { get; }
    DbSet<WodExercise> WodExercises { get; }
    DbSet<WodResultAmrap> WodResultAmraps { get; }
    DbSet<WodResultEmom> WodResultEmoms { get; }
    DbSet<LiftSession> LiftSessions { get; }
    DbSet<StrengthSet> StrengthSets { get; }
    DbSet<PushSubscription> PushSubscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
