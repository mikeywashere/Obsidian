using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Obsidian.Api.Services;
using Obsidian.DataAccess;
using Obsidian.Models;
using Obsidian.Models.Authorization;
using System.Security.Claims;

namespace Obsidian.UnitTests;

public class AdminOverrideClaimsTransformationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ObsidianDbContext _db;

    public AdminOverrideClaimsTransformationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ObsidianDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new ObsidianDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static ClaimsPrincipal CreatePrincipalWithOid(string oid)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser"),
            new("oid", oid)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreatePrincipalWithoutOid()
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, "testuser") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task TransformAsync_AddsRoleClaim_WhenUserHasAdminOverride()
    {
        _db.UserAdminOverrides.Add(new UserAdminOverride
        {
            ObjectId = "oid-admin",
            DisplayName = "Alice",
            Role = Roles.Admin,
            GrantedBy = "system"
        });
        await _db.SaveChangesAsync();
        var transformation = new AdminOverrideClaimsTransformation(_db);
        var principal = CreatePrincipalWithOid("oid-admin");

        var result = await transformation.TransformAsync(principal);

        Assert.True(result.IsInRole(Roles.Admin));
        Assert.False(result.IsInRole(Roles.SystemAdmin));
    }

    [Fact]
    public async Task TransformAsync_AddsSystemAdminClaim_WhenUserHasSystemAdminOverride()
    {
        _db.UserAdminOverrides.Add(new UserAdminOverride
        {
            ObjectId = "oid-sysadmin",
            DisplayName = "Bob",
            Role = Roles.SystemAdmin,
            GrantedBy = "system"
        });
        await _db.SaveChangesAsync();
        var transformation = new AdminOverrideClaimsTransformation(_db);
        var principal = CreatePrincipalWithOid("oid-sysadmin");

        var result = await transformation.TransformAsync(principal);

        Assert.True(result.IsInRole(Roles.SystemAdmin));
    }

    [Fact]
    public async Task TransformAsync_DoesNotAddClaim_WhenNoOverrideExists()
    {
        var transformation = new AdminOverrideClaimsTransformation(_db);
        var principal = CreatePrincipalWithOid("oid-no-override");

        var result = await transformation.TransformAsync(principal);

        Assert.Same(principal, result);
        Assert.False(result.IsInRole(Roles.Admin));
        Assert.False(result.IsInRole(Roles.SystemAdmin));
    }

    [Fact]
    public async Task TransformAsync_ReturnsPrincipalUnchanged_WhenNoOidClaim()
    {
        var transformation = new AdminOverrideClaimsTransformation(_db);
        var principal = CreatePrincipalWithoutOid();

        var result = await transformation.TransformAsync(principal);

        Assert.Same(principal, result);
    }
}
