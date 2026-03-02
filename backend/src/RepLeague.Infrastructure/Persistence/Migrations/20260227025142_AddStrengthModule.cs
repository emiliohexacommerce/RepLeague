using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepLeague.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStrengthModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiftSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiftSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiftSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StrengthSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LiftSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SetNumber = table.Column<int>(type: "int", nullable: false),
                    Reps = table.Column<int>(type: "int", nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    IsWarmup = table.Column<bool>(type: "bit", nullable: false),
                    IsPr = table.Column<bool>(type: "bit", nullable: false),
                    OneRepMaxKg = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrengthSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrengthSets_LiftSessions_LiftSessionId",
                        column: x => x.LiftSessionId,
                        principalTable: "LiftSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiftSessions_UserId_Date",
                table: "LiftSessions",
                columns: new[] { "UserId", "Date" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_StrengthSets_LiftSessionId_ExerciseName_SetNumber",
                table: "StrengthSets",
                columns: new[] { "LiftSessionId", "ExerciseName", "SetNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StrengthSets");

            migrationBuilder.DropTable(
                name: "LiftSessions");
        }
    }
}
