using Microsoft.EntityFrameworkCore.Migrations;

namespace GiganticEmu.Shared.Backend.Migrations
{
    public partial class add_salsa_ck : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SalsaSCK",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalsaSCK",
                table: "AspNetUsers");
        }
    }
}
