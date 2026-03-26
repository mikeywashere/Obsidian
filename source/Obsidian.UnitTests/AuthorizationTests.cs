using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Obsidian.Web.Authorization;
using Shouldly;
using System.Security.Claims;

namespace Obsidian.UnitTests;

public class AuthorizationTests
{
    [Fact]
    public void Roles_ShouldHaveCorrectConstants()
    {
        // Assert
        Roles.SystemAdmin.ShouldBe("SystemAdmin");
        Roles.Admin.ShouldBe("Admin");
        Roles.User.ShouldBe("User");
    }

    [Fact]
    public void Roles_AllRoles_ShouldContainAllRoles()
    {
        // Assert
        Roles.AllRoles.ShouldContain(Roles.SystemAdmin);
        Roles.AllRoles.ShouldContain(Roles.Admin);
        Roles.AllRoles.ShouldContain(Roles.User);
        Roles.AllRoles.Length.ShouldBe(3);
    }

    [Fact]
    public void Policies_ShouldHaveCorrectConstants()
    {
        // Assert
        Policies.RequireSystemAdmin.ShouldBe("RequireSystemAdmin");
        Policies.RequireAdmin.ShouldBe("RequireAdmin");
        Policies.RequireUser.ShouldBe("RequireUser");
    }

    [Fact]
    public async Task SystemAdminPolicy_ShouldAllowSystemAdmin()
    {
        // Arrange
        var options = new AuthorizationOptions();
        options.AddPolicy(Policies.RequireSystemAdmin, policy =>
            policy.RequireRole(Roles.SystemAdmin));

        var user = CreateUserWithRole(Roles.SystemAdmin);
        var authorizationService = CreateAuthorizationService(options);

        // Act
        var result = await authorizationService.AuthorizeAsync(user, Policies.RequireSystemAdmin);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task SystemAdminPolicy_ShouldDenyAdmin()
    {
        // Arrange
        var options = new AuthorizationOptions();
        options.AddPolicy(Policies.RequireSystemAdmin, policy =>
            policy.RequireRole(Roles.SystemAdmin));

        var user = CreateUserWithRole(Roles.Admin);
        var authorizationService = CreateAuthorizationService(options);

        // Act
        var result = await authorizationService.AuthorizeAsync(user, Policies.RequireSystemAdmin);

        // Assert
        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task AdminPolicy_ShouldAllowAdmin()
    {
        // Arrange
        var options = new AuthorizationOptions();
        options.AddPolicy(Policies.RequireAdmin, policy =>
            policy.RequireRole(Roles.Admin, Roles.SystemAdmin));

        var user = CreateUserWithRole(Roles.Admin);
        var authorizationService = CreateAuthorizationService(options);

        // Act
        var result = await authorizationService.AuthorizeAsync(user, Policies.RequireAdmin);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task AdminPolicy_ShouldAllowSystemAdmin()
    {
        // Arrange
        var options = new AuthorizationOptions();
        options.AddPolicy(Policies.RequireAdmin, policy =>
            policy.RequireRole(Roles.Admin, Roles.SystemAdmin));

        var user = CreateUserWithRole(Roles.SystemAdmin);
        var authorizationService = CreateAuthorizationService(options);

        // Act
        var result = await authorizationService.AuthorizeAsync(user, Policies.RequireAdmin);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task AdminPolicy_ShouldDenyUser()
    {
        // Arrange
        var options = new AuthorizationOptions();
        options.AddPolicy(Policies.RequireAdmin, policy =>
            policy.RequireRole(Roles.Admin, Roles.SystemAdmin));

        var user = CreateUserWithRole(Roles.User);
        var authorizationService = CreateAuthorizationService(options);

        // Act
        var result = await authorizationService.AuthorizeAsync(user, Policies.RequireAdmin);

        // Assert
        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task UserPolicy_ShouldAllowUser()
    {
        // Arrange
        var options = new AuthorizationOptions();
        options.AddPolicy(Policies.RequireUser, policy =>
            policy.RequireRole(Roles.User, Roles.Admin, Roles.SystemAdmin));

        var user = CreateUserWithRole(Roles.User);
        var authorizationService = CreateAuthorizationService(options);

        // Act
        var result = await authorizationService.AuthorizeAsync(user, Policies.RequireUser);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task UserPolicy_ShouldAllowAdmin()
    {
        // Arrange
        var options = new AuthorizationOptions();
        options.AddPolicy(Policies.RequireUser, policy =>
            policy.RequireRole(Roles.User, Roles.Admin, Roles.SystemAdmin));

        var user = CreateUserWithRole(Roles.Admin);
        var authorizationService = CreateAuthorizationService(options);

        // Act
        var result = await authorizationService.AuthorizeAsync(user, Policies.RequireUser);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task UserPolicy_ShouldAllowSystemAdmin()
    {
        // Arrange
        var options = new AuthorizationOptions();
        options.AddPolicy(Policies.RequireUser, policy =>
            policy.RequireRole(Roles.User, Roles.Admin, Roles.SystemAdmin));

        var user = CreateUserWithRole(Roles.SystemAdmin);
        var authorizationService = CreateAuthorizationService(options);

        // Act
        var result = await authorizationService.AuthorizeAsync(user, Policies.RequireUser);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task UserPolicy_ShouldDenyUnauthenticatedUser()
    {
        // Arrange
        var options = new AuthorizationOptions();
        options.AddPolicy(Policies.RequireUser, policy =>
            policy.RequireRole(Roles.User, Roles.Admin, Roles.SystemAdmin));

        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var authorizationService = CreateAuthorizationService(options);

        // Act
        var result = await authorizationService.AuthorizeAsync(user, Policies.RequireUser);

        // Assert
        result.Succeeded.ShouldBeFalse();
    }

    private static ClaimsPrincipal CreateUserWithRole(string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    private static IAuthorizationService CreateAuthorizationService(AuthorizationOptions options)
    {
        var policyProvider = new DefaultAuthorizationPolicyProvider(
            Microsoft.Extensions.Options.Options.Create(options));

        var handlers = new List<IAuthorizationHandler>
        {
            new PassThroughAuthorizationHandler()
        };

        return new DefaultAuthorizationService(
            policyProvider,
            new DefaultAuthorizationHandlerProvider(handlers),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DefaultAuthorizationService>.Instance,
            new DefaultAuthorizationHandlerContextFactory(),
            new DefaultAuthorizationEvaluator(),
            Microsoft.Extensions.Options.Options.Create(options));
    }

    private class PassThroughAuthorizationHandler : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RolesAuthorizationRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                foreach (var role in requirement.AllowedRoles)
                {
                    if (context.User.IsInRole(role))
                    {
                        context.Succeed(requirement);
                        break;
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
