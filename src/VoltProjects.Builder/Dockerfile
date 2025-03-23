# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Restore Projects
COPY VoltProjects.Shared/VoltProjects.Shared.csproj VoltProjects.Shared/
COPY VoltProjects.Builder/VoltProjects.Builder.csproj VoltProjects.Builder/
RUN dotnet restore "VoltProjects.Builder/VoltProjects.Builder.csproj"

# Copy everything else
COPY version.json version.json
COPY VoltProjects.Shared/ VoltProjects.Shared/
COPY VoltProjects.Builder/ VoltProjects.Builder/

# Build App
RUN dotnet publish "VoltProjects.Builder/VoltProjects.Builder.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# MkDocs uses python, so setup that and the modules
FROM python:3.12-alpine AS python
WORKDIR /src

COPY VoltProjects.Integration.MkDocs/ VoltProjects.Integration.MkDocs/
RUN python -m venv VoltProjects.Integration.MkDocs/venv
RUN VoltProjects.Integration.MkDocs/venv/bin/python -m pip install mkdocs VoltProjects.Integration.MkDocs/

# Final Stage
# We using SDK to run the app, as .NET doc projects need it
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS final
WORKDIR /app
RUN apk add --no-cache python3 py3-pip

# Install VDocFx
RUN dotnet tool update -g vdocfx --version "1.0.0-0010-gaa68456a75" --add-source https://pkgs.dev.azure.com/Voltstro/VDocFx/_packaging/VDocFx/nuget/v3/index.json

# Install DocFx
RUN dotnet tool update -g docfx --version "2.78.3"

# Install MKDocs
COPY VoltProjects.Integration.MkDocs/ /src/VoltProjects.Integration.MkDocs/
RUN python -m pip install mkdocs /src/VoltProjects.Integration.MkDocs/ --break-system-packages

# Copy VoltProjects.Builder app
COPY --from=build /app/publish .

ENV DOTNET_ENVIRONMENT=Production
ENV PATH="/root/.dotnet/tools/:$PATH"
ENTRYPOINT ["dotnet", "VoltProjects.Builder.dll"]
