using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Obsidian.Api.Controllers;
using Obsidian.Api.Services;
using Obsidian.Models;

namespace Obsidian.UnitTests;

public class PlayersControllerTests
{
    [Fact]
    public void GetPlayers_ReturnsOkWithPlayerList()
    {
        var tracker = Substitute.For<IPlayerTracker>();
        var now = DateTime.UtcNow;
        var players = new List<PlayerInfo>
        {
            new("server-1", "Steve", "2535416409", now, now),
            new("server-1", "Alex", "9876543210", now, now)
        };
        tracker.GetPlayers("server-1").Returns(players);
        var controller = new PlayersController(tracker);

        var result = controller.GetPlayers("server-1");

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<PlayerInfo>>(ok.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public void GetPlayers_ReturnsEmptyList_WhenNoPlayersOnline()
    {
        var tracker = Substitute.For<IPlayerTracker>();
        tracker.GetPlayers("server-1").Returns(Enumerable.Empty<PlayerInfo>());
        var controller = new PlayersController(tracker);

        var result = controller.GetPlayers("server-1");

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<PlayerInfo>>(ok.Value);
        Assert.Empty(returned);
    }
}
