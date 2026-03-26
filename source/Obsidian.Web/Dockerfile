# === Build Stage ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Obsidian.sln", "."]
COPY ["source/Obsidian.Web/Obsidian.Web.csproj", "source/Obsidian.Web/"]
COPY ["source/Obsidian.Models/Obsidian.Models.csproj", "source/Obsidian.Models/"]
COPY ["source/Obsidian.ServiceDefaults/Obsidian.ServiceDefaults.csproj", "source/Obsidian.ServiceDefaults/"]
RUN dotnet restore "source/Obsidian.Web/Obsidian.Web.csproj"

COPY . .
RUN dotnet publish "source/Obsidian.Web/Obsidian.Web.csproj" \
    -c Release -o /app/publish --no-restore

# === Runtime Stage ===
FROM nginx:alpine AS final

# Install envsubst (part of gettext)
RUN apk add --no-cache gettext

# Copy published WASM output
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Create appsettings template for runtime substitution
RUN cp /usr/share/nginx/html/appsettings.json \
       /usr/share/nginx/html/appsettings.json.template

# nginx config and entrypoint
COPY source/Obsidian.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY source/Obsidian.Web/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/entrypoint.sh"]
