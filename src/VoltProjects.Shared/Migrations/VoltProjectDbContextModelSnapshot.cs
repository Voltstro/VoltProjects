﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

#nullable disable

namespace VoltProjects.Shared.Migrations
{
    [DbContext(typeof(VoltProjectDbContext))]
    partial class VoltProjectDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:CollationDefinition:vp_collation_nondeterministic", "en-u-ks-primary,en-u-ks-primary,icu,False")
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("VoltProjects.Shared.Models.DocBuilder", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<string>("Application")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("application");

                    b.Property<string[]>("Arguments")
                        .HasColumnType("text[]")
                        .HasColumnName("arguments");

                    b.Property<string[]>("EnvironmentVariables")
                        .HasColumnType("text[]")
                        .HasColumnName("environment_variables");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_doc_builder");

                    b.ToTable("doc_builder", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "vdocfx",
                            Application = "vdocfx",
                            Arguments = new[] { "build", "--output-type PageJson", "--output {0}" },
                            EnvironmentVariables = new[] { "DOCS_GITHUB_TOKEN=" },
                            Name = "VDocFx"
                        },
                        new
                        {
                            Id = "docfx",
                            Application = "docfx",
                            Arguments = new[] { "build", "--exportRawModel" },
                            Name = "DocFx"
                        },
                        new
                        {
                            Id = "mkdocs",
                            Application = "python",
                            Arguments = new[] { "-m mkdocs", "build" },
                            Name = "MkDocs"
                        });
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.Language", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<uint>("Configuration")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("oid")
                        .HasColumnName("configuration")
                        .HasDefaultValueSql("'english'");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_language");

                    b.ToTable("language", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Configuration = 0u,
                            Name = "en"
                        });
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_time")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("display_name");

                    b.Property<string>("GitUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("git_url");

                    b.Property<string>("IconPath")
                        .HasColumnType("text")
                        .HasColumnName("icon_path");

                    b.Property<DateTime>("LastUpdateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_update_time")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name")
                        .UseCollation("vp_collation_nondeterministic");

                    b.Property<string>("ShortName")
                        .HasColumnType("text")
                        .HasColumnName("short_name");

                    b.HasKey("Id")
                        .HasName("pk_project");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_project_name");

                    b.ToTable("project", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectBuildEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<int>("BuilderVer")
                        .HasColumnType("integer")
                        .HasColumnName("builder_ver");

                    b.Property<DateTime>("Date")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("GitHash")
                        .IsRequired()
                        .HasMaxLength(41)
                        .HasColumnType("character varying(41)")
                        .HasColumnName("git_hash");

                    b.Property<string>("Message")
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.Property<int>("ProjectVersionId")
                        .HasColumnType("integer")
                        .HasColumnName("project_version_id");

                    b.Property<bool>("Successful")
                        .HasColumnType("boolean")
                        .HasColumnName("successful");

                    b.HasKey("Id")
                        .HasName("pk_project_build_event");

                    b.HasIndex("ProjectVersionId")
                        .HasDatabaseName("ix_project_build_event_project_version_id");

                    b.ToTable("project_build_event", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectExternalItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_time")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime>("LastUpdateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_update_time")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("path");

                    b.Property<int>("ProjectVersionId")
                        .HasColumnType("integer")
                        .HasColumnName("project_version_id");

                    b.HasKey("Id")
                        .HasName("pk_project_external_item");

                    b.HasIndex("ProjectVersionId", "Path")
                        .IsUnique()
                        .HasDatabaseName("ix_project_external_item_project_version_id_path");

                    b.ToTable("project_external_item", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectExternalItemStorageItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ProjectExternalItemId")
                        .HasColumnType("integer")
                        .HasColumnName("project_external_item_id");

                    b.Property<int>("StorageItemId")
                        .HasColumnType("integer")
                        .HasColumnName("storage_item_id");

                    b.HasKey("Id")
                        .HasName("pk_project_external_item_storage_item");

                    b.HasIndex("StorageItemId")
                        .HasDatabaseName("ix_project_external_item_storage_item_storage_item_id");

                    b.HasIndex("ProjectExternalItemId", "StorageItemId")
                        .IsUnique()
                        .HasDatabaseName("ix_project_external_item_storage_item_project_external_item_id");

                    b.ToTable("project_external_item_storage_item", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectMenu", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_time")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime>("LastUpdateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_update_time")
                        .HasDefaultValueSql("now()");

                    b.Property<LinkItem>("LinkItem")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("link_item");

                    b.Property<int>("ProjectVersionId")
                        .HasColumnType("integer")
                        .HasColumnName("project_version_id");

                    b.HasKey("Id")
                        .HasName("pk_project_menu");

                    b.HasIndex("ProjectVersionId")
                        .IsUnique()
                        .HasDatabaseName("ix_project_menu_project_version_id");

                    b.ToTable("project_menu", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectPage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<bool>("Aside")
                        .HasColumnType("boolean")
                        .HasColumnName("aside");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<DateTime>("CreationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_time")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("GitUrl")
                        .HasColumnType("text")
                        .HasColumnName("git_url");

                    b.Property<uint>("LanguageConfiguration")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("oid")
                        .HasColumnName("language_configuration")
                        .HasDefaultValueSql("'english'");

                    b.Property<DateTime>("LastUpdateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_update_time")
                        .HasDefaultValueSql("now()");

                    b.Property<bool>("Metabar")
                        .HasColumnType("boolean")
                        .HasColumnName("metabar");

                    b.Property<string>("PageHash")
                        .HasColumnType("text")
                        .HasColumnName("page_hash");

                    b.Property<int?>("ParentPageId")
                        .HasColumnType("integer")
                        .HasColumnName("parent_page_id");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("path");

                    b.Property<int?>("ProjectTocId")
                        .HasColumnType("integer")
                        .HasColumnName("project_toc_id");

                    b.Property<int>("ProjectVersionId")
                        .HasColumnType("integer")
                        .HasColumnName("project_version_id");

                    b.Property<bool>("Published")
                        .HasColumnType("boolean")
                        .HasColumnName("published");

                    b.Property<DateTime>("PublishedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("published_date");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.Property<bool>("TitleDisplay")
                        .HasColumnType("boolean")
                        .HasColumnName("title_display");

                    b.Property<string>("TocRel")
                        .HasColumnType("text")
                        .HasColumnName("toc_rel");

                    b.Property<int?>("WordCount")
                        .HasColumnType("integer")
                        .HasColumnName("word_count");

                    b.HasKey("Id")
                        .HasName("pk_project_page");

                    b.HasIndex("ParentPageId")
                        .HasDatabaseName("ix_project_page_parent_page_id");

                    b.HasIndex("ProjectTocId")
                        .HasDatabaseName("ix_project_page_project_toc_id");

                    b.HasIndex("ProjectVersionId", "Path")
                        .IsUnique()
                        .HasDatabaseName("ix_project_page_project_version_id_path");

                    b.ToTable("project_page", null, t =>
                        {
                            t.HasCheckConstraint("ck_toc_nullability_same", "(project_toc_id IS NULL AND toc_rel IS NULL) OR (project_toc_id IS NOT NULL AND toc_rel IS NOT NULL)");
                        });
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectPageContributor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<string>("GitHubUserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("git_hub_user_id");

                    b.Property<int>("PageId")
                        .HasColumnType("integer")
                        .HasColumnName("page_id");

                    b.HasKey("Id")
                        .HasName("pk_project_page_contributor");

                    b.HasIndex("PageId")
                        .IsUnique()
                        .HasDatabaseName("ix_project_page_contributor_page_id");

                    b.HasIndex("PageId", "GitHubUserId")
                        .IsUnique()
                        .HasDatabaseName("ix_project_page_contributor_page_id_git_hub_user_id");

                    b.ToTable("project_page_contributor", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectPageStorageItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("PageId")
                        .HasColumnType("integer")
                        .HasColumnName("page_id");

                    b.Property<int>("StorageItemId")
                        .HasColumnType("integer")
                        .HasColumnName("storage_item_id");

                    b.HasKey("Id")
                        .HasName("pk_project_page_storage_item");

                    b.HasIndex("StorageItemId")
                        .HasDatabaseName("ix_project_page_storage_item_storage_item_id");

                    b.HasIndex("PageId", "StorageItemId")
                        .IsUnique()
                        .HasDatabaseName("ix_project_page_storage_item_page_id_storage_item_id");

                    b.ToTable("project_page_storage_item", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectPreBuild", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<string>("Arguments")
                        .HasColumnType("text")
                        .HasColumnName("arguments");

                    b.Property<string>("Command")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("command");

                    b.Property<DateTime>("CreationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_time")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime>("LastUpdateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_update_time")
                        .HasDefaultValueSql("now()");

                    b.Property<int>("Order")
                        .HasColumnType("integer")
                        .HasColumnName("order");

                    b.Property<int>("ProjectVersionId")
                        .HasColumnType("integer")
                        .HasColumnName("project_version_id");

                    b.HasKey("Id")
                        .HasName("pk_project_pre_build");

                    b.HasIndex("ProjectVersionId")
                        .HasDatabaseName("ix_project_pre_build_project_version_id");

                    b.ToTable("project_pre_build", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectStorageItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_time")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("hash");

                    b.Property<DateTime>("LastUpdateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_update_time")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("path");

                    b.Property<int>("ProjectVersionId")
                        .HasColumnType("integer")
                        .HasColumnName("project_version_id");

                    b.HasKey("Id")
                        .HasName("pk_project_storage_item");

                    b.HasIndex("ProjectVersionId", "Path")
                        .IsUnique()
                        .HasDatabaseName("ix_project_storage_item_project_version_id_path");

                    b.ToTable("project_storage_item", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectToc", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_time")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime>("LastUpdateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_update_time")
                        .HasDefaultValueSql("now()");

                    b.Property<int>("ProjectVersionId")
                        .HasColumnType("integer")
                        .HasColumnName("project_version_id");

                    b.Property<LinkItem>("TocItem")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("toc_item");

                    b.Property<string>("TocRel")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("toc_rel");

                    b.HasKey("Id")
                        .HasName("pk_project_toc");

                    b.HasIndex("ProjectVersionId", "TocRel")
                        .IsUnique()
                        .HasDatabaseName("ix_project_toc_project_version_id_toc_rel");

                    b.ToTable("project_toc", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectVersion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("creation_time")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("DocBuilderId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("doc_builder_id");

                    b.Property<string>("DocsBuiltPath")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("docs_built_path");

                    b.Property<string>("DocsPath")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("docs_path");

                    b.Property<string>("GitBranch")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("git_branch");

                    b.Property<string>("GitTag")
                        .HasColumnType("text")
                        .HasColumnName("git_tag");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("boolean")
                        .HasColumnName("is_default");

                    b.Property<int>("LanguageId")
                        .HasColumnType("integer")
                        .HasColumnName("language_id");

                    b.Property<DateTime>("LastUpdateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_update_time")
                        .HasDefaultValueSql("now()");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer")
                        .HasColumnName("project_id");

                    b.Property<string>("VersionTag")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("version_tag")
                        .UseCollation("vp_collation_nondeterministic");

                    b.HasKey("Id")
                        .HasName("pk_project_version");

                    b.HasIndex("DocBuilderId")
                        .HasDatabaseName("ix_project_version_doc_builder_id");

                    b.HasIndex("LanguageId")
                        .HasDatabaseName("ix_project_version_language_id");

                    b.HasIndex("ProjectId", "VersionTag", "LanguageId")
                        .IsUnique()
                        .HasDatabaseName("ix_project_version_project_id_version_tag_language_id");

                    b.HasIndex("ProjectId", "VersionTag", "LanguageId", "IsDefault")
                        .IsUnique()
                        .HasDatabaseName("ix_project_version_project_id_version_tag_language_id_is_defau")
                        .HasFilter("is_default = true");

                    b.ToTable("project_version", (string)null);
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectBuildEvent", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.ProjectVersion", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectVersionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_build_event_project_version_project_version_id");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectExternalItem", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.ProjectVersion", "ProjectVersion")
                        .WithMany()
                        .HasForeignKey("ProjectVersionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_external_item_project_version_project_version_id");

                    b.Navigation("ProjectVersion");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectExternalItemStorageItem", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.ProjectExternalItem", "ProjectExternalItem")
                        .WithMany()
                        .HasForeignKey("ProjectExternalItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_external_item_storage_item_project_external_item_pr");

                    b.HasOne("VoltProjects.Shared.Models.ProjectStorageItem", "StorageItem")
                        .WithMany()
                        .HasForeignKey("StorageItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_external_item_storage_item_project_storage_item_sto");

                    b.Navigation("ProjectExternalItem");

                    b.Navigation("StorageItem");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectMenu", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.ProjectVersion", "ProjectVersion")
                        .WithMany()
                        .HasForeignKey("ProjectVersionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_menu_project_version_project_version_id");

                    b.Navigation("ProjectVersion");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectPage", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.ProjectPage", "ParentPage")
                        .WithMany()
                        .HasForeignKey("ParentPageId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_project_page_project_page_parent_page_id");

                    b.HasOne("VoltProjects.Shared.Models.ProjectToc", "ProjectToc")
                        .WithMany()
                        .HasForeignKey("ProjectTocId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_project_page_project_toc_project_toc_id");

                    b.HasOne("VoltProjects.Shared.Models.ProjectVersion", "ProjectVersion")
                        .WithMany()
                        .HasForeignKey("ProjectVersionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_page_project_version_project_version_id");

                    b.Navigation("ParentPage");

                    b.Navigation("ProjectToc");

                    b.Navigation("ProjectVersion");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectPageContributor", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.ProjectPage", "Page")
                        .WithMany()
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_page_contributor_project_page_page_id");

                    b.Navigation("Page");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectPageStorageItem", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.ProjectPage", "Page")
                        .WithMany()
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_page_storage_item_project_page_page_id");

                    b.HasOne("VoltProjects.Shared.Models.ProjectStorageItem", "StorageItem")
                        .WithMany()
                        .HasForeignKey("StorageItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_page_storage_item_project_storage_item_storage_item");

                    b.Navigation("Page");

                    b.Navigation("StorageItem");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectPreBuild", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.ProjectVersion", "ProjectVersion")
                        .WithMany()
                        .HasForeignKey("ProjectVersionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_pre_build_project_version_project_version_id");

                    b.Navigation("ProjectVersion");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectStorageItem", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.ProjectVersion", "ProjectVersion")
                        .WithMany()
                        .HasForeignKey("ProjectVersionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_storage_item_project_version_project_version_id");

                    b.Navigation("ProjectVersion");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectToc", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.ProjectVersion", "ProjectVersion")
                        .WithMany()
                        .HasForeignKey("ProjectVersionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_toc_project_version_project_version_id");

                    b.Navigation("ProjectVersion");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.ProjectVersion", b =>
                {
                    b.HasOne("VoltProjects.Shared.Models.DocBuilder", "DocBuilder")
                        .WithMany()
                        .HasForeignKey("DocBuilderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_version_doc_builder_doc_builder_id");

                    b.HasOne("VoltProjects.Shared.Models.Language", "Language")
                        .WithMany()
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_version_language_language_id");

                    b.HasOne("VoltProjects.Shared.Models.Project", "Project")
                        .WithMany("ProjectVersions")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_project_version_project_project_id");

                    b.Navigation("DocBuilder");

                    b.Navigation("Language");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("VoltProjects.Shared.Models.Project", b =>
                {
                    b.Navigation("ProjectVersions");
                });
#pragma warning restore 612, 618
        }
    }
}
