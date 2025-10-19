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

### Role-Based Authorization

The application supports three roles for access control:

- **SystemAdmin**: Full system access including user management and system configuration
- **Admin**: Server management and configuration access (can start/stop servers)
- **User**: Basic access to view servers and logs (read-only)

#### Configuring Roles in Azure AD

To assign roles to users:

1. In the Azure Portal, navigate to your App Registration
2. Go to **App roles** > **Create app role**
3. Create the following roles:

**User Role:**
- Display name: `User`
- Allowed member types: `Users/Groups`
- Value: `User`
- Description: `Basic user with read-only access to servers and logs`

**Admin Role:**
- Display name: `Admin`
- Allowed member types: `Users/Groups`
- Value: `Admin`
- Description: `Administrator with server management capabilities`

**SystemAdmin Role:**
- Display name: `SystemAdmin`
- Allowed member types: `Users/Groups`
- Value: `SystemAdmin`
- Description: `System administrator with full access`

4. Assign roles to users:
   - Navigate to **Enterprise applications**
   - Find and select your application
   - Go to **Users and groups** > **Add user/group**
   - Select users and assign them to the appropriate roles

#### Authorization Policies

The application uses three authorization policies:

- `RequireUser`: Requires User, Admin, or SystemAdmin role
- `RequireAdmin`: Requires Admin or SystemAdmin role
- `RequireSystemAdmin`: Requires SystemAdmin role only

Pages are protected with these policies using the `[Authorize(Policy = "...")]` attribute.

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
