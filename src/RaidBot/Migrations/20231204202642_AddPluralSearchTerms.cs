using Microsoft.EntityFrameworkCore.Migrations;

namespace RaidBot.Migrations;

/// <inheritdoc />
public partial class AddPluralSearchTerms : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: 1,
            column: "SearchTerms",
            value: "tanks;tank;t");

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: 2,
            column: "SearchTerms",
            value: "healers;healer;heals;heal;h");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: 1,
            column: "SearchTerms",
            value: "tank;t");

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: 2,
            column: "SearchTerms",
            value: "healer;heals;heal;h");
    }
}
