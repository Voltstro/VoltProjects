# VoltProjects

The project responsible for hosting [projects.voltstro.dev](https://projects.voltstro.dev).

## What does it do?

VoltProjects will clone repos and build their docs, then serve the static built content, automatically.

It will periodically pull the latest tag of defined repositories.

## Getting Started

### Prerequisites

```
Yarn
.NET 6 SDK
```

### Building

To build VoltProjects, simply use your IDE, or `dotnet build`.

### VoltProjects Config

VoltProjects is configured using a `appsettings.json` file.

#### Sentry

VoltProjects uses Sentry, if you want to configure it's Dsn, either use the environmental variable `SENTRY_DSN` or by using [.NET's user secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=linux#set-a-secret).

### Docs Configuration

Each docs project needs a `VoltDocsBuilder.json` file. See [VoltRpc for an example](https://github.com/Voltstro-Studios/VoltRpc/blob/master/docs/VoltDocsBuilder.json).

#### Doc Builders

Doc builders are what are responsible for building the docs.

- DocFx v3


## Authors

**Voltstro** - *Initial work* - [Voltstro](https://github.com/Voltstro)

## License

This project is licensed under the AGPL-3.0 license - see the [LICENSE](/LICENSE) file for details.

The content that VoltProjects may be served under a different license.