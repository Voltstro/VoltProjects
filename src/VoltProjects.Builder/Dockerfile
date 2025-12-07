# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Restore Projects
COPY VoltProjects.Shared/VoltProjects.Shared.csproj VoltProjects.Shared/
COPY VoltProjects.Builder/VoltProjects.Builder.csproj VoltProjects.Builder/
RUN dotnet restore "VoltProjects.Builder/VoltProjects.Builder.csproj"

# Copy everything else
COPY version.json version.json
COPY VoltProjects.Shared/ VoltProjects.Shared/
COPY VoltProjects.Builder/ VoltProjects.Builder/
COPY VoltProjects.Builder.HlJs/ VoltProjects.Builder.HlJs/

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
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS final
WORKDIR /home/app/VoltProjects.Builder/

# Install DocFx
RUN dotnet tool update docfx --version "2.78.3" --tool-path /opt/docfx/

# Install MKDocs
COPY VoltProjects.Integration.MkDocs/ /home/app/src/VoltProjects.Integration.MkDocs/
RUN apk add --no-cache python3 py3-pip && \
    python -m pip install mkdocs /home/app/src/VoltProjects.Integration.MkDocs/ --break-system-packages

# Copy VoltProjects.Builder app
COPY --from=build /app/publish/ /home/app/VoltProjects.Builder/

# Builder needs perms to its own folder
RUN chown $APP_UID -R /home/app/VoltProjects.Builder/

ENV DOTNET_ENVIRONMENT=Production
ENV PATH="/opt/docfx/:$PATH"
ENTRYPOINT ["dotnet", "VoltProjects.Builder.dll"]
USER app
