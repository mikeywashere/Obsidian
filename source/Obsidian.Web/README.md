# Obsidian Web UI

A Blazor WebAssembly application for managing and monitoring Minecraft Bedrock servers.

## Features

- **Microsoft Authentication**: Secure login using Microsoft accounts
- **Server Management**: View, start, and stop Minecraft servers
- **Real-time Monitoring**: Monitor server status and player counts
- **Log Viewer**: View server logs in real-time
- **Flat UI Design**: Clean, modern interface

## Setup

### Running the Application

```bash
cd source/Obsidian.Web
dotnet run
```

Navigate to `https://localhost:7223` or `http://localhost:5276`

### Configuring Microsoft Authentication

To enable Microsoft Authentication, you need to register an Azure AD application:

1. Go to the [Azure Portal](https://portal.azure.com/)
2. Navigate to **Azure Active Directory** > **App registrations** > **New registration**
3. Enter a name for your application (e.g., "Obsidian Web UI")
4. Set the redirect URI to: `https://localhost:7223/authentication/login-callback` (or your deployment URL)
5. Click **Register**
6. Copy the **Application (client) ID**
7. Update `wwwroot/appsettings.json` with your Client ID:

```json
{
  "AzureAd": {
    "Authority": "https://login.microsoftonline.com/common",
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "ValidateAuthority": true
  }
}
```

### Authentication Configuration

The application uses MSAL (Microsoft Authentication Library) for authentication. When authentication is not configured, the app will still work but won't enforce login requirements.

## Project Structure

```
Obsidian.Web/
├── Models/           # Data models (ServerInfo, ServerLog)
├── Services/         # Services (MockServerService)
├── Pages/           # Blazor pages (Home, Servers, ServerDetail, Authentication)
├── Layout/          # Layout components (MainLayout, NavMenu)
├── Shared/          # Shared components (LoginDisplay, RedirectToLogin)
└── wwwroot/         # Static assets and configuration
```

## Development

The application currently uses a mock service (`MockServerService`) for demonstration purposes. In production, this should be replaced with a real service that communicates with the Minecraft server management API.

## Technologies

- Blazor WebAssembly (.NET 8.0)
- Microsoft Authentication Library (MSAL)
- Bootstrap 5 for styling
- CSS isolation for component-specific styles
