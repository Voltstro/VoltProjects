using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class BuildSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "date",
                table: "project_build_event",
                newName: "last_update_time");

            migrationBuilder.AddColumn<DateTime>(
                name: "creation_time",
                table: "project_build_event",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.CreateTable(
                name: "project_build_schedule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    project_version_id = table.Column<int>(type: "integer", nullable: false),
                    cron = table.Column<string>(type: "text", nullable: false),
                    last_execute_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    ignore_build_events = table.Column<bool>(type: "boolean", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_build_schedule", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_build_schedule_project_version_project_version_id",
                        column: x => x.project_version_id,
                        principalTable: "project_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_project_build_schedule_project_version_id",
                table: "project_build_schedule",
                column: "project_version_id");

            migrationBuilder.Sql(@"
CREATE TRIGGER trg_project_build_schedule_last_update_time
    BEFORE INSERT OR UPDATE
    ON project_build_schedule
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_last_update_time();

CREATE TRIGGER trg_project_build_event_last_update_time
    BEFORE INSERT OR UPDATE
    ON project_build_event
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_last_update_time();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_build_schedule");

            migrationBuilder.DropColumn(
                name: "creation_time",
                table: "project_build_event");

            migrationBuilder.RenameColumn(
                name: "last_update_time",
                table: "project_build_event",
                newName: "date");
        }
    }
}
