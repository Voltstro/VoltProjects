using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Searching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "language_configuration",
                table: "project_page",
                type: "regconfig",
                nullable: false,
                defaultValueSql: "'english'");

            migrationBuilder.AddColumn<uint>(
                name: "configuration",
                table: "language",
                type: "regconfig",
                nullable: false,
                defaultValueSql: "'english'");

            migrationBuilder.Sql(
                "CREATE INDEX ix_project_page_search_vector ON public.project_page USING gin (to_tsvector(language_configuration, content));");

            /*
            migrationBuilder.UpdateData(
                table: "language",
                keyColumn: "id",
                keyValue: 1,
                columns: new string[0],
                values: new object[0]);
                */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("ix_project_page_search_vector", "project_page");
            
            migrationBuilder.DropColumn(
                name: "language_configuration",
                table: "project_page");

            migrationBuilder.DropColumn(
                name: "configuration",
                table: "language");
        }
    }
}
