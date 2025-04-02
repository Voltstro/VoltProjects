# Builders

Volt Projects current supports two doc builders.

- [DocFx](https://dotnet.github.io/docfx/)
- [MkDocs](https://www.mkdocs.org/)

Volt Project's architecture allows other doc builders to easily be integrated.

## DocFx

Volt Projects has no issues with DocFx. DocFx's "ugly" urls will be "prettified" (E.G: `articles/builders.html` -> `articles/builders/`.).

A big feature of DocFx is it ability to generate API docs from source code. For the API docs to be generated, `docfx metadata` must be ran, you can add this command as a pre-build command in the project config.

## MkDocs

Docs generated using MkDocs can be ingested by Volt Projects. There are couple of notes to keep in mind.

The MkDocs config file must be called `mkdocs.yml`. Its recommended to stick with the default MkDocs theme.

Volt Projects uses a custom plugin called 'vp_integration' to generate pages and other project details as json files. If your config doesn't have this plugin defined, it will automatically add it before running mkdocs.
