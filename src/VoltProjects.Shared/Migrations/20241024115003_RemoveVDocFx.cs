using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVDocFx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_project_page_project_page_parent_page_id",
                table: "project_page");

            migrationBuilder.DropIndex(
                name: "ix_project_page_parent_page_id",
                table: "project_page");

            migrationBuilder.DeleteData(
                table: "doc_builder",
                keyColumn: "id",
                keyValue: "vdocfx");

            migrationBuilder.DropColumn(
                name: "parent_page_id",
                table: "project_page");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "parent_page_id",
                table: "project_page",
                type: "integer",
                nullable: true);

            migrationBuilder.InsertData(
                table: "doc_builder",
                columns: new[] { "id", "application", "arguments", "environment_variables", "name" },
                values: new object[] { "vdocfx", "vdocfx", new[] { "build", "--output-type PageJson", "--output {0}" }, new[] { "DOCS_GITHUB_TOKEN=" }, "VDocFx" });

            migrationBuilder.CreateIndex(
                name: "ix_project_page_parent_page_id",
                table: "project_page",
                column: "parent_page_id");

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_project_page_parent_page_id",
                table: "project_page",
                column: "parent_page_id",
                principalTable: "project_page",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
