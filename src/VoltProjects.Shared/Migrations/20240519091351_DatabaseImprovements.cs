using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_project_build_event_project_version_project_version_id",
                table: "project_build_event");

            migrationBuilder.DropForeignKey(
                name: "fk_project_external_item_project_version_project_version_id",
                table: "project_external_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_external_item_storage_item_project_external_item_pr",
                table: "project_external_item_storage_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_external_item_storage_item_project_storage_item_sto",
                table: "project_external_item_storage_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_menu_project_version_project_version_id",
                table: "project_menu");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_project_page_parent_page_id",
                table: "project_page");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_project_toc_project_toc_id",
                table: "project_page");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_project_version_project_version_id",
                table: "project_page");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_contributor_project_page_page_id",
                table: "project_page_contributor");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_storage_item_project_page_page_id",
                table: "project_page_storage_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_storage_item_project_storage_item_storage_item",
                table: "project_page_storage_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_pre_build_project_version_project_version_id",
                table: "project_pre_build");

            migrationBuilder.DropForeignKey(
                name: "fk_project_storage_item_project_version_project_version_id",
                table: "project_storage_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_toc_project_version_project_version_id",
                table: "project_toc");

            migrationBuilder.DropForeignKey(
                name: "fk_project_version_doc_builder_doc_builder_id",
                table: "project_version");

            migrationBuilder.DropForeignKey(
                name: "fk_project_version_language_language_id",
                table: "project_version");

            migrationBuilder.DropForeignKey(
                name: "fk_project_version_project_project_id",
                table: "project_version");

            migrationBuilder.AddCheckConstraint(
                name: "ck_toc_nullability_same",
                table: "project_page",
                sql: "(project_toc_id IS NULL AND toc_rel IS NULL) OR (project_toc_id IS NOT NULL AND toc_rel IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "fk_project_build_event_project_version_project_version_id",
                table: "project_build_event",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_external_item_project_version_project_version_id",
                table: "project_external_item",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_external_item_storage_item_project_external_item_pr",
                table: "project_external_item_storage_item",
                column: "project_external_item_id",
                principalTable: "project_external_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_external_item_storage_item_project_storage_item_sto",
                table: "project_external_item_storage_item",
                column: "storage_item_id",
                principalTable: "project_storage_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_menu_project_version_project_version_id",
                table: "project_menu",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_project_page_parent_page_id",
                table: "project_page",
                column: "parent_page_id",
                principalTable: "project_page",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_project_toc_project_toc_id",
                table: "project_page",
                column: "project_toc_id",
                principalTable: "project_toc",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_project_version_project_version_id",
                table: "project_page",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_contributor_project_page_page_id",
                table: "project_page_contributor",
                column: "page_id",
                principalTable: "project_page",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_storage_item_project_page_page_id",
                table: "project_page_storage_item",
                column: "page_id",
                principalTable: "project_page",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_storage_item_project_storage_item_storage_item",
                table: "project_page_storage_item",
                column: "storage_item_id",
                principalTable: "project_storage_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_pre_build_project_version_project_version_id",
                table: "project_pre_build",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_storage_item_project_version_project_version_id",
                table: "project_storage_item",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_toc_project_version_project_version_id",
                table: "project_toc",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_version_doc_builder_doc_builder_id",
                table: "project_version",
                column: "doc_builder_id",
                principalTable: "doc_builder",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_version_language_language_id",
                table: "project_version",
                column: "language_id",
                principalTable: "language",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_project_version_project_project_id",
                table: "project_version",
                column: "project_id",
                principalTable: "project",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_project_build_event_project_version_project_version_id",
                table: "project_build_event");

            migrationBuilder.DropForeignKey(
                name: "fk_project_external_item_project_version_project_version_id",
                table: "project_external_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_external_item_storage_item_project_external_item_pr",
                table: "project_external_item_storage_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_external_item_storage_item_project_storage_item_sto",
                table: "project_external_item_storage_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_menu_project_version_project_version_id",
                table: "project_menu");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_project_page_parent_page_id",
                table: "project_page");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_project_toc_project_toc_id",
                table: "project_page");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_project_version_project_version_id",
                table: "project_page");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_contributor_project_page_page_id",
                table: "project_page_contributor");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_storage_item_project_page_page_id",
                table: "project_page_storage_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_page_storage_item_project_storage_item_storage_item",
                table: "project_page_storage_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_pre_build_project_version_project_version_id",
                table: "project_pre_build");

            migrationBuilder.DropForeignKey(
                name: "fk_project_storage_item_project_version_project_version_id",
                table: "project_storage_item");

            migrationBuilder.DropForeignKey(
                name: "fk_project_toc_project_version_project_version_id",
                table: "project_toc");

            migrationBuilder.DropForeignKey(
                name: "fk_project_version_doc_builder_doc_builder_id",
                table: "project_version");

            migrationBuilder.DropForeignKey(
                name: "fk_project_version_language_language_id",
                table: "project_version");

            migrationBuilder.DropForeignKey(
                name: "fk_project_version_project_project_id",
                table: "project_version");

            migrationBuilder.DropCheckConstraint(
                name: "ck_toc_nullability_same",
                table: "project_page");

            migrationBuilder.AddForeignKey(
                name: "fk_project_build_event_project_version_project_version_id",
                table: "project_build_event",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_external_item_project_version_project_version_id",
                table: "project_external_item",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_external_item_storage_item_project_external_item_pr",
                table: "project_external_item_storage_item",
                column: "project_external_item_id",
                principalTable: "project_external_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_external_item_storage_item_project_storage_item_sto",
                table: "project_external_item_storage_item",
                column: "storage_item_id",
                principalTable: "project_storage_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_menu_project_version_project_version_id",
                table: "project_menu",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_project_page_parent_page_id",
                table: "project_page",
                column: "parent_page_id",
                principalTable: "project_page",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_project_toc_project_toc_id",
                table: "project_page",
                column: "project_toc_id",
                principalTable: "project_toc",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_project_version_project_version_id",
                table: "project_page",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_contributor_project_page_page_id",
                table: "project_page_contributor",
                column: "page_id",
                principalTable: "project_page",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_storage_item_project_page_page_id",
                table: "project_page_storage_item",
                column: "page_id",
                principalTable: "project_page",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_page_storage_item_project_storage_item_storage_item",
                table: "project_page_storage_item",
                column: "storage_item_id",
                principalTable: "project_storage_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_pre_build_project_version_project_version_id",
                table: "project_pre_build",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_storage_item_project_version_project_version_id",
                table: "project_storage_item",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_toc_project_version_project_version_id",
                table: "project_toc",
                column: "project_version_id",
                principalTable: "project_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_version_doc_builder_doc_builder_id",
                table: "project_version",
                column: "doc_builder_id",
                principalTable: "doc_builder",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_version_language_language_id",
                table: "project_version",
                column: "language_id",
                principalTable: "language",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_project_version_project_project_id",
                table: "project_version",
                column: "project_id",
                principalTable: "project",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
