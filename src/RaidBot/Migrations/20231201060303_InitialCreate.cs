using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RaidBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SearchTerms = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expansions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expansions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SearchTerms = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpansionClasses",
                columns: table => new
                {
                    ExpansionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpansionClasses", x => new { x.ClassId, x.ExpansionId });
                    table.ForeignKey(
                        name: "FK_ExpansionClasses_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpansionClasses_Expansions_ExpansionId",
                        column: x => x.ExpansionId,
                        principalTable: "Expansions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildExpansionConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ExpansionId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CreateRoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Timezone = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildExpansionConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildExpansionConfigurations_Expansions_ExpansionId",
                        column: x => x.ExpansionId,
                        principalTable: "Expansions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Raids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Raids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Raids_GuildExpansionConfigurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "GuildExpansionConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaidMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    OwnerId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    OwnerName = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    RaidId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaidMembers_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaidMembers_Raids_RaidId",
                        column: x => x.RaidId,
                        principalTable: "Raids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaidMembers_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "Id", "Icon", "Name", "SearchTerms" },
                values: new object[,]
                {
                    { 1, "<:druid:945492995321528370>", "Druid", "druid" },
                    { 2, "<:hunter:945492995417972736>", "Hunter", "hunter;huntard" },
                    { 3, "<:mage:945492995225030697>", "Mage", "mage" },
                    { 4, "<:paladin:945492995321495582>", "Paladin", "paladin;pala;pally" },
                    { 5, "<:priest:945492995602526238>", "Priest", "priest" },
                    { 6, "<:rogue:945492995480883210>", "Rogue", "rogue;rouge" },
                    { 7, "<:shaman:945492995459940402>", "Shaman", "shaman;shammy;sham" },
                    { 8, "<:warlock:945492995648671754>", "Warlock", "warlock;lock" },
                    { 9, "<:warrior:945492995602538546>", "Warrior", "warrior;warr" },
                    { 10, "<:deathknight:945492994964979722>", "Death Knight", "death knight;deathknight;dk" },
                    { 11, "<:monk:945492996554629170>", "Monk", "monk" },
                    { 12, "<:demonhunter:945492995149545552>", "Demon Hunter", "demon hunter;demonhunter;dh" },
                    { 13, "<:evoker:1179290186530697296>", "Evoker", "evoker" }
                });

            migrationBuilder.InsertData(
                table: "Expansions",
                columns: new[] { "Id", "Name", "ShortName" },
                values: new object[,]
                {
                    { 2, "The Burning Crusade", "TBC" },
                    { 3, "Wrath of the Lich King", "WotLK" },
                    { 4, "Cataclysm", "Cata" },
                    { 5, "Mists of Pandaria", "Mists" },
                    { 6, "Warlords of Draenor", "WoD" },
                    { 7, "Legion", "Legion" },
                    { 8, "Battle for Azeroth", "BfA" },
                    { 9, "Shadowlands", "SL" },
                    { 10, "Dragonflight", "DF" },
                    { 101, "Classic (Alliance)", "Classic" },
                    { 102, "Classic (Horde)", "Classic" },
                    { 111, "Season of Discovery (Alliance)", "SoD" },
                    { 112, "Season of Discovery (Horde)", "SoD" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Icon", "Name", "SearchTerms" },
                values: new object[,]
                {
                    { 1, "<:tank:945067186421116958>", "Tanks", "tank;t" },
                    { 2, "<:healer:945067186542772244>", "Healers", "healer;heals;heal;h" },
                    { 3, "<:rdps:945067186203033691>", "Ranged", "ranged;caster;rdps;r" },
                    { 4, "<:mdps:945067186421104670>", "Melee", "melee;mdps;m" }
                });

            migrationBuilder.InsertData(
                table: "ExpansionClasses",
                columns: new[] { "ClassId", "ExpansionId" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 1, 3 },
                    { 1, 4 },
                    { 1, 5 },
                    { 1, 6 },
                    { 1, 7 },
                    { 1, 8 },
                    { 1, 9 },
                    { 1, 10 },
                    { 1, 101 },
                    { 1, 102 },
                    { 1, 111 },
                    { 1, 112 },
                    { 2, 2 },
                    { 2, 3 },
                    { 2, 4 },
                    { 2, 5 },
                    { 2, 6 },
                    { 2, 7 },
                    { 2, 8 },
                    { 2, 9 },
                    { 2, 10 },
                    { 2, 101 },
                    { 2, 102 },
                    { 2, 111 },
                    { 2, 112 },
                    { 3, 2 },
                    { 3, 3 },
                    { 3, 4 },
                    { 3, 5 },
                    { 3, 6 },
                    { 3, 7 },
                    { 3, 8 },
                    { 3, 9 },
                    { 3, 10 },
                    { 3, 101 },
                    { 3, 102 },
                    { 3, 111 },
                    { 3, 112 },
                    { 4, 2 },
                    { 4, 3 },
                    { 4, 4 },
                    { 4, 5 },
                    { 4, 6 },
                    { 4, 7 },
                    { 4, 8 },
                    { 4, 9 },
                    { 4, 10 },
                    { 4, 101 },
                    { 4, 111 },
                    { 5, 2 },
                    { 5, 3 },
                    { 5, 4 },
                    { 5, 5 },
                    { 5, 6 },
                    { 5, 7 },
                    { 5, 8 },
                    { 5, 9 },
                    { 5, 10 },
                    { 5, 101 },
                    { 5, 102 },
                    { 5, 111 },
                    { 5, 112 },
                    { 6, 2 },
                    { 6, 3 },
                    { 6, 4 },
                    { 6, 5 },
                    { 6, 6 },
                    { 6, 7 },
                    { 6, 8 },
                    { 6, 9 },
                    { 6, 10 },
                    { 6, 101 },
                    { 6, 102 },
                    { 6, 111 },
                    { 6, 112 },
                    { 7, 2 },
                    { 7, 3 },
                    { 7, 4 },
                    { 7, 5 },
                    { 7, 6 },
                    { 7, 7 },
                    { 7, 8 },
                    { 7, 9 },
                    { 7, 10 },
                    { 7, 102 },
                    { 7, 112 },
                    { 8, 2 },
                    { 8, 3 },
                    { 8, 4 },
                    { 8, 5 },
                    { 8, 6 },
                    { 8, 7 },
                    { 8, 8 },
                    { 8, 9 },
                    { 8, 10 },
                    { 8, 101 },
                    { 8, 102 },
                    { 8, 111 },
                    { 8, 112 },
                    { 9, 2 },
                    { 9, 3 },
                    { 9, 4 },
                    { 9, 5 },
                    { 9, 6 },
                    { 9, 7 },
                    { 9, 8 },
                    { 9, 9 },
                    { 9, 10 },
                    { 9, 101 },
                    { 9, 102 },
                    { 9, 111 },
                    { 9, 112 },
                    { 10, 3 },
                    { 10, 4 },
                    { 10, 5 },
                    { 10, 6 },
                    { 10, 7 },
                    { 10, 8 },
                    { 10, 9 },
                    { 10, 10 },
                    { 11, 5 },
                    { 11, 6 },
                    { 11, 7 },
                    { 11, 8 },
                    { 11, 9 },
                    { 11, 10 },
                    { 12, 7 },
                    { 12, 8 },
                    { 12, 9 },
                    { 12, 10 },
                    { 13, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpansionClasses_ExpansionId",
                table: "ExpansionClasses",
                column: "ExpansionId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildExpansionConfigurations_ExpansionId",
                table: "GuildExpansionConfigurations",
                column: "ExpansionId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidMembers_ClassId",
                table: "RaidMembers",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidMembers_RaidId",
                table: "RaidMembers",
                column: "RaidId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidMembers_RoleId",
                table: "RaidMembers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Raids_ConfigurationId",
                table: "Raids",
                column: "ConfigurationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpansionClasses");

            migrationBuilder.DropTable(
                name: "RaidMembers");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Raids");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "GuildExpansionConfigurations");

            migrationBuilder.DropTable(
                name: "Expansions");
        }
    }
}
