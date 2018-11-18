using Microsoft.EntityFrameworkCore.Migrations;

namespace ARAMLadder.Migrations
{
    public partial class AddLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "LoginGames",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "LoginGames");
        }
    }
}
