// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Obsidian;
using System.Runtime.InteropServices;

Console.WriteLine("Bedrock downloader");

var sc = new ServiceCollection();
sc.AddHttpClient();
sc.AddTransient<IBedrock, Bedrock>();
var sp = sc.BuildServiceProvider();
var factory = sp.GetRequiredService<IHttpClientFactory>();

var bedrock = new Bedrock(factory, RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux : OSPlatform.Windows);
var v = await bedrock.Versions();
Console.WriteLine($"Found {v.Count()} versions.");
Console.WriteLine(
    $"{string.Join(
        $"{Environment.NewLine}",
        v.Select(a => $"{a.Version}{(a.Preview ? " preview" : "")}")
        .ToList())}");
UdpProxy proxy = new UdpProxy(19132, "192.168.0.29", 19132);
await proxy.StartAsync();

Console.ReadLine();