using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    /// <inheritdoc />
    public partial class UpsertProjectTocImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP FUNCTION IF EXISTS public.upsert_project_tocs;
CREATE OR REPLACE FUNCTION public.upsert_project_tocs(version_id integer, toc upsertedtoc[])
 RETURNS SETOF project_toc
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        INSERT INTO public.project_toc AS project_toc (project_version_id, toc_rel, toc_item)
        SELECT version_id, toc.rel, toc.toc 
            FROM unnest(toc) AS toc
        ON CONFLICT (project_version_id, toc_rel)
        DO UPDATE SET 
            toc_item = EXCLUDED.toc_item,
            toc_rel = EXCLUDED.toc_rel
        RETURNING *;
END;
$function$
;

COMMENT ON FUNCTION public.upsert_project_tocs IS 'Upserts a project''s TOCs';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
