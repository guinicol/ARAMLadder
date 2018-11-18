using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ARAMLadder.Migrations
{
    public partial class AddRunesAndSpells : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Runes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spells",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoginGameRunes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LoginGameId = table.Column<int>(nullable: false),
                    RuneId = table.Column<int>(nullable: false),
                    Position = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginGameRunes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginGameRunes_LoginGames_LoginGameId",
                        column: x => x.LoginGameId,
                        principalTable: "LoginGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoginGameRunes_Runes_RuneId",
                        column: x => x.RuneId,
                        principalTable: "Runes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoginGameSpells",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LoginGameId = table.Column<int>(nullable: false),
                    SpellId = table.Column<int>(nullable: false),
                    Position = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginGameSpells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginGameSpells_LoginGames_LoginGameId",
                        column: x => x.LoginGameId,
                        principalTable: "LoginGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoginGameSpells_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoginGameRunes_LoginGameId",
                table: "LoginGameRunes",
                column: "LoginGameId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginGameRunes_RuneId",
                table: "LoginGameRunes",
                column: "RuneId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginGameSpells_LoginGameId",
                table: "LoginGameSpells",
                column: "LoginGameId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginGameSpells_SpellId",
                table: "LoginGameSpells",
                column: "SpellId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginGameRunes");

            migrationBuilder.DropTable(
                name: "LoginGameSpells");

            migrationBuilder.DropTable(
                name: "Runes");

            migrationBuilder.DropTable(
                name: "Spells");
        }
    }
}
