#!/bin/sh
set -e

# Default API URL if not provided
API_URL="${API_URL:-http://api.obsidian.local}"

# Substitute environment variables into appsettings.json
# The template contains the original appsettings.json from dotnet publish
# We replace the ApiBaseUrl value with the runtime API_URL
sed -i "s|\"ApiBaseUrl\": \"[^\"]*\"|\"ApiBaseUrl\": \"${API_URL}/\"|g" \
    /usr/share/nginx/html/appsettings.json

# Also update the services section for Aspire service discovery fallback
sed -i "s|https://localhost:5001|${API_URL}|g" \
    /usr/share/nginx/html/appsettings.json
sed -i "s|http://localhost:5000|${API_URL}|g" \
    /usr/share/nginx/html/appsettings.json

exec nginx -g 'daemon off;'
