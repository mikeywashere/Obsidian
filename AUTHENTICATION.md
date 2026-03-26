# Authentication and Authorization Guide

This document describes the authentication and role-based authorization system implemented in Obsidian.

## Overview

Obsidian uses Microsoft Authentication Library (MSAL) for authentication with Azure Active Directory (Azure AD), combined with role-based authorization to control access to different features of the application.

## Roles

The application supports three distinct roles:

### 1. User
- **Description**: Basic user with read-only access
- **Permissions**:
  - View server list and status
  - View server details
  - View server logs
  - Cannot start or stop servers

### 2. Admin
- **Description**: Administrator with server management capabilities
- **Permissions**:
  - All User permissions
  - Start and stop servers
  - View server configurations
  - Cannot manage users or system settings

### 3. SystemAdmin
- **Description**: System administrator with full access
- **Permissions**:
  - All Admin permissions
  - Manage users (future feature)
  - Configure system settings (future feature)
  - Full access to all features

## Authorization Policies

Three authorization policies are defined in the application:

1. **RequireUser**: Allows access to users with User, Admin, or SystemAdmin roles
2. **RequireAdmin**: Allows access to users with Admin or SystemAdmin roles
3. **RequireSystemAdmin**: Allows access only to users with SystemAdmin role

## Implementation Details

### Files Added

1. **source/Obsidian.Web/Authorization/Roles.cs**
   - Defines role constants (User, Admin, SystemAdmin)
   - Provides AllRoles array for enumeration

2. **source/Obsidian.Web/Authorization/Policies.cs**
   - Defines policy names as constants
   - Used throughout the application for consistent policy references

3. **source/Obsidian.UnitTests/AuthorizationTests.cs**
   - 12 unit tests covering all authorization scenarios
   - Tests role constants, policies, and authorization behavior

### Files Modified

1. **source/Obsidian.Web/Program.cs**
   - Added authorization policy configuration
   - Configured three policies with appropriate role requirements

2. **source/Obsidian.Web/Pages/Servers.razor**
   - Added `[Authorize(Policy = Policies.RequireUser)]` attribute
   - Wrapped server control buttons in `<AuthorizeView Policy="@Policies.RequireAdmin">`
   - Shows informational message to non-admin users

3. **source/Obsidian.Web/Pages/ServerDetail.razor**
   - Added `[Authorize(Policy = Policies.RequireUser)]` attribute
   - Wrapped server control buttons in `<AuthorizeView Policy="@Policies.RequireAdmin">`
   - Admins can start/stop servers, regular users can only view

4. **source/Obsidian.Web/Shared/LoginDisplay.razor**
   - Added role badge display showing user's highest role
   - Visual indicators: SystemAdmin (purple), Admin (blue), User (green)

5. **source/Obsidian.Web/README.md**
   - Added comprehensive documentation on role-based authorization
   - Included step-by-step Azure AD configuration instructions
   - Documented how to create and assign roles

## Azure AD Configuration

### Step 1: Create App Roles in Azure AD

1. Navigate to [Azure Portal](https://portal.azure.com/)
2. Go to **Azure Active Directory** > **App registrations**
3. Select your application
4. Navigate to **App roles** > **Create app role**
5. Create three roles:

**User Role:**
```
Display name: User
Allowed member types: Users/Groups
Value: User
Description: Basic user with read-only access to servers and logs
```

**Admin Role:**
```
Display name: Admin
Allowed member types: Users/Groups
Value: Admin
Description: Administrator with server management capabilities
```

**SystemAdmin Role:**
```
Display name: SystemAdmin
Allowed member types: Users/Groups
Value: SystemAdmin
Description: System administrator with full access
```

### Step 2: Assign Roles to Users

1. Navigate to **Enterprise applications**
2. Find and select your application
3. Go to **Users and groups** > **Add user/group**
4. Select users and assign them to the appropriate roles

### Step 3: Configure Application Settings

Update `wwwroot/appsettings.json` with your Azure AD configuration:

```json
{
  "AzureAd": {
    "Authority": "https://login.microsoftonline.com/common",
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "ValidateAuthority": true
  }
}
```

## Testing

The authorization system includes comprehensive unit tests:

```bash
# Run all authorization tests
dotnet test --filter "FullyQualifiedName~AuthorizationTests"
```

### Test Coverage

- ✅ Role constant validation
- ✅ Policy constant validation
- ✅ SystemAdmin policy allows SystemAdmin role
- ✅ SystemAdmin policy denies Admin role
- ✅ Admin policy allows Admin role
- ✅ Admin policy allows SystemAdmin role
- ✅ Admin policy denies User role
- ✅ User policy allows User role
- ✅ User policy allows Admin role
- ✅ User policy allows SystemAdmin role
- ✅ User policy denies unauthenticated users

All 12 tests pass successfully.

## Usage Examples

### Protecting a Page

```csharp
@page "/admin-only"
@using Microsoft.AspNetCore.Authorization
@using Obsidian.Web.Authorization
@attribute [Authorize(Policy = Policies.RequireAdmin)]

<h3>Admin Only Page</h3>
<p>Only admins and system admins can see this.</p>
```

### Conditional UI Based on Role

```razor
<AuthorizeView Policy="@Policies.RequireAdmin">
    <Authorized>
        <button class="btn btn-danger" @onclick="DeleteServer">Delete Server</button>
    </Authorized>
    <NotAuthorized>
        <p>Admin access required to delete servers.</p>
    </NotAuthorized>
</AuthorizeView>
```

### Checking Role in Code

```csharp
@inject AuthenticationStateProvider AuthenticationStateProvider

@code {
    private async Task CheckUserRole()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (user.IsInRole(Roles.SystemAdmin))
        {
            // System admin logic
        }
        else if (user.IsInRole(Roles.Admin))
        {
            // Admin logic
        }
        else if (user.IsInRole(Roles.User))
        {
            // User logic
        }
    }
}
```

## Security Considerations

1. **Role-Based Access Control (RBAC)**: The application uses Azure AD's built-in RBAC features
2. **Token-Based Authentication**: Uses JWT tokens issued by Azure AD
3. **Policy-Based Authorization**: Declarative authorization using policies
4. **Client-Side Authorization**: Note that Blazor WebAssembly runs on the client, so authorization checks are for UX purposes only. Always implement server-side authorization for APIs.

## Future Enhancements

Potential improvements for the authentication system:

1. **API Authorization**: Implement authorization for backend APIs
2. **User Management**: Add UI for SystemAdmins to manage users and roles
3. **Audit Logging**: Log authentication and authorization events
4. **Multi-Factor Authentication**: Enforce MFA for sensitive operations
5. **Permission Levels**: More granular permissions beyond roles
6. **Group-Based Authorization**: Support for Azure AD group-based access

## Troubleshooting

### Users Can't See Role Badges

- Ensure app roles are created in Azure AD
- Verify users are assigned to roles in Enterprise Applications
- Check that the `roles` claim is included in the token

### Authorization Policies Not Working

- Verify policy names match exactly (case-sensitive)
- Check that `AddAuthorizationCore` is called in Program.cs
- Ensure the user is authenticated before checking roles

### Tests Failing

- Run `dotnet build` to ensure no compilation errors
- Check that all required NuGet packages are installed
- Verify test project references the Web project

## Resources

- [Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview)
- [Azure AD App Roles](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps)
- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/introduction)
- [Blazor Authentication and Authorization](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/)
