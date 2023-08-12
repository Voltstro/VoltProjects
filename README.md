# Volt Projects

[![License](https://img.shields.io/github/license/voltstro/VoltProjects.svg)](/LICENSE)
[![Build](https://img.shields.io/azure-devops/build/Voltstro/1cec7788-61ae-4aca-9579-dec5233a934e/6?logo=azure-pipelines)](https://dev.azure.com/Voltstro/VoltProjects/_build?definitionId=6)
[![Docs Status](https://img.shields.io/website?down_color=red&down_message=Offline&label=Docs&up_color=blue&up_message=Online&url=https%3A%2F%2Fprojects.voltstro.dev)](https://projects.voltstro.dev)
[![Discord](https://img.shields.io/badge/Discord-Voltstro-7289da.svg?logo=discord)](https://discord.voltstro.dev) 
[![YouTube](https://img.shields.io/badge/Youtube-Voltstro-red.svg?logo=youtube)](https://www.youtube.com/Voltstro)

Volt Projects - An automatic documentation building and hosting service.

## What does it do?

Volt Projects main purpose is to host a project's documentation. It handles the process of automatically updating and deploying a project's documentation.

**NOTE**: This is branch is for Volt Project v2. This version is still under development.

## Getting Started

### Database Setup

For either hosting or development, you will need a Postgres 15 database. See [Postgres's website](https://www.postgresql.org/download/) for installation instructions. If your hosting a production environment, then you should know how to set this up! We recommend sticking with the default 'postgres' admin user, but set a strong password!

We need some roles for Volt Projects to access the DB, and you need a DB it self of course, these SQL commands should provide a point direction for what you need.


```sql
CREATE USER vpbuilder WITH INHERIT LOGIN ENCRYPTED PASSWORD 'Testing123';
CREATE USER vpserver WITH INHERIT LOGIN ENCRYPTED PASSWORD 'Testing123';

CREATE DATABASE voltprojects WITH
	OWNER = 'postgres';

-- Execute on new DB
GRANT USAGE ON SCHEMA public TO vpbuilder;
GRANT USAGE ON SCHEMA public TO vpserver;
```

(**NOTE:** Do not use these password in a production environment! If your dumb enough to not realize this, then you should definitely not be hosting an instance.)

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
    yarn run build:prod
    ```

2. Build the .NET projects using your IDE of choice, or `dotnet build` in the `src/` directory.

3. Run Migrations on your Postgres database. To run migrations, execute the following command in `src/VoltProjects.Shared`.

    ```
    dotnet ef database update --connection "Host=localhost;Username=postgres;Password=<password>;Database=voltprojects"
    ```

4. Add Projects to build to the 'Project' and 'ProjectVersion' tables.

## Security

Please see [SECURITY.md](/SECURITY.md) for details about security.

## Authors

**Voltstro** - *Initial work* - [Voltstro](https://github.com/Voltstro)

## License

This project is licensed under the GPL-3.0 license - see the [LICENSE](/LICENSE) file for details.

The content that VoltProjects hosts may be under a different license. See that project's git repo for information on it's license.
