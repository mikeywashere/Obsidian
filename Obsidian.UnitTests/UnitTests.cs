using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Obsidian.UnitTests
{
    public class UnitTests
    {
        [Fact]
        public async Task TestDownloadLinux()
        {
            var sc = new ServiceCollection();
            sc.AddHttpClient();
            sc.AddTransient<IBedrock, Bedrock>();
            var sp = sc.BuildServiceProvider();
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            
            var bedrock = new Bedrock(factory, OSPlatform.Linux);
            var v = await bedrock.Versions();
            v.ShouldNotBeEmpty();
            v.Count().ShouldBeGreaterThan(0);
            BedrockVersion bv = v.First() ?? throw new NullReferenceException();
            bv.ShouldNotBeNull();
            var download = await bv.DownloadAsync();
            download.ShouldNotBeNull();
        }

        [Fact]
        public async Task TestDownloadWindowsAndExtract()
        {
            var sc = new ServiceCollection();
            sc.AddHttpClient();
            sc.AddTransient<IBedrock, Bedrock>();
            var sp = sc.BuildServiceProvider();
            var factory = sp.GetRequiredService<IHttpClientFactory>();

            var bedrock = new Bedrock(factory, OSPlatform.Windows);
            var v = await bedrock.Versions();
            v.ShouldNotBeEmpty();
            v.Count().ShouldBeGreaterThan(0);
            BedrockVersion bv = v.First() ?? throw new NullReferenceException();
            bv.ShouldNotBeNull();
            var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirectory);
            await bv.ExtractToDirectoryAsync(tempDirectory);
        }
    }
}