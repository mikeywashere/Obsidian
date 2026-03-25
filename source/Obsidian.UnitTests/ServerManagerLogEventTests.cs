using Obsidian.Api.Services;
using Obsidian.Models;

namespace Obsidian.UnitTests;

public class ServerManagerLogEventTests
{
    [Fact]
    public void GetInstallPath_ReturnsNullForUnknownServer()
    {
        var manager = new ServerManager();

        var path = manager.GetInstallPath("unknown-id");

        Assert.Null(path);
    }

    [Fact]
    public void GetInstallPath_ReturnsPathForRegisteredServer()
    {
        var manager = new ServerManager();

        var serverInfo = manager.RegisterServer("Test Server", "C:\\test-bedrock", 19132);
        var path = manager.GetInstallPath(serverInfo.Id);

        Assert.NotNull(path);
        Assert.Equal("C:\\test-bedrock", path);
    }

    [Fact]
    public void RegisterServer_StoresInstallPath()
    {
        var manager = new ServerManager();

        var serverInfo = manager.RegisterServer("Bedrock Test", "C:\\test-bedrock", 25565);
        var retrievedPath = manager.GetInstallPath(serverInfo.Id);

        Assert.Equal("C:\\test-bedrock", retrievedPath);
    }

    // Note: LogReceived event testing requires integration with BedrockProcess stdout redirection
    // This is an integration test boundary. The event is raised from ServerManager.OnOutputDataReceived
    // which requires a real process spawn. Unit testing this would require:
    //   1. Mocking Process (complex and brittle)
    //   2. Internal access to raise events manually (breaks encapsulation)
    //   3. Integration test with actual process execution
    // Decision: Test LogReceived subscription in ServerLogBroadcasterTests (consumer side).
    // The producer side (ServerManager firing event on process output) is best tested via integration tests.
}
