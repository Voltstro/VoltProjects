using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VoltProjects.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "DocView",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocView", x => x.Id);
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
                    GitBranch = table.Column<string>(type: "text", nullable: false),
                    IconPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectLanguage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectLanguage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectLanguage_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectLanguage_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    VersionTag = table.Column<string>(type: "text", nullable: false),
                    DocBuilderId = table.Column<string>(type: "text", nullable: false),
                    DocViewId = table.Column<string>(type: "text", nullable: false),
                    DocsPath = table.Column<string>(type: "text", nullable: false),
                    DocsBuiltPath = table.Column<string>(type: "text", nullable: false),
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
                        name: "FK_ProjectVersion_DocView_DocViewId",
                        column: x => x.DocViewId,
                        principalTable: "DocView",
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
                name: "PreBuildCommand",
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
                    table.PrimaryKey("PK_PreBuildCommand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreBuildCommand_ProjectVersion_ProjectVersionId",
                        column: x => x.ProjectVersionId,
                        principalTable: "ProjectVersion",
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

            migrationBuilder.CreateIndex(
                name: "IX_PreBuildCommand_ProjectVersionId",
                table: "PreBuildCommand",
                column: "ProjectVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBuildEvent_ProjectVersionId",
                table: "ProjectBuildEvent",
                column: "ProjectVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLanguage_LanguageId",
                table: "ProjectLanguage",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLanguage_ProjectId",
                table: "ProjectLanguage",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersion_DocBuilderId",
                table: "ProjectVersion",
                column: "DocBuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersion_DocViewId",
                table: "ProjectVersion",
                column: "DocViewId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersion_ProjectId",
                table: "ProjectVersion",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreBuildCommand");

            migrationBuilder.DropTable(
                name: "ProjectBuildEvent");

            migrationBuilder.DropTable(
                name: "ProjectLanguage");

            migrationBuilder.DropTable(
                name: "ProjectVersion");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropTable(
                name: "DocBuilder");

            migrationBuilder.DropTable(
                name: "DocView");

            migrationBuilder.DropTable(
                name: "Project");
        }
    }
}
