# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

# Install Node & Yarn
ENV NODE_MAJOR=18
RUN apt-get -y update \
    && apt-get install -y curl \
    && curl -sL https://deb.nodesource.com/setup_$NODE_MAJOR.x | bash - \ 
    && apt-get install -y nodejs \
    && apt-get clean \
    && npm install --global yarn

# Copy what server needs
COPY version.json version.json
COPY VoltProjects.Shared/ VoltProjects.Shared/
COPY VoltProjects.Client/ VoltProjects.Client/
COPY VoltProjects.Server/ VoltProjects.Server/

# Build client project
WORKDIR "/src/VoltProjects.Client"
RUN yarn
RUN yarn run build

# Restore and build the server project
WORKDIR "/src/VoltProjects.Server"
RUN dotnet restore "VoltProjects.Server.csproj"
RUN dotnet publish "VoltProjects.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false  --no-restore

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VoltProjects.Server.dll"]
