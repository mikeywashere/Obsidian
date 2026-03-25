using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Obsidian.Api.Controllers;
using Obsidian.Api.Services;
using Obsidian.Models;

namespace Obsidian.UnitTests;

public class ServersControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOkWithServers()
    {
        var mockServerManager = Substitute.For<IServerManager>();
        var servers = new List<ServerInfo>
        {
            new() { Id = "server-1", Name = "Server 1", Status = ServerStatus.Running },
            new() { Id = "server-2", Name = "Server 2", Status = ServerStatus.Stopped }
        };
        mockServerManager.GetAllAsync().Returns(servers);
        var controller = new ServersController(mockServerManager);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedServers = Assert.IsAssignableFrom<IEnumerable<ServerInfo>>(okResult.Value);
        Assert.Equal(2, returnedServers.Count());
    }

    [Fact]
    public async Task GetById_ReturnsOkWhenFound()
    {
        var mockServerManager = Substitute.For<IServerManager>();
        var server = new ServerInfo { Id = "server-1", Name = "Test Server", Status = ServerStatus.Running };
        mockServerManager.GetAsync("server-1").Returns(server);
        var controller = new ServersController(mockServerManager);

        var result = await controller.Get("server-1");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedServer = Assert.IsType<ServerInfo>(okResult.Value);
        Assert.Equal("server-1", returnedServer.Id);
    }

    [Fact]
    public async Task GetById_Returns404WhenNotFound()
    {
        var mockServerManager = Substitute.For<IServerManager>();
        mockServerManager.GetAsync("nonexistent").Returns((ServerInfo?)null);
        var controller = new ServersController(mockServerManager);

        var result = await controller.Get("nonexistent");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Start_ReturnsOkAndCallsManager()
    {
        var mockServerManager = Substitute.For<IServerManager>();
        var controller = new ServersController(mockServerManager);

        var result = await controller.Start("server-1");

        Assert.IsType<OkResult>(result);
        await mockServerManager.Received(1).StartAsync("server-1");
    }

    [Fact]
    public async Task Start_ReturnsBadRequestOnInvalidOperation()
    {
        var mockServerManager = Substitute.For<IServerManager>();
        mockServerManager.StartAsync("server-1")
            .Returns(Task.FromException(new InvalidOperationException("Server is already running.")));
        var controller = new ServersController(mockServerManager);

        var result = await controller.Start("server-1");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Stop_ReturnsOkAndCallsManager()
    {
        var mockServerManager = Substitute.For<IServerManager>();
        var controller = new ServersController(mockServerManager);

        var result = await controller.Stop("server-1");

        Assert.IsType<OkResult>(result);
        await mockServerManager.Received(1).StopAsync("server-1");
    }

    [Fact]
    public async Task Stop_ReturnsBadRequestOnInvalidOperation()
    {
        var mockServerManager = Substitute.For<IServerManager>();
        mockServerManager.StopAsync("server-1")
            .Returns(Task.FromException(new InvalidOperationException("Server not found.")));
        var controller = new ServersController(mockServerManager);

        var result = await controller.Stop("server-1");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetLogs_ReturnsOkWithLogs()
    {
        var mockServerManager = Substitute.For<IServerManager>();
        var logs = new List<ServerLog>
        {
            new() { Timestamp = DateTime.UtcNow, Level = LogLevel.Info, Message = "Log 1" },
            new() { Timestamp = DateTime.UtcNow, Level = LogLevel.Info, Message = "Log 2" },
            new() { Timestamp = DateTime.UtcNow, Level = LogLevel.Error, Message = "Log 3" }
        };
        mockServerManager.GetLogsAsync("server-1", 100).Returns(logs);
        var controller = new ServersController(mockServerManager);

        var result = await controller.GetLogs("server-1", 100);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedLogs = Assert.IsAssignableFrom<IEnumerable<ServerLog>>(okResult.Value);
        Assert.Equal(3, returnedLogs.Count());
    }

    [Fact]
    public async Task GetLogs_PassesMaxLinesToManager()
    {
        var mockServerManager = Substitute.For<IServerManager>();
        mockServerManager.GetLogsAsync("server-1", 50).Returns(new List<ServerLog>());
        var controller = new ServersController(mockServerManager);

        await controller.GetLogs("server-1", 50);

        await mockServerManager.Received(1).GetLogsAsync("server-1", 50);
    }
}
