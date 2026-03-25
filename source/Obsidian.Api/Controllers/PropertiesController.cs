using Microsoft.AspNetCore.Mvc;
using Obsidian.Api.Services;

namespace Obsidian.Api.Controllers;

[ApiController]
[Route("api/servers/{serverId}/properties")]
public class PropertiesController : ControllerBase
{
    private readonly IServerManager _serverManager;

    public PropertiesController(IServerManager serverManager)
    {
        _serverManager = serverManager;
    }

    [HttpGet]
    public async Task<ActionResult<ServerProperties>> Get(string serverId)
    {
        var server = await _serverManager.GetAsync(serverId);
        if (server == null)
        {
            return NotFound(new { error = $"Server '{serverId}' not found." });
        }

        var propertiesPath = Path.Combine(GetServerInstallPath(serverId), "server.properties");
        if (!System.IO.File.Exists(propertiesPath))
        {
            return NotFound(new { error = "server.properties file not found." });
        }

        try
        {
            var properties = ServerProperties.Load(propertiesPath);
            return Ok(properties);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to load properties: {ex.Message}" });
        }
    }

    [HttpPut]
    public async Task<IActionResult> Put(string serverId, [FromBody] ServerProperties properties)
    {
        var server = await _serverManager.GetAsync(serverId);
        if (server == null)
        {
            return NotFound(new { error = $"Server '{serverId}' not found." });
        }

        var propertiesPath = Path.Combine(GetServerInstallPath(serverId), "server.properties");

        try
        {
            properties.Save(propertiesPath);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to save properties: {ex.Message}" });
        }
    }

    private string GetServerInstallPath(string serverId)
    {
        return _serverManager.GetInstallPath(serverId) 
            ?? throw new InvalidOperationException($"Install path not found for server '{serverId}'.");
    }
}
