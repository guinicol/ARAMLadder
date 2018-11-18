using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ARAMLadder.Migrations
{
    public partial class AddChampionAndItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChampionId",
                table: "LoginGames",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Champions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Champions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoginGameItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LoginGameId = table.Column<int>(nullable: false),
                    ItemId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginGameItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginGameItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoginGameItems_LoginGames_LoginGameId",
                        column: x => x.LoginGameId,
                        principalTable: "LoginGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoginGames_ChampionId",
                table: "LoginGames",
                column: "ChampionId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginGameItems_ItemId",
                table: "LoginGameItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginGameItems_LoginGameId",
                table: "LoginGameItems",
                column: "LoginGameId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginGames_Champions_ChampionId",
                table: "LoginGames",
                column: "ChampionId",
                principalTable: "Champions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoginGames_Champions_ChampionId",
                table: "LoginGames");

            migrationBuilder.DropTable(
                name: "Champions");

            migrationBuilder.DropTable(
                name: "LoginGameItems");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropIndex(
                name: "IX_LoginGames_ChampionId",
                table: "LoginGames");

            migrationBuilder.DropColumn(
                name: "ChampionId",
                table: "LoginGames");
        }
    }
}
