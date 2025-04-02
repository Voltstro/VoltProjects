# Development

Want to work on Volt Projects? Here are some dev instructions to get you going.

## Prerequisites

- .NET 9 SDK
- NodeJs 22
- Postgres 17

You will also still need an OpenId provider and an object storage provider. There are "emulators" available for object storage that can be used to host object storage locally (E.G. [Azurite (Azure)](https://learn.microsoft.com/en-au/azure/storage/common/storage-use-azurite), [Minio (S3)](https://github.com/minio/minio)).

The builders themselves will also need to be installed. See their docs for how to install. If needed, the exact command executed by builder can be changed by editing the record in the `doc_builder` table (once Database created). 

## Database Setup

By default, development appsettings is configured to use a local database running on `localhost`, connecting to a database called `voltprojects`, using a user called `voltprojects` that has a password of `Testing123`.

## Setup

1. Build the client project by using these commands in the `src/VoltProjects.Client` directory.
    ```
    pnpm install
    pnpm run build
    ```

2. Update `appsettings.Development.json` in both Builder and Server accordingly for what you need. Credentials (E.G. OpenId credentials) should be set using environment variables.

3. Build the .NET projects using your IDE of choice, or `dotnet build` in the `src/` directory.

4. Builder requires `DOTNET_ENVIRONMENT` environment variable set to `Development` to use development settings. 

5. Run both Server and Builder projects. The Database should migrate automatically if all goes well.

By default, you can access the server on `http://localhost:5000`.
