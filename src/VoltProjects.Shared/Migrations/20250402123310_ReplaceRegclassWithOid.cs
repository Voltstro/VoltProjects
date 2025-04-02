using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceRegclassWithOid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "language_configuration",
                table: "project_page",
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
                name: "language_configuration",
                table: "project_page",
                type: "regconfig",
                nullable: false,
                defaultValueSql: "'english'",
                oldClrType: typeof(uint),
                oldType: "oid",
                oldDefaultValueSql: "'english'");
        }
    }
}
