using Microsoft.EntityFrameworkCore.Migrations;

namespace ARAMLadder.Migrations
{
    public partial class AddElo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoseStreak",
                table: "LoginGames",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PointLose",
                table: "LoginGames",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<int>(
                name: "PointWin",
                table: "LoginGames",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "LoginGames",
                nullable: false,
                defaultValue: 500);

            migrationBuilder.AddColumn<int>(
                name: "WinStreak",
                table: "LoginGames",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoseStreak",
                table: "LoginGames");

            migrationBuilder.DropColumn(
                name: "PointLose",
                table: "LoginGames");

            migrationBuilder.DropColumn(
                name: "PointWin",
                table: "LoginGames");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "LoginGames");

            migrationBuilder.DropColumn(
                name: "WinStreak",
                table: "LoginGames");
        }
    }
}
