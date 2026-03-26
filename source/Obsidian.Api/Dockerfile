# === Build Stage ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for restore (layer caching)
COPY ["Obsidian.sln", "."]
COPY ["source/Obsidian.Api/Obsidian.Api.csproj", "source/Obsidian.Api/"]
COPY ["source/Obsidian/Obsidian.csproj", "source/Obsidian/"]
COPY ["source/Obsidian.Models/Obsidian.Models.csproj", "source/Obsidian.Models/"]
COPY ["source/Obsidian.DataAccess/Obsidian.DataAccess.csproj", "source/Obsidian.DataAccess/"]
COPY ["source/Obsidian.ServiceDefaults/Obsidian.ServiceDefaults.csproj", "source/Obsidian.ServiceDefaults/"]
RUN dotnet restore "source/Obsidian.Api/Obsidian.Api.csproj"

# Copy all source and publish
COPY . .
RUN dotnet publish "source/Obsidian.Api/Obsidian.Api.csproj" \
    -c Release -o /app/publish --no-restore

# === Runtime Stage ===
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser

COPY --from=build /app/publish .

# SQLite data volume mount point
RUN mkdir -p /data && chown appuser:appuser /data
VOLUME ["/data"]

USER appuser
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Obsidian.Api.dll"]
