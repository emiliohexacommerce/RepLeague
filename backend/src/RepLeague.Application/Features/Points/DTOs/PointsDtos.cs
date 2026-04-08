namespace RepLeague.Application.Features.Points.DTOs;

public record LeaguePointsRankingEntryDto(
    int Position,
    Guid UserId,
    string DisplayName,        // "Atleta Anónimo" si private
    string? AvatarUrl,         // null si private
    string? Country,           // null si private
    string? City,              // null si private
    string? GymName,           // null si private
    bool IsAnonymous,
    int TotalPoints,
    int AttendancePoints,
    int VolumePoints,
    int PrPoints,
    int WodCompletionPoints,
    int WodRankingPoints,
    int StreakPoints,
    int TrainingDays
);
