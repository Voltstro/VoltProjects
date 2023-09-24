# Volt Projects

[![License](https://img.shields.io/github/license/voltstro/VoltProjects.svg)](/LICENSE)
[![Build](https://img.shields.io/azure-devops/build/Voltstro/1cec7788-61ae-4aca-9579-dec5233a934e/6?logo=azure-pipelines)](https://dev.azure.com/Voltstro/VoltProjects/_build?definitionId=6)
[![Docs Status](https://img.shields.io/website?down_color=red&down_message=Offline&label=Docs&up_color=blue&up_message=Online&url=https%3A%2F%2Fprojects.voltstro.dev)](https://projects.voltstro.dev)
[![Discord](https://img.shields.io/badge/Discord-Voltstro-7289da.svg?logo=discord)](https://discord.voltstro.dev) 
[![YouTube](https://img.shields.io/badge/Youtube-Voltstro-red.svg?logo=youtube)](https://www.youtube.com/Voltstro)

Volt Projects - An automatic documentation building and hosting service.

## What does it do?

Volt Projects main purpose is to host a project's documentation. It handles the process of automatically updating and deploying a project's documentation.

> [!NOTE]
> This branch is Volt Projects v2. This version is still under development!

## Getting Started

### Database Setup

For either hosting or development of Volt Projects, you will need a Postgres 15 database. You can really use whatever service you want to run Postgres ([local install](https://www.postgresql.org/download/), [Docker](https://hub.docker.com/_/postgres/), etc), as long as VP can connect to it using a [connection string](https://www.npgsql.org/doc/connection-string-parameters.html).

You will obviously need the database for VP. VP also needs two roles, one called `vpbuilder`, and the other called `vpserver`. The SQL commands below should provide you with enough information on how to set this up.


```sql
CREATE USER vpbuilder WITH INHERIT LOGIN ENCRYPTED PASSWORD 'Testing123';
CREATE USER vpserver WITH INHERIT LOGIN ENCRYPTED PASSWORD 'Testing123';

CREATE DATABASE voltprojects WITH
	OWNER = 'postgres';

-- Execute on new DB
GRANT USAGE ON SCHEMA public TO vpbuilder;
GRANT USAGE ON SCHEMA public TO vpserver;
```

> [!NOTE]
> Do not use these passwords in a production environment! If your dumb enough to not realize this, then you should definitely not be hosting a production instance.

### Azure Blob Storage

VoltProjects.Builder makes use of Azure Blob Storage for uploading image assets. You will need an Azure Storage Account, and create a container for Volt Projects to use. The container needs to be setup for public access. When running VoltProjects.Builder, you need to set `VP_AZURE_CREDENTIAL` environment variable to one of your storage account's connection strings. The `PublicUrl` in both server and builder's config then needs to be set to your container's public url (usually `https://<Account Name>.blob.core.windows.net/<Container Name>/`).

### Hosting

TODO

### Build

#### Prerequisites

```
.NET 7 SDK
Yarn
Postgres 15 DB
```

### Setup

1. Build the client project by using these commands in the `src/VoltProjects.Client` directory.

    ```
    yarn
    yarn run build
    ```

2. Build the .NET projects using your IDE of choice, or `dotnet build` in the `src/` directory.

3. Run Migrations on your Postgres database. To run migrations, execute the following command in `src/VoltProjects.Shared`.

    ```
    dotnet ef database update --connection "Host=localhost;Username=postgres;Password=<password>;Database=voltprojects"
    ```

4. Add Projects to build to the `project` and `project_version` tables, and the project's accompanying pre build command to `project_pre_build`. An example for VoltRpc is provided below.

    ```sql
    INSERT INTO public.project
    (id, "name", short_name, description, git_url, icon_path)
    VALUES(1, 'VoltRpc', NULL, 'VoltRpc', 'https://github.com/Voltstro-Studios/VoltRpc', 'icon.svg');

    INSERT INTO public.project_version
    (id, project_id, version_tag, git_branch, git_tag, doc_builder_id, docs_path, docs_built_path, language_id, is_default)
    VALUES(1, 1, 'latest', 'chore/vdocfx', NULL, 'vdocfx', 'docs/', 'docs/_site/VoltRpc/', 1, true);

    INSERT INTO public.project_pre_build
    (id, project_version_id, "order", command, arguments)
    VALUES(1, 1, 1, 'dotnet', 'build src/VoltRpc.sln -c ReleaseNoPackage');
    ```

5. Update `appsettings.json` in both Builder and Server accordingly for what you need.

6. Run Builder and Server projects. Make sure everything in builder goes well. You can access the server on `http://localhost:5000`.

## Security

Please see [SECURITY.md](/SECURITY.md) for details about security.

## Authors

**Voltstro** - *Initial work* - [Voltstro](https://github.com/Voltstro)

## License

This project is licensed under the GPL-3.0 license - see the [LICENSE](/LICENSE) file for details.

The content that VoltProjects hosts may be under a different license. See that project's git repo for information on it's license.
