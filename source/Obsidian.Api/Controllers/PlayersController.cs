using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obsidian.Api.Services;
using Obsidian.Models;
using Obsidian.Models.Authorization;

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
    [Authorize(Policy = Policies.RequireUser)]
    public IActionResult GetPlayers(string serverId) =>
        Ok(_playerTracker.GetPlayers(serverId));
}
