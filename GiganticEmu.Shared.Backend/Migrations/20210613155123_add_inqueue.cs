using Microsoft.EntityFrameworkCore.Migrations;

namespace GiganticEmu.Shared.Backend.Migrations
{
    public partial class add_inqueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InQueue",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InQueue",
                table: "AspNetUsers");
        }
    }
}
