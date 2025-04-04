using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceRegclassInLanguageWithOid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "configuration",
                table: "language",
                type: "oid",
                nullable: false,
                defaultValueSql: "'english'",
                oldClrType: typeof(uint),
                oldType: "regconfig",
                oldDefaultValueSql: "'english'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "configuration",
                table: "language",
                type: "regconfig",
                nullable: false,
                defaultValueSql: "'english'",
                oldClrType: typeof(uint),
                oldType: "oid",
                oldDefaultValueSql: "'english'");
        }
    }
}
