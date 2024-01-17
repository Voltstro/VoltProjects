using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class DocFx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "doc_builder",
                columns: new[] { "id", "application", "arguments", "environment_variables", "name" },
                values: new object[] { "docfx", "docfx", new[] { "build", "--exportRawModel" }, null, "DocFx" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "doc_builder",
                keyColumn: "id",
                keyValue: "docfx");
        }
    }
}
