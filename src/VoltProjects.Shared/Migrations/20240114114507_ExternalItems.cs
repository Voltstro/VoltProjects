using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class ExternalItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_external_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_version_id = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "text", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_external_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_external_item_project_version_project_version_id",
                        column: x => x.project_version_id,
                        principalTable: "project_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_external_item_storage_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_external_item_id = table.Column<int>(type: "integer", nullable: false),
                    storage_item_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_external_item_storage_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_external_item_storage_item_project_external_item_pr",
                        column: x => x.project_external_item_id,
                        principalTable: "project_external_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_external_item_storage_item_project_storage_item_sto",
                        column: x => x.storage_item_id,
                        principalTable: "project_storage_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_project_external_item_project_version_id_path",
                table: "project_external_item",
                columns: new[] { "project_version_id", "path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_external_item_storage_item_project_external_item_id",
                table: "project_external_item_storage_item",
                columns: new[] { "project_external_item_id", "storage_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_external_item_storage_item_storage_item_id",
                table: "project_external_item_storage_item",
                column: "storage_item_id");
            
            migrationBuilder.Sql(@"
CREATE TRIGGER trg_project_external_item_last_update_time
    BEFORE INSERT OR UPDATE
    ON project_external_item
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_last_update_time();"
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_external_item_storage_item");

            migrationBuilder.DropTable(
                name: "project_external_item");
        }
    }
}
