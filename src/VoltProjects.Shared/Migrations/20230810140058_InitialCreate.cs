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
            //Custom types
            migrationBuilder.Sql(@"
                CREATE TYPE upsertedtoc AS (rel text, toc jsonb);
                CREATE TYPE upsertedpage AS (publisheddate timestamptz, title text, titleDisplay boolean, aside boolean, wordCount int4, tocId int4, tocRel text, path text, description text, pageContent text);
            ");
            
            migrationBuilder.CreateTable(
                name: "DocBuilder",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocBuilder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    GitUrl = table.Column<string>(type: "text", nullable: false),
                    IconPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    VersionTag = table.Column<string>(type: "text", nullable: false),
                    GitBranch = table.Column<string>(type: "text", nullable: false),
                    GitTag = table.Column<string>(type: "text", nullable: true),
                    DocBuilderId = table.Column<string>(type: "text", nullable: false),
                    DocsPath = table.Column<string>(type: "text", nullable: false),
                    DocsBuiltPath = table.Column<string>(type: "text", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectVersion_DocBuilder_DocBuilderId",
                        column: x => x.DocBuilderId,
                        principalTable: "DocBuilder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectVersion_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectVersion_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectBuildEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectVersionId = table.Column<int>(type: "integer", nullable: false),
                    Successful = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    GitHash = table.Column<string>(type: "character varying(41)", maxLength: 41, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectBuildEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectBuildEvent_ProjectVersion_ProjectVersionId",
                        column: x => x.ProjectVersionId,
                        principalTable: "ProjectVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectVersionId = table.Column<int>(type: "integer", nullable: false),
                    LinkItem = table.Column<LinkItem>(type: "jsonb", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMenu_ProjectVersion_ProjectVersionId",
                        column: x => x.ProjectVersionId,
                        principalTable: "ProjectVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPreBuild",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectVersionId = table.Column<int>(type: "integer", nullable: false),
                    Command = table.Column<string>(type: "text", nullable: false),
                    Arguments = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPreBuild", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPreBuild_ProjectVersion_ProjectVersionId",
                        column: x => x.ProjectVersionId,
                        principalTable: "ProjectVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectToc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectVersionId = table.Column<int>(type: "integer", nullable: false),
                    TocRel = table.Column<string>(type: "text", nullable: false),
                    TocItem = table.Column<LinkItem>(type: "jsonb", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectToc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectToc_ProjectVersion_ProjectVersionId",
                        column: x => x.ProjectVersionId,
                        principalTable: "ProjectVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectVersionId = table.Column<int>(type: "integer", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Published = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    TitleDisplay = table.Column<bool>(type: "boolean", nullable: false),
                    WordCount = table.Column<int>(type: "integer", nullable: false),
                    ProjectTocId = table.Column<int>(type: "integer", nullable: true),
                    TocRel = table.Column<string>(type: "text", nullable: true),
                    Aside = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPage_ProjectToc_ProjectTocId",
                        column: x => x.ProjectTocId,
                        principalTable: "ProjectToc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectPage_ProjectVersion_ProjectVersionId",
                        column: x => x.ProjectVersionId,
                        principalTable: "ProjectVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPageContributor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    GitHubUserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPageContributor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPageContributor_ProjectPage_PageId",
                        column: x => x.PageId,
                        principalTable: "ProjectPage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DocBuilder",
                columns: new[] { "Id", "Name" },
                values: new object[] { "vdocfx", "VDocFx" });

            migrationBuilder.InsertData(
                table: "Language",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "en" });

            migrationBuilder.CreateIndex(
                name: "IX_Project_Name",
                table: "Project",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBuildEvent_ProjectVersionId",
                table: "ProjectBuildEvent",
                column: "ProjectVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMenu_ProjectVersionId",
                table: "ProjectMenu",
                column: "ProjectVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPage_ProjectTocId",
                table: "ProjectPage",
                column: "ProjectTocId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPage_ProjectVersionId_Path",
                table: "ProjectPage",
                columns: new[] { "ProjectVersionId", "Path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPageContributor_PageId",
                table: "ProjectPageContributor",
                column: "PageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPageContributor_PageId_GitHubUserId",
                table: "ProjectPageContributor",
                columns: new[] { "PageId", "GitHubUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPreBuild_ProjectVersionId",
                table: "ProjectPreBuild",
                column: "ProjectVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectToc_ProjectVersionId_TocRel",
                table: "ProjectToc",
                columns: new[] { "ProjectVersionId", "TocRel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersion_DocBuilderId",
                table: "ProjectVersion",
                column: "DocBuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersion_LanguageId",
                table: "ProjectVersion",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersion_ProjectId_VersionTag_LanguageId",
                table: "ProjectVersion",
                columns: new[] { "ProjectId", "VersionTag", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersion_ProjectId_VersionTag_LanguageId_IsDefault",
                table: "ProjectVersion",
                columns: new[] { "ProjectId", "VersionTag", "LanguageId", "IsDefault" },
                unique: true,
                filter: "\"IsDefault\" = true");
            
            //Upsert Project TOCs Function
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.""UpsertProjectTOCs""(""projectVersionId"" int4, toc upsertedtoc[])
RETURNS SETOF ""ProjectToc""
    LANGUAGE plpgsql
    AS $function$
BEGIN
    RETURN QUERY
        INSERT INTO public.""ProjectToc"" (""ProjectVersionId"", ""LastUpdateTime"", ""TocRel"", ""TocItem"")
        SELECT ""projectVersionId"", NOW(), toc.rel, toc.toc
            FROM unnest(toc) AS toc
        ON CONFLICT (""ProjectVersionId"", ""TocRel"")
        DO UPDATE SET 
            ""LastUpdateTime"" = NOW(),
            ""TocItem"" = EXCLUDED.""TocItem""
        RETURNING *;
END;
$function$
;

COMMENT ON FUNCTION public.""UpsertProjectTOCs"" IS 'Upserts a project''s TOCs';
            ");
            
            //Upsert Project Pages Function
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.""UpsertProjectPages""(""projectVersionId"" int4, page upsertedpage[])
RETURNS void
    LANGUAGE plpgsql
    AS $function$
BEGIN
    -- Upsert project pages, and store results in temp table
    CREATE TEMP TABLE tmp_projectpage
    AS WITH inserted AS (
        INSERT INTO public.""ProjectPage""
            (""ProjectVersionId"", ""Path"", ""Published"", ""PublishedDate"", ""Title"", ""TitleDisplay"", ""WordCount"", ""ProjectTocId"", ""TocRel"", ""Aside"", ""Description"", ""Content"", ""LastUpdateTime"")
        SELECT ""projectVersionId"", page.""path"", TRUE, page.publisheddate, page.title, page.titledisplay, page.wordcount, page.tocid, page.tocrel, page.aside, page.description, page.pagecontent, NOW()
            FROM unnest(page) AS page
        ON CONFLICT (""ProjectVersionId"", ""Path"")
        DO UPDATE SET 
            ""LastUpdateTime"" = NOW(),
            ""Published"" = EXCLUDED.""Published"",
            ""Title"" = EXCLUDED.""Title"",
            ""TitleDisplay"" = EXCLUDED.""TitleDisplay"",
            ""WordCount"" = EXCLUDED.""WordCount"",
            ""ProjectTocId"" = EXCLUDED.""ProjectTocId"",
            ""Aside"" = EXCLUDED.""Aside"", 
            ""Path"" = EXCLUDED.""Path"",
            ""Content"" = EXCLUDED.""Content""
        RETURNING ""Id""
    )
    SELECT ""Id"" FROM inserted;

    -- Any page that wasn't inserted/updated should be marked as unpublished (basically deleted)
    UPDATE public.""ProjectPage""
    SET ""Published"" = false
    WHERE ""Id"" NOT IN (SELECT ""Id"" FROM tmp_projectpage);

    -- Clean up
    DROP TABLE IF EXISTS tmp_projectpage;
    RETURN;	
END;
$function$
;

COMMENT ON FUNCTION public.""UpsertProjectPages"" IS 'Upserts a project''s pages.
Marks all un-edited pages as not published!';
            ");

            //Permissions
            migrationBuilder.Sql(@"
--Project
GRANT SELECT ON TABLE public.""Project"" TO vpbuilder, vpserver;

--ProjectVersion
GRANT SELECT ON TABLE public.""ProjectVersion"" TO vpbuilder, vpserver;

-- ProjectBuildEvent
GRANT SELECT, INSERT ON TABLE public.""ProjectBuildEvent"" TO vpbuilder;
GRANT SELECT ON TABLE public.""ProjectBuildEvent"" TO vpserver;

--ProjectMenu
GRANT UPDATE, SELECT, INSERT ON TABLE public.""ProjectMenu"" TO vpbuilder;
GRANT SELECT ON TABLE public.""ProjectMenu"" TO vpserver;

--ProjectPage
GRANT UPDATE, SELECT, INSERT ON TABLE public.""ProjectPage"" TO vpbuilder;
GRANT SELECT ON TABLE public.""ProjectPage"" TO vpserver;

--ProjectPageContributor
GRANT UPDATE, SELECT, INSERT ON TABLE public.""ProjectPageContributor"" TO vpbuilder;
GRANT SELECT ON TABLE public.""ProjectPageContributor"" TO voltweb;

--ProjectToc
GRANT UPDATE, SELECT, INSERT ON TABLE public.""ProjectToc"" TO vpbuilder;
GRANT SELECT ON TABLE public.""ProjectToc"" TO vpserver;

--ProjectPreBuild
GRANT SELECT ON TABLE public.""ProjectPreBuild"" TO vpbuilder;

--Functions
GRANT EXECUTE ON FUNCTION public.""UpsertProjectPages"" TO vpbuilder;
GRANT EXECUTE ON FUNCTION public.""UpsertProjectTOCs"" TO vpbuilder;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectBuildEvent");

            migrationBuilder.DropTable(
                name: "ProjectMenu");

            migrationBuilder.DropTable(
                name: "ProjectPageContributor");

            migrationBuilder.DropTable(
                name: "ProjectPreBuild");

            migrationBuilder.DropTable(
                name: "ProjectPage");

            migrationBuilder.DropTable(
                name: "ProjectToc");

            migrationBuilder.DropTable(
                name: "ProjectVersion");

            migrationBuilder.DropTable(
                name: "DocBuilder");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropTable(
                name: "Project");
        }
    }
}
