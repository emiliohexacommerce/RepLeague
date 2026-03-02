using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepLeague.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWodModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WodEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeCapSeconds = table.Column<int>(type: "int", nullable: true),
                    ElapsedSeconds = table.Column<int>(type: "int", nullable: true),
                    Rounds = table.Column<int>(type: "int", nullable: true),
                    RxScaled = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WodEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WodEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WodExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WodEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    LoadValue = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    LoadUnit = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Reps = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WodExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WodExercises_WodEntries_WodEntryId",
                        column: x => x.WodEntryId,
                        principalTable: "WodEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WodResultAmraps",
                columns: table => new
                {
                    WodEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundsCompleted = table.Column<int>(type: "int", nullable: false),
                    ExtraReps = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WodResultAmraps", x => x.WodEntryId);
                    table.ForeignKey(
                        name: "FK_WodResultAmraps_WodEntries_WodEntryId",
                        column: x => x.WodEntryId,
                        principalTable: "WodEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WodResultEmoms",
                columns: table => new
                {
                    WodEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalMinutes = table.Column<int>(type: "int", nullable: false),
                    IntervalsDone = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WodResultEmoms", x => x.WodEntryId);
                    table.ForeignKey(
                        name: "FK_WodResultEmoms_WodEntries_WodEntryId",
                        column: x => x.WodEntryId,
                        principalTable: "WodEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WodEntries_Type_Date",
                table: "WodEntries",
                columns: new[] { "Type", "Date" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WodEntries_UserId_Date",
                table: "WodEntries",
                columns: new[] { "UserId", "Date" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WodEntries_UserId_Title",
                table: "WodEntries",
                columns: new[] { "UserId", "Title" },
                filter: "[IsDeleted] = 0 AND [Title] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WodExercises_WodEntryId_OrderIndex",
                table: "WodExercises",
                columns: new[] { "WodEntryId", "OrderIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WodExercises");

            migrationBuilder.DropTable(
                name: "WodResultAmraps");

            migrationBuilder.DropTable(
                name: "WodResultEmoms");

            migrationBuilder.DropTable(
                name: "WodEntries");
        }
    }
}
