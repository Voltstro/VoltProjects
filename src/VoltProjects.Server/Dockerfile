# Build Client Stage
FROM node:22-alpine AS build-client
WORKDIR /src

# Install Packages
COPY VoltProjects.Client/pnpm-lock.yaml VoltProjects.Client/package.json ./
RUN corepack enable
RUN corepack install
RUN pnpm install

# Copy everything else
COPY VoltProjects.Client/build/ ./build/
COPY VoltProjects.Client/src/ ./src/
COPY VoltProjects.Client/public/ ./public/
COPY VoltProjects.Client/tsconfig.json ./
COPY VoltProjects.Client/vite.config.ts ./

# Build Client
RUN pnpm run icons
RUN pnpm run build:dist

# Build Server Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build-app
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
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final

# Setup ICU
ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

RUN apk add --no-cache icu-data-full icu-libs

WORKDIR /app
EXPOSE 80
COPY --from=build-app /app/publish .
ENTRYPOINT ["dotnet", "VoltProjects.Server.dll"]
