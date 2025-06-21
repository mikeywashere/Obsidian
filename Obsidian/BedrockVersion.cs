using Microsoft.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Obsidian
{
    public class BedrockVersion(IHttpClientFactory factory)
    {
        private static readonly RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager();

        public string? Link { get; set; }

        public OSPlatform Platform { get; set; }

        public string? Version { get; set; }

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
            
            response.CopyTo(managedStream);
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
}
