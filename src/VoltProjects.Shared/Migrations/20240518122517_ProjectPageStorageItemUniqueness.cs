using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class ProjectPageStorageItemUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM public.project_page_storage_item AS a
	WHERE EXISTS (SELECT 1
					FROM public.project_page_storage_item AS b
					WHERE
						b.page_id = a.page_id AND 
						b.storage_item_id = a.storage_item_id AND 
						b.id > a.id
					);
");
            
            migrationBuilder.DropIndex(
                name: "ix_project_page_storage_item_page_id",
                table: "project_page_storage_item");

            migrationBuilder.CreateIndex(
                name: "ix_project_page_storage_item_page_id_storage_item_id",
                table: "project_page_storage_item",
                columns: new[] { "page_id", "storage_item_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_project_page_storage_item_page_id_storage_item_id",
                table: "project_page_storage_item");

            migrationBuilder.CreateIndex(
                name: "ix_project_page_storage_item_page_id",
                table: "project_page_storage_item",
                column: "page_id");
        }
    }
}
