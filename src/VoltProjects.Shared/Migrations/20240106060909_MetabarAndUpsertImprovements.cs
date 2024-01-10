using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class MetabarAndUpsertImprovements : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "page_hash",
                table: "project_page",
                type: "text",
                nullable: true);
            
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
	page_content text,
	page_hash text);
            ");

            //Create upsert_project_pages, with metabar attribute (and updates other attributes that should be updated, but were not)
            migrationBuilder.Sql(@"
DROP FUNCTION upsert_project_pages;
CREATE OR REPLACE FUNCTION public.upsert_project_pages(version_id integer, pages upsertedpage[])
 RETURNS void
 LANGUAGE plpgsql
AS $function$
BEGIN
	DROP TABLE IF EXISTS temp_pages;
	CREATE TEMPORARY TABLE temp_pages AS
		 SELECT
		 	version_id,
		 	page.PATH AS path,
		 	TRUE AS published,
		 	page.published_date,
		 	page.title,
		 	page.title_display,
		 	page.word_count,
		 	page.toc_id,
		 	page.toc_rel,
		 	page.git_url,
		 	page.aside,
		 	page.metabar,
		 	page.description,
		 	page.page_content,
		 	page.page_hash
	     FROM unnest(pages) AS page;
	
    INSERT INTO public.project_page
        (project_version_id, path, published, published_date, title, title_display, word_count, project_toc_id, toc_rel, git_url, aside, metabar, description, content, page_hash)
    	SELECT * FROM temp_pages AS page
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
        content = EXCLUDED.CONTENT,
        page_hash = EXCLUDED.page_hash
		WHERE project_page.page_hash != EXCLUDED.page_hash;
	
	UPDATE public.project_page
	SET published=false
	WHERE
		project_version_id = version_id
	AND ""path"" NOT IN (SELECT path FROM temp_pages WHERE PATH IS NOT NULL);
	
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

            migrationBuilder.DropColumn(
                name: "page_hash",
                table: "project_page");
        }
    }
}
