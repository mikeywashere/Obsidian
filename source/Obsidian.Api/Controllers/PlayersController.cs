using Microsoft.AspNetCore.Mvc;
using Obsidian.Api.Services;
using Obsidian.Models;

namespace Obsidian.Api.Controllers;

[ApiController]
[Route("api/servers/{serverId}/players")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerTracker _playerTracker;

    public PlayersController(IPlayerTracker playerTracker)
    {
        _playerTracker = playerTracker;
    }

    [HttpGet]
    public IActionResult GetPlayers(string serverId) =>
        Ok(_playerTracker.GetPlayers(serverId));
}
