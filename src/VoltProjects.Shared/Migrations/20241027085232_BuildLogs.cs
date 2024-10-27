using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class BuildLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "log_level",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_log_level", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_build_event_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    build_event_id = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    log_level_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_build_event_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_build_event_log_log_level_log_level_id",
                        column: x => x.log_level_id,
                        principalTable: "log_level",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_project_build_event_log_project_build_event_build_event_id",
                        column: x => x.build_event_id,
                        principalTable: "project_build_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "log_level",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Info" },
                    { 2, "Error" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_project_build_event_log_build_event_id",
                table: "project_build_event_log",
                column: "build_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_build_event_log_log_level_id",
                table: "project_build_event_log",
                column: "log_level_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_build_event_log");

            migrationBuilder.DropTable(
                name: "log_level");
        }
    }
}
