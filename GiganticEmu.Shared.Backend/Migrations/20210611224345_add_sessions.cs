using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GiganticEmu.Shared.Backend.Migrations
{
    public partial class add_sessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSessionHost",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JoinState",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MemberSettings",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SessionConfiguration",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionSettings",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SessionVersion",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSessionHost",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "JoinState",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MemberSettings",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SessionConfiguration",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SessionSettings",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SessionVersion",
                table: "AspNetUsers");
        }
    }
}
