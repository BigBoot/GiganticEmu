using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiganticEmu.Shared.Backend.Migrations
{
    /// <inheritdoc />
    public partial class add_matchmaking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DiscordId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscordLinkToken",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchToken",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SkillDeviation",
                table: "AspNetUsers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SkillRating",
                table: "AspNetUsers",
                type: "double precision",
                nullable: false,
                defaultValue: 350.0);

            migrationBuilder.AddColumn<double>(
                name: "SkillVolatility",
                table: "AspNetUsers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.06);

            migrationBuilder.CreateTable(
                name: "ReportTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTokens", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportTokens");

            migrationBuilder.DropColumn(
                name: "DiscordId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DiscordLinkToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MatchToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SkillDeviation",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SkillRating",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SkillVolatility",
                table: "AspNetUsers");
        }
    }
}
