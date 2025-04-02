# Getting Started

This guide should hopefully get the basics across for hosting Volt Projects.

## Prerequisites

- Postgres 17
- An [Object Storage Provider](object-storage.md)
- An OpenId provider (E.G: [Microsoft Entra Id](https://learn.microsoft.com/en-au/entra/fundamentals/whatis), [Google Identity](https://developers.google.com/identity/openid-connect/openid-connect), [GitLab](https://docs.gitlab.com/integration/openid_connect_provider/))
  - It doesn't have to be a cloud solution, the only thing that matters is that it adheres to the OpenId spec
- Docker (or something that can run Docker containers)

## Database Setup

Volt Projects requires a Postgres database. The Volt Project apps will handle migrating the database, but as such, will require the user who connects to the database to have permissions to create objects in the database.

The connection string used should end up looking like this:

```
Host=<Database Host>;Username=voltprojects;Password=<Password>;Database=voltprojects
```

You can find more details on options for the connection string in [Npgsql docs](https://www.npgsql.org/doc/connection-string-parameters.html).

## Apps

You can find the two apps Docker images hosted on Docker Hub.

- [VPServer](https://hub.docker.com/repository/docker/voltstro/vpserver/general)
- [VPBuilder](https://hub.docker.com/repository/docker/voltstro/vpbuilder/general)

You can use Docker compose to run the two images together. There is an [example Docker compose config](https://github.com/Voltstro/VoltProjects/blob/master/src/docker-compose.yml) in the repo.

## Config

Configuration is done by either setting the options in `appsettings.json`, or by setting the options using environment variables.

Setting options by the environment options is done in a format of `<Key>__<Key>`.

### Base Settings

The main base options you will need to set are:

1. The connection string (`ConnectionStrings.(Server/Builder)Connection`)
2. Object storage (`Config.ObjectStorageProvider`)
3. OpenId config (`Config.OpenIdConfig`)

Example settings for server:

```ini
ConnectionStrings__ServerConnection="Host=<Database Host>;Username=voltprojects;Password=<Password>;Database=voltprojects"
Config__PublicUrl="https://<Server URL>"
Config__OpenIdConfig__ClientId=""
Config__OpenIdConfig__ClientSecret=""
Config__OpenIdConfig__Authority=""
Config__ObjectStorageProvider__Provider="Azure"
Config__ObjectStorageProvider__ContainerName="voltprojects"
Config__ObjectStorageProvider__BasePath="https://<Container URL>"
Config__ObjectStorageProvider__SubPath="voltprojects/"
```

Example settings for builder:

```ini
ConnectionStrings__BuilderConnection="Host=<Database Host>;Username=voltprojects;Password=<Password>;Database=voltprojects"
Config__ObjectStorageProvider__Provider="Azure"
Config__ObjectStorageProvider__ContainerName="voltprojects"
Config__ObjectStorageProvider__BasePath="https://<Container URL>"
Config__ObjectStorageProvider__SubPath="voltprojects/"
```
