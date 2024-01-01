using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Metabar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "metabar",
                table: "project_page",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            
            //Drop upsert_project_pages
            migrationBuilder.Sql("DROP FUNCTION public.upsert_project_pages;");
            migrationBuilder.Sql("DROP TYPE public.upsertedpage;");
            
            //Create upsertedpage with metabar attribute
            migrationBuilder.Sql(@"
CREATE TYPE public.upsertedpage AS (
	published_date timestamptz,
	title text,
	title_display bool,
	git_url text,
	aside bool,
	metabar bool,
	word_count int4,
	toc_id int4,
	toc_rel text,
	path text,
	description text,
	page_content text);
            ");

            //Create upsert_project_pages, with metabar attribute (and updates other attributes that should be updated, but were not)
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.upsert_project_pages(version_id integer, page upsertedpage[])
 RETURNS void
 LANGUAGE plpgsql
AS $function$
BEGIN
    WITH inserted AS (
	    INSERT INTO public.project_page
	        (project_version_id, path, published, published_date, title, title_display, word_count, project_toc_id, toc_rel, git_url, aside, metabar, description, content)
	    SELECT version_id, page.path, TRUE, page.published_date, page.title, page.title_display, page.word_count, page.toc_id, page.toc_rel, page.git_url, page.aside, page.metabar, page.description, page.page_content
	        FROM unnest(page) AS page
	    ON CONFLICT (project_version_id, path)
	    DO UPDATE SET 
	        published = EXCLUDED.published,
	        published_date = EXCLUDED.published_date,
	        title = EXCLUDED.title,
	        title_display = EXCLUDED.title_display,
	        word_count = EXCLUDED.word_count,
	        project_toc_id = EXCLUDED.project_toc_id,
			toc_rel = EXCLUDED.toc_rel,
	        git_url = EXCLUDED.git_url,
	        aside = EXCLUDED.aside,
			metabar = EXCLUDED.metabar,
	        path = EXCLUDED.path,
			description = EXCLUDED.description,
	        content = EXCLUDED.content
	    RETURNING id
	)
	UPDATE public.project_page
	    SET published = false
	    WHERE id NOT IN (SELECT id FROM inserted)
	    AND project_version_id = version_id;
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
            migrationBuilder.DropColumn(
                name: "metabar",
                table: "project_page");
        }
    }
}
