using Microsoft.AspNetCore.Mvc;
using Obsidian.Api.Services;
using Obsidian.Models;

namespace Obsidian.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServersController : ControllerBase
{
    private readonly IServerManager _serverManager;

    public ServersController(IServerManager serverManager)
    {
        _serverManager = serverManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServerInfo>>> GetAll()
    {
        var servers = await _serverManager.GetAllAsync();
        return Ok(servers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerInfo>> Get(string id)
    {
        var server = await _serverManager.GetAsync(id);
        if (server == null)
        {
            return NotFound();
        }
        return Ok(server);
    }

    [HttpGet("{id}/logs")]
    public async Task<ActionResult<IEnumerable<ServerLog>>> GetLogs(string id, [FromQuery] int maxLines = 100)
    {
        var logs = await _serverManager.GetLogsAsync(id, maxLines);
        return Ok(logs);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(string id)
    {
        try
        {
            await _serverManager.StartAsync(id);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> Stop(string id)
    {
        try
        {
            await _serverManager.StopAsync(id);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
