using Microsoft.EntityFrameworkCore.Migrations;

namespace RaidBot.Migrations;

/// <inheritdoc />
public partial class UpdateRangedIcon : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: 3,
            column: "Icon",
            value: "<:rdps:1179280825196478555>");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: 3,
            column: "Icon",
            value: "<:rdps:945067186203033691>");
    }
}
