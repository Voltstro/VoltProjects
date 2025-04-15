using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VoltProjects.Shared.Models;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class MenuAndTocImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_menu");

            migrationBuilder.DropColumn(
                name: "toc_item",
                table: "project_toc");

            migrationBuilder.CreateTable(
                name: "project_menu_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    project_version_id = table.Column<int>(type: "integer", nullable: false),
                    href = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    item_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_menu_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_menu_item_project_version_project_version_id",
                        column: x => x.project_version_id,
                        principalTable: "project_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_toc_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_toc_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    href = table.Column<string>(type: "text", nullable: true),
                    item_order = table.Column<int>(type: "integer", nullable: false),
                    parent_toc_item_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_toc_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_toc_item_project_toc_item_parent_toc_item_id",
                        column: x => x.parent_toc_item_id,
                        principalTable: "project_toc_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_toc_item_project_toc_project_toc_id",
                        column: x => x.project_toc_id,
                        principalTable: "project_toc",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_project_menu_item_project_version_id_href",
                table: "project_menu_item",
                columns: new[] { "project_version_id", "href" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_toc_item_parent_toc_item_id",
                table: "project_toc_item",
                column: "parent_toc_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_toc_item_project_toc_id_title_parent_toc_item_id_hr",
                table: "project_toc_item",
                columns: new[] { "project_toc_id", "title", "parent_toc_item_id", "href" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_menu_item");

            migrationBuilder.DropTable(
                name: "project_toc_item");

            migrationBuilder.AddColumn<LinkItem>(
                name: "toc_item",
                table: "project_toc",
                type: "jsonb",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "project_menu",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    project_version_id = table.Column<int>(type: "integer", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    link_item = table.Column<LinkItem>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_menu", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_menu_project_version_project_version_id",
                        column: x => x.project_version_id,
                        principalTable: "project_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_project_menu_project_version_id",
                table: "project_menu",
                column: "project_version_id",
                unique: true);
        }
    }
}
