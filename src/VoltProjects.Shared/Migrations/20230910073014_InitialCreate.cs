using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VoltProjects.Shared.Models;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Custom Types
            migrationBuilder.Sql(@"
CREATE TYPE upsertedtoc AS (rel text, toc jsonb);

CREATE TYPE public.upsertedpage AS (
	published_date timestamptz,
	title text,
	title_display bool,
	git_url text,
	aside bool,
	word_count int4,
	toc_id int4,
	toc_rel text,
	path text,
	description text,
	page_content text);
            ");
            
            migrationBuilder.CreateTable(
                name: "doc_builder",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    application = table.Column<string>(type: "text", nullable: false),
                    arguments = table.Column<string[]>(type: "text[]", nullable: true),
                    environment_variables = table.Column<string[]>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_doc_builder", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "language",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_language", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    short_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    git_url = table.Column<string>(type: "text", nullable: false),
                    icon_path = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_version",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    version_tag = table.Column<string>(type: "text", nullable: false),
                    git_branch = table.Column<string>(type: "text", nullable: false),
                    git_tag = table.Column<string>(type: "text", nullable: true),
                    doc_builder_id = table.Column<string>(type: "text", nullable: false),
                    docs_path = table.Column<string>(type: "text", nullable: false),
                    docs_built_path = table.Column<string>(type: "text", nullable: false),
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_version", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_version_doc_builder_doc_builder_id",
                        column: x => x.doc_builder_id,
                        principalTable: "doc_builder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_version_language_language_id",
                        column: x => x.language_id,
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_version_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_build_event",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    project_version_id = table.Column<int>(type: "integer", nullable: false),
                    builder_ver = table.Column<int>(type: "integer", nullable: false),
                    successful = table.Column<bool>(type: "boolean", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    git_hash = table.Column<string>(type: "character varying(41)", maxLength: 41, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_build_event", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_build_event_project_version_project_version_id",
                        column: x => x.project_version_id,
                        principalTable: "project_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_menu",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    project_version_id = table.Column<int>(type: "integer", nullable: false),
                    link_item = table.Column<LinkItem>(type: "jsonb", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_menu", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_menu_project_version_project_version_id",
                        column: x => x.project_version_id,
                        principalTable: "project_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_pre_build",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    project_version_id = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    command = table.Column<string>(type: "text", nullable: false),
                    arguments = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_pre_build", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_pre_build_project_version_project_version_id",
                        column: x => x.project_version_id,
                        principalTable: "project_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_storage_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_version_id = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "text", nullable: false),
                    hash = table.Column<string>(type: "text", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_storage_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_storage_item_project_version_project_version_id",
                        column: x => x.project_version_id,
                        principalTable: "project_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_toc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    project_version_id = table.Column<int>(type: "integer", nullable: false),
                    toc_rel = table.Column<string>(type: "text", nullable: false),
                    toc_item = table.Column<LinkItem>(type: "jsonb", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_toc", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_toc_project_version_project_version_id",
                        column: x => x.project_version_id,
                        principalTable: "project_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_page",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    project_version_id = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "text", nullable: false),
                    published = table.Column<bool>(type: "boolean", nullable: false),
                    published_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    parent_page_id = table.Column<int>(type: "integer", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    title_display = table.Column<bool>(type: "boolean", nullable: false),
                    word_count = table.Column<int>(type: "integer", nullable: true),
                    project_toc_id = table.Column<int>(type: "integer", nullable: true),
                    toc_rel = table.Column<string>(type: "text", nullable: true),
                    git_url = table.Column<string>(type: "text", nullable: true),
                    aside = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_page", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_page_project_page_parent_page_id",
                        column: x => x.parent_page_id,
                        principalTable: "project_page",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_project_page_project_toc_project_toc_id",
                        column: x => x.project_toc_id,
                        principalTable: "project_toc",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_project_page_project_version_project_version_id",
                        column: x => x.project_version_id,
                        principalTable: "project_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_page_contributor",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    page_id = table.Column<int>(type: "integer", nullable: false),
                    git_hub_user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_page_contributor", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_page_contributor_project_page_page_id",
                        column: x => x.page_id,
                        principalTable: "project_page",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_page_storage_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<int>(type: "integer", nullable: false),
                    storage_item_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_page_storage_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_page_storage_item_project_page_page_id",
                        column: x => x.page_id,
                        principalTable: "project_page",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_page_storage_item_project_storage_item_storage_item",
                        column: x => x.storage_item_id,
                        principalTable: "project_storage_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "doc_builder",
                columns: new[] { "id", "application", "arguments", "environment_variables", "name" },
                values: new object[] { "vdocfx", "vdocfx", new[] { "build", "--output-type PageJson", "--output {0}" }, new[] { "DOCS_GITHUB_TOKEN=" }, "VDocFx" });

            migrationBuilder.InsertData(
                table: "language",
                columns: new[] { "id", "name" },
                values: new object[] { 1, "en" });

            migrationBuilder.CreateIndex(
                name: "ix_project_name",
                table: "project",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_build_event_project_version_id",
                table: "project_build_event",
                column: "project_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_menu_project_version_id",
                table: "project_menu",
                column: "project_version_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_page_parent_page_id",
                table: "project_page",
                column: "parent_page_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_page_project_toc_id",
                table: "project_page",
                column: "project_toc_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_page_project_version_id_path",
                table: "project_page",
                columns: new[] { "project_version_id", "path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_page_contributor_page_id",
                table: "project_page_contributor",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_page_contributor_page_id_git_hub_user_id",
                table: "project_page_contributor",
                columns: new[] { "page_id", "git_hub_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_page_storage_item_page_id",
                table: "project_page_storage_item",
                column: "page_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_page_storage_item_storage_item_id",
                table: "project_page_storage_item",
                column: "storage_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_pre_build_project_version_id",
                table: "project_pre_build",
                column: "project_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_storage_item_project_version_id_path",
                table: "project_storage_item",
                columns: new[] { "project_version_id", "path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_toc_project_version_id_toc_rel",
                table: "project_toc",
                columns: new[] { "project_version_id", "toc_rel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_version_doc_builder_id",
                table: "project_version",
                column: "doc_builder_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_version_language_id",
                table: "project_version",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_version_project_id_version_tag_language_id",
                table: "project_version",
                columns: new[] { "project_id", "version_tag", "language_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_version_project_id_version_tag_language_id_is_defau",
                table: "project_version",
                columns: new[] { "project_id", "version_tag", "language_id", "is_default" },
                unique: true,
                filter: "is_default = true");
            
            //Upsert Project TOCs Function
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.upsert_project_tocs(version_id integer, toc upsertedtoc[])
 RETURNS SETOF project_toc
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        INSERT INTO public.project_toc AS project_toc
            (project_version_id, last_update_time, toc_rel, toc_item)
        SELECT version_id, NOW(), toc.rel, toc.toc
            FROM unnest(toc) AS toc
        ON CONFLICT (project_version_id, toc_rel)
        DO UPDATE SET 
            last_update_time = NOW(),
            toc_item = EXCLUDED.toc_item
        RETURNING *;
END;
$function$
;

COMMENT ON FUNCTION public.upsert_project_tocs IS 'Upserts a project''s TOCs';
            ");
            
            //Upsert Pages Function
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.upsert_project_pages(version_id integer, page upsertedpage[])
 RETURNS void
 LANGUAGE plpgsql
AS $function$
BEGIN
    -- Upsert project pages, and store results in temp table
    CREATE TEMP TABLE tmp_projectpage
    AS WITH inserted AS (
        INSERT INTO public.project_page
            (project_version_id, path, published, published_date, title, title_display, word_count, project_toc_id, toc_rel, git_url, aside, description, content, last_update_time)
        SELECT version_id, page.path, TRUE, page.published_date, page.title, page.title_display, page.word_count, page.toc_id, page.toc_rel, page.git_url, page.aside, page.description, page.page_content, NOW()
            FROM unnest(page) AS page
        ON CONFLICT (project_version_id, path)
        DO UPDATE SET 
            last_update_time = NOW(),
            published = EXCLUDED.published,
            published_date = EXCLUDED.published_date,
            title = EXCLUDED.title,
            title_display = EXCLUDED.title_display,
            word_count = EXCLUDED.word_count,
            project_toc_id = EXCLUDED.project_toc_id,
            git_url = EXCLUDED.git_url,
            aside = EXCLUDED.aside, 
            path = EXCLUDED.path,
            content = EXCLUDED.content
        RETURNING id
    )
    SELECT id FROM inserted;

    -- Any page that wasn't inserted/updated should be marked as unpublished (basically deleted)
    UPDATE public.project_page
    SET published = false
    WHERE id NOT IN (SELECT id FROM tmp_projectpage);

    -- Clean up
    DROP TABLE IF EXISTS tmp_projectpage;
    RETURN;	
END;
$function$
;

COMMENT ON FUNCTION public.upsert_project_pages IS 'Upserts a project''s pages.
Marks all un-edited pages as not published!';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_build_event");

            migrationBuilder.DropTable(
                name: "project_menu");

            migrationBuilder.DropTable(
                name: "project_page_contributor");

            migrationBuilder.DropTable(
                name: "project_page_storage_item");

            migrationBuilder.DropTable(
                name: "project_pre_build");

            migrationBuilder.DropTable(
                name: "project_page");

            migrationBuilder.DropTable(
                name: "project_storage_item");

            migrationBuilder.DropTable(
                name: "project_toc");

            migrationBuilder.DropTable(
                name: "project_version");

            migrationBuilder.DropTable(
                name: "doc_builder");

            migrationBuilder.DropTable(
                name: "language");

            migrationBuilder.DropTable(
                name: "project");
        }
    }
}
