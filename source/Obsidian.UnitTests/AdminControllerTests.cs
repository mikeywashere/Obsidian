using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Obsidian.Api.Controllers;
using Obsidian.DataAccess;
using Obsidian.Models;
using Obsidian.Models.Authorization;
using System.Security.Claims;

namespace Obsidian.UnitTests;

public class AdminControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ObsidianDbContext _db;

    public AdminControllerTests()
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

    private AdminController CreateController(string granterOid = "granter-oid")
    {
        var controller = new AdminController(_db);
        var claims = new List<Claim> { new("oid", granterOid) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    [Fact]
    public void GetAdminUsers_RequiresSystemAdminRole()
    {
        var classAttr = typeof(AdminController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(classAttr);
        Assert.Equal(Policies.RequireSystemAdmin, classAttr.Policy);
    }

    [Fact]
    public async Task GetAdminUsers_ReturnsListFromDb()
    {
        _db.UserAdminOverrides.AddRange(
            new UserAdminOverride { ObjectId = "oid-1", DisplayName = "Alice", Role = Roles.Admin, GrantedBy = "system" },
            new UserAdminOverride { ObjectId = "oid-2", DisplayName = "Bob", Role = Roles.SystemAdmin, GrantedBy = "system" }
        );
        await _db.SaveChangesAsync();
        var controller = CreateController();

        var result = await controller.GetUsers();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<UserAdminOverride>>(okResult.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public void GrantAdmin_RequiresSystemAdminRole()
    {
        var classAttr = typeof(AdminController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(classAttr);
        Assert.Equal(Policies.RequireSystemAdmin, classAttr.Policy);
    }

    [Fact]
    public async Task GrantAdmin_AddsOverrideToDb()
    {
        var controller = CreateController("granter-123");
        var request = new UserAdminOverride
        {
            ObjectId = "new-user-oid",
            DisplayName = "Charlie",
            Role = Roles.Admin
        };

        var result = await controller.AddUser(request);

        Assert.IsType<CreatedAtActionResult>(result.Result);
        var entry = await _db.UserAdminOverrides.FindAsync("new-user-oid");
        Assert.NotNull(entry);
        Assert.Equal(Roles.Admin, entry.Role);
        Assert.Equal("granter-123", entry.GrantedBy);
    }

    [Fact]
    public void RevokeAdmin_RequiresSystemAdminRole()
    {
        var classAttr = typeof(AdminController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(classAttr);
        Assert.Equal(Policies.RequireSystemAdmin, classAttr.Policy);
    }

    [Fact]
    public async Task RevokeAdmin_RemovesOverrideFromDb()
    {
        _db.UserAdminOverrides.Add(
            new UserAdminOverride { ObjectId = "to-remove", DisplayName = "Dave", Role = Roles.Admin, GrantedBy = "system" }
        );
        await _db.SaveChangesAsync();
        var controller = CreateController();

        var result = await controller.DeleteUser("to-remove");

        Assert.IsType<NoContentResult>(result);
        var entry = await _db.UserAdminOverrides.FindAsync("to-remove");
        Assert.Null(entry);
    }

    [Fact]
    public async Task RevokeAdmin_Returns404_WhenNotFound()
    {
        var controller = CreateController();

        var result = await controller.DeleteUser("nonexistent-oid");

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
