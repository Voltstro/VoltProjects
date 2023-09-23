# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:7.0-bookworm-slim AS build
WORKDIR /src

# Install Node & Yarn
ENV NODE_MAJOR=18
RUN apt-get update  \
    && apt-get install curl ca-certificates gnupg -y \
    && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
    && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list \
    && apt-get update \
    && apt-get install nodejs -y \
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
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS final
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VoltProjects.Server.dll"]
