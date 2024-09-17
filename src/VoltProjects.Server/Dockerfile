# Build Client Stage
FROM node:18-alpine AS build-client
WORKDIR /src

# Install Packages
COPY VoltProjects.Client/yarn.lock ./
COPY VoltProjects.Client/package.json ./
RUN yarn install

# Copy everything else
COPY VoltProjects.Client/src/ ./src/
COPY VoltProjects.Client/public/ ./public/
COPY VoltProjects.Client/tsconfig.json ./
COPY VoltProjects.Client/vite.config.ts ./

# Build Client
RUN yarn run build:dist

# Build Server Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-app
WORKDIR /src

# Restore Projects
COPY VoltProjects.Shared/VoltProjects.Shared.csproj VoltProjects.Shared/
COPY VoltProjects.Server/VoltProjects.Server.csproj VoltProjects.Server/
RUN dotnet restore "VoltProjects.Server/VoltProjects.Server.csproj"

# Copy everything else
COPY version.json version.json
COPY VoltProjects.Shared/ VoltProjects.Shared/
COPY VoltProjects.Server/ VoltProjects.Server/
COPY --from=build-client /src/dist /src/VoltProjects.Server/wwwroot/

# Build App
RUN dotnet publish "VoltProjects.Server/VoltProjects.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 80
COPY --from=build-app /app/publish .
ENTRYPOINT ["dotnet", "VoltProjects.Server.dll"]
