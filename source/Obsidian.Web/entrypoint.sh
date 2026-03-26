#!/bin/sh
set -e

# Default API URL if not provided
API_URL="${API_URL:-http://api.obsidian.local}"

# Restore the original appsettings.json from the baked-in template so that
# repeated container restarts stay idempotent (sed would otherwise re-apply
# substitutions on already-substituted values).
cp /usr/share/nginx/html/appsettings.json.template \
   /usr/share/nginx/html/appsettings.json

# Substitute environment variables into appsettings.json
sed -i "s|\"ApiBaseUrl\": \"[^\"]*\"|\"ApiBaseUrl\": \"${API_URL}/\"|g" \
    /usr/share/nginx/html/appsettings.json

# Also update the services section for Aspire service discovery fallback
sed -i "s|https://localhost:5001|${API_URL}|g" \
    /usr/share/nginx/html/appsettings.json
sed -i "s|http://localhost:5000|${API_URL}|g" \
    /usr/share/nginx/html/appsettings.json

exec nginx -g 'daemon off;'
