# Volt Projects

[![License](https://img.shields.io/github/license/voltstro/VoltProjects.svg)](/LICENSE)
[![Build](https://img.shields.io/azure-devops/build/Voltstro/1cec7788-61ae-4aca-9579-dec5233a934e/6?logo=azure-pipelines)](https://dev.azure.com/Voltstro/VoltProjects/_build?definitionId=6)
[![Docs Status](https://img.shields.io/uptimerobot/status/m794227043-7e2bf837661fcd75d2af6804?label=Docs)](https://projects.voltstro.dev/)
[![Discord](https://img.shields.io/badge/Discord-Voltstro-7289da.svg?logo=discord)](https://discord.voltstro.dev) 
[![YouTube](https://img.shields.io/badge/Youtube-Voltstro-red.svg?logo=youtube)](https://www.youtube.com/Voltstro)

Volt Projects - An automatic documentation building and hosting service.

## What does it do?

Volt Projects main purpose is to host a project's documentation. It handles the process of automatically updating and deploying a project's documentation. Volt Projects it self does not generate any static files, instead it can ingest documentation from any existing static doc builder.

## Getting Started

### Database Setup

For either hosting or development of Volt Projects, you will need a Postgres 17 database. You can really use whatever service you want to run Postgres, ([local install](https://www.postgresql.org/download/), [Docker](https://hub.docker.com/_/postgres/), etc), as long as VP can connect to it using a [connection string](https://www.npgsql.org/doc/connection-string-parameters.html).

You will obviously need the database for VP and a user to access the DB.

```sql
CREATE USER voltprojects WITH INHERIT LOGIN ENCRYPTED PASSWORD 'Testing123';

CREATE DATABASE voltprojects WITH
	OWNER = 'voltprojects';

-- Execute on new DB
GRANT USAGE ON SCHEMA public TO voltprojects;
```

> [!NOTE]
> Do not use this password in a production environment! If your dumb enough to not realize this, then you should definitely not be hosting a production instance.

### Object Storage

Volt Project uses "object storage" for storage of project's external assets, such as images and scripts. Supported object storage providers are [Azure Storage](https://learn.microsoft.com/en-au/azure/storage/common/storage-account-overview), [AWS S3](https://docs.aws.amazon.com/AmazonS3/latest/userguide/Welcome.html) (or other S3 implementations such as [Cloudflare's R2](https://developers.cloudflare.com/r2/)) and [Google Cloud Storage](https://cloud.google.com/storage/docs/).

You will need to use one of these providers (or an emulator such [Azurite](https://learn.microsoft.com/en-au/azure/storage/common/storage-use-azurite)). You will also need to configure the container to host to blobs publicly.

Configure the object storage provider using `Config.ObjectStorageProvider` in the appsettings for both Server and Builder. The credentials for storage providers are always provided by environment variables.

### Hosting

We provide VoltProject as Docker images, one for Server and one for Builder.

- [VPServer](https://hub.docker.com/r/voltstro/vpserver)
- [VPBuilder](https://hub.docker.com/r/voltstro/vpbuilder)

You can run these apps together with Docker Compose, using a `docker-compose.yml` file like so:

```yml
version: "3.9"

services:
  server:
    image: voltstro/vpserver:latest
    container_name: vpserver
    restart: unless-stopped
    ports:
      - "8080:80"
    environment:
      - ConnectionStrings__ServerConnection
      - Config__ObjectStorageProvider__Provider
      - Config__ObjectStorageProvider__ContainerName
      - Config__ObjectStorageProvider__BasePath
      - Config__ObjectStorageProvider__SubPath
      - Config__PublicUrl
      - Config__OpenIdConfig__Authority
      - Config__OpenIdConfig__ClientId
      - Config__OpenIdConfig__ClientSecret
      - VP_AZURE_CREDENTIAL

  builder:
    image: voltstro/vpbuilder:latest
    container_name: vpbuilder
    restart: unless-stopped
    environment:
      - ConnectionStrings__BuilderConnection
      - Config__ObjectStorageProvider__Provider
      - Config__ObjectStorageProvider__ContainerName
      - Config__ObjectStorageProvider__BasePath
      - Config__ObjectStorageProvider__SubPath
      - VP_AZURE_CREDENTIAL
```

### Build

#### Prerequisites

```
.NET 9 SDK
NodeJs 22
Postgres 17
```

### Setup

1. Build the client project by using these commands in the `src/VoltProjects.Client` directory.

    ```
    pnpm install
    pnpm run build
    ```

2. Update `appsettings.json` in both Builder and Server accordingly for what you need.

3. Build the .NET projects using your IDE of choice, or `dotnet build` in the `src/` directory.

4. Run either the Server or Builder project for it to migrate the database. You can close it once the DB has been migrated.

5. Run Builder and Server projects. Make sure everything in builder goes well. You can access the server on `http://localhost:5000`.

#### Docker-Compose

An all-in-one Docker compose file is provided in `src/docker-compose.yml`. This will setup Builder, Server, Postgres and Azurite for you.

## Security

Please see [SECURITY.md](/SECURITY.md) for details about security.

## Authors

**Voltstro** - *Initial work* - [Voltstro](https://github.com/Voltstro)

## License

This project is licensed under the GPL-3.0 license - see the [LICENSE](/LICENSE) file for details.

The content that VoltProjects hosts may be under a different license. See that project's git repo for information on it's license.
