using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class DateTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "creation_time",
                table: "project_version",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_update_time",
                table: "project_version",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_update_time",
                table: "project_toc",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "creation_time",
                table: "project_toc",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_update_time",
                table: "project_storage_item",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "creation_time",
                table: "project_storage_item",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "creation_time",
                table: "project_pre_build",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_update_time",
                table: "project_pre_build",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_update_time",
                table: "project_page",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "creation_time",
                table: "project_page",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_update_time",
                table: "project_menu",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "creation_time",
                table: "project_menu",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "project_build_event",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "creation_time",
                table: "project",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_update_time",
                table: "project",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            //Last update last update time function
            migrationBuilder.Sql(@"
CREATE FUNCTION trg_update_last_update_time() RETURNS TRIGGER LANGUAGE PLPGSQL AS $$
BEGIN
    NEW.last_update_time := now();
    RETURN NEW;
END;
$$;
");

            //Add trigger to all tables that need it
            migrationBuilder.Sql(@"
CREATE TRIGGER trg_project_last_update_time
    BEFORE INSERT OR UPDATE
    ON project
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_last_update_time();

CREATE TRIGGER trg_project_menu_last_update_time
    BEFORE INSERT OR UPDATE
    ON project_menu
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_last_update_time();

CREATE TRIGGER trg_project_page_last_update_time
    BEFORE INSERT OR UPDATE
    ON project_page
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_last_update_time();

CREATE TRIGGER trg_project_pre_build_last_update_time
    BEFORE INSERT OR UPDATE
    ON project_pre_build
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_last_update_time();

CREATE TRIGGER trg_project_storage_item_last_update_time
    BEFORE INSERT OR UPDATE
    ON project_storage_item
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_last_update_time();

CREATE TRIGGER trg_project_tco_last_update_time
    BEFORE INSERT OR UPDATE
    ON project_toc
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_last_update_time();

CREATE TRIGGER trg_project_version_last_update_time
    BEFORE INSERT OR UPDATE
    ON project_version
    FOR EACH ROW
    EXECUTE FUNCTION trg_update_last_update_time();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "creation_time",
                table: "project_version");

            migrationBuilder.DropColumn(
                name: "last_update_time",
                table: "project_version");

            migrationBuilder.DropColumn(
                name: "creation_time",
                table: "project_toc");

            migrationBuilder.DropColumn(
                name: "creation_time",
                table: "project_pre_build");

            migrationBuilder.DropColumn(
                name: "last_update_time",
                table: "project_pre_build");

            migrationBuilder.DropColumn(
                name: "creation_time",
                table: "project_page");

            migrationBuilder.DropColumn(
                name: "creation_time",
                table: "project_menu");

            migrationBuilder.DropColumn(
                name: "creation_time",
                table: "project");

            migrationBuilder.DropColumn(
                name: "last_update_time",
                table: "project");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_update_time",
                table: "project_toc",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_update_time",
                table: "project_storage_item",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "creation_time",
                table: "project_storage_item",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_update_time",
                table: "project_page",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_update_time",
                table: "project_menu",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "project_build_event",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");
        }
    }
}
