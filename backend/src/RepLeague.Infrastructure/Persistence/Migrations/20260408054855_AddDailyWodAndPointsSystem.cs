using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepLeague.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyWodAndPointsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BackfillCompleted",
                table: "Leagues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PointsActivatedAt",
                table: "Leagues",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DailyPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    AttendancePoints = table.Column<int>(type: "int", nullable: false),
                    VolumePoints = table.Column<int>(type: "int", nullable: false),
                    PrPoints = table.Column<int>(type: "int", nullable: false),
                    WodCompletionPoints = table.Column<int>(type: "int", nullable: false),
                    WodRankingPoints = table.Column<int>(type: "int", nullable: false),
                    StreakPoints = table.Column<int>(type: "int", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyPoints_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyPoints_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyWods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TimeCapSeconds = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyWods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyWods_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyWods_Users_SetByUserId",
                        column: x => x.SetByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyWodExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyWodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    ExerciseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Reps = table.Column<int>(type: "int", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyWodExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyWodExercises_DailyWods_DailyWodId",
                        column: x => x.DailyWodId,
                        principalTable: "DailyWods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyWodResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyWodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElapsedSeconds = table.Column<int>(type: "int", nullable: true),
                    RoundsCompleted = table.Column<int>(type: "int", nullable: true),
                    TotalReps = table.Column<int>(type: "int", nullable: true),
                    IsRx = table.Column<bool>(type: "bit", nullable: false),
                    DidNotFinish = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyWodResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyWodResults_DailyWods_DailyWodId",
                        column: x => x.DailyWodId,
                        principalTable: "DailyWods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyWodResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyWodResultExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyWodResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyWodExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepsCompleted = table.Column<int>(type: "int", nullable: true),
                    WeightUsedKg = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyWodResultExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyWodResultExercises_DailyWodExercises_DailyWodExerciseId",
                        column: x => x.DailyWodExerciseId,
                        principalTable: "DailyWodExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyWodResultExercises_DailyWodResults_DailyWodResultId",
                        column: x => x.DailyWodResultId,
                        principalTable: "DailyWodResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyPoints_LeagueId",
                table: "DailyPoints",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyPoints_UserId_LeagueId_Date",
                table: "DailyPoints",
                columns: new[] { "UserId", "LeagueId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyWodExercises_DailyWodId",
                table: "DailyWodExercises",
                column: "DailyWodId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyWodResultExercises_DailyWodExerciseId",
                table: "DailyWodResultExercises",
                column: "DailyWodExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyWodResultExercises_DailyWodResultId",
                table: "DailyWodResultExercises",
                column: "DailyWodResultId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyWodResults_DailyWodId_UserId",
                table: "DailyWodResults",
                columns: new[] { "DailyWodId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyWodResults_UserId",
                table: "DailyWodResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyWods_LeagueId_Date",
                table: "DailyWods",
                columns: new[] { "LeagueId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyWods_SetByUserId",
                table: "DailyWods",
                column: "SetByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyPoints");

            migrationBuilder.DropTable(
                name: "DailyWodResultExercises");

            migrationBuilder.DropTable(
                name: "DailyWodExercises");

            migrationBuilder.DropTable(
                name: "DailyWodResults");

            migrationBuilder.DropTable(
                name: "DailyWods");

            migrationBuilder.DropColumn(
                name: "BackfillCompleted",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "PointsActivatedAt",
                table: "Leagues");
        }
    }
}
