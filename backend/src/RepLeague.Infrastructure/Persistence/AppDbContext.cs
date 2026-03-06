using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<WorkoutWod> WorkoutWods => Set<WorkoutWod>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<LeagueMember> LeagueMembers => Set<LeagueMember>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<RankingEntry> RankingEntries => Set<RankingEntry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<WodEntry> WodEntries => Set<WodEntry>();
    public DbSet<WodExercise> WodExercises => Set<WodExercise>();
    public DbSet<WodResultAmrap> WodResultAmraps => Set<WodResultAmrap>();
    public DbSet<WodResultEmom> WodResultEmoms => Set<WodResultEmom>();
    public DbSet<LiftSession> LiftSessions => Set<LiftSession>();
    public DbSet<StrengthSet> StrengthSets => Set<StrengthSet>();
    public DbSet<ManualLiftPr> ManualLiftPrs => Set<ManualLiftPr>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
