using Microsoft.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Obsidian;

/// <summary>
/// Represents a specific version of a Minecraft Bedrock server.
/// This class provides functionality to download and extract Bedrock server files
/// for a particular version, platform, and release type (stable or preview).
/// </summary>
/// <remarks>
/// Uses RecyclableMemoryStreamManager for efficient memory usage during downloads
/// and supports both Windows and Linux platforms.
/// </remarks>
public class BedrockVersion(IHttpClientFactory factory)
{
    /// <summary>
    /// Memory stream manager for efficient memory usage during downloads.
    /// Helps reduce GC pressure by recycling memory streams.
    /// </summary>
    private static readonly RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager();

    /// <summary>
    /// Gets or sets the download URL for the Bedrock server package.
    /// This typically points to a ZIP file on the official Minecraft website.
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// Gets or sets the operating system platform for which this Bedrock server is intended.
    /// Supported platforms are Windows and Linux.
    /// </summary>
    public OSPlatform Platform { get; set; }

    /// <summary>
    /// Gets or sets the version string of the Bedrock server.
    /// Typically follows the format "major.minor.patch" (e.g., "1.20.30"), but may occasionally include a fourth component for build numbers (e.g., "1.20.30.2").
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a preview (beta) version.
    /// Preview versions contain experimental features not yet available in stable releases.
    /// </summary>
    /// <remarks>
    /// Defaults to false, indicating a stable release.
    /// </remarks>
    public bool Preview { get; set; } = false;

    /// <summary>
    /// Downloads the file from the specified <see cref="Link"/> using an HTTP client.
    /// The file is streamed into a recyclable memory stream for efficient memory usage.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{Stream}"/> representing the asynchronous operation, with the downloaded data as a stream.
    /// </returns>
    /// <exception cref="NullReferenceException">
    /// Thrown if <see cref="Link"/> is null.
    /// </exception>
    public async Task<Stream> DownloadAsync()
    {
        var managedStream = manager.GetStream();

        if (Link is null)
            throw new NullReferenceException(nameof(Link));
        var client = factory.CreateClient();
        var response = await client.GetStreamAsync(Link);

        await response.CopyToAsync(managedStream);
        managedStream.Position = 0;
        return managedStream;
    }

    /// <summary>
    /// Extracts the downloaded ZIP archive to the specified directory.
    /// The archive is downloaded using <see cref="DownloadAsync"/> and extracted using UTF-8 encoding.
    /// </summary>
    /// <param name="directory">The target directory to extract the contents to.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous extraction operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="directory"/> is null.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown if the specified directory does not exist.
    /// </exception>
    public async Task ExtractToDirectoryAsync(string directory)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory));

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"The directory '{directory}' does not exist.");
        }

        var download = await DownloadAsync();
        ZipFile.ExtractToDirectory(download, directory, System.Text.Encoding.UTF8, true);
    }
}
