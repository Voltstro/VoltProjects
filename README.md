# VoltProjects

VoltProjects - An online documentation building and hosting service. Built with ASP.NET Core.

## What does it do?

VoltProjects hosts project's online documentation, by periodically checking defined project's git repos, and building their documentation using their required tools, then serving the freshly built static content.



## Getting Started

### Prerequisites

```
.NET 6 SDK
Yarn
```

### Building

To build VoltProjects, simply use your IDE, or `dotnet build`. Please not that Yarn will automatically be run for you.

#### Publishing

For publishing, run the `src/DevScripts/publish-linux-x64.ps1` file with PowerShell. This will build a published build of VoltProject for linux-x64. You can then run this build however you feel like, whether that is directly facing the internet, behind a reverse proxy, or for local use only.

### VoltProjects Config

VoltProjects is configured using the `appsettings.json` file. The main configuration is done under the "`VoltProjects`" key. Is is fairly easy to understand what each option does.

### Docs Configuration

Each docs project needs a `VoltDocsBuilder.json` file. See [VoltRpc for an example](https://github.com/Voltstro-Studios/VoltRpc/blob/master/docs/VoltDocsBuilder.json).

#### Doc Builders

Doc builders are what are responsible for building the docs.

Currently, we only have one, more should hopefully be added.

- DocFx v3

## Limitations

VoltProjects is very simple, it only hosts content built by a static documentation generator. If versioned docs or multi-lang docs are required, you will need a generator that can do that (Such as [Docusaurus](https://docusaurus.io/)). Maybe one day VoltProjects could be responsible for doing this, but not right now.

The main pages (Home & About) also don't support any other languages, english is hard baked into them. While I do belive adding multi-language support would be easy, the only reason why I haven't is I don't speak any other languages, I am your *usual western english only speaking person*.

## Security

Please see [SECURITY.md](/SECURITY.md) for details about security.

## Authors

**Voltstro** - *Initial work* - [Voltstro](https://github.com/Voltstro)

## License

This project is licensed under the AGPL-3.0 license - see the [LICENSE](/LICENSE) file for details.

The content that VoltProjects may be served under a different license.