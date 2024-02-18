# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

# Copy what builder needs
COPY version.json version.json
COPY VoltProjects.Shared/ VoltProjects.Shared/
COPY VoltProjects.Builder/ VoltProjects.Builder/

# Restore and build the builder project
WORKDIR "/src/VoltProjects.Builder"
RUN dotnet restore "VoltProjects.Builder.csproj"
RUN dotnet publish "VoltProjects.Builder.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Final Stage
# We using SDK to run the app, as VDocFx needs it (plus projects may need it for thie pre-build commands)
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .

# Install VDocFx
RUN dotnet tool update -g vdocfx --version "1.0.0-0010-gaa68456a75" --add-source https://pkgs.dev.azure.com/Voltstro/VDocFx/_packaging/VDocFx/nuget/v3/index.json 
RUN dotnet tool update -g docfx --version "2.75.2"

ENV DOTNET_ENVIRONMENT=Production
ENV PATH="/root/.dotnet/tools/:$PATH"
ENTRYPOINT ["dotnet", "VoltProjects.Builder.dll"]
