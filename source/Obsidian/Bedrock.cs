using Microsoft.Playwright;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Obsidian
{
    /// <summary>
    /// Provides functionality for retrieving Minecraft Bedrock server versions from the official website.
    /// This class handles platform-specific server downloads (Windows/Linux) and supports both release and preview versions.
    /// </summary>
    /// <remarks>
    /// The class uses Playwright for browser automation to interact with the Minecraft download page,
    /// which requires JavaScript execution to fully render and access download links.
    /// </remarks>
    public class Bedrock : IBedrock
    {
        /// <summary>Factory for creating HTTP clients used for download operations</summary>
        private IHttpClientFactory factory;

        /// <summary>Target operating system platform for server downloads (Windows or Linux)</summary>
        private OSPlatform findPlatform;

        /// <summary>Locale for the Minecraft download page (e.g., "en-us")</summary>
        private string findLocale;

        /// <summary>Whether to search for preview (beta) versions instead of stable releases</summary>
        private bool findPreview;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bedrock"/> class for retrieving Minecraft Bedrock server versions.
        /// </summary>
        /// <param name="factory">HTTP client factory for download operations</param>
        /// <param name="findPlatform">Target operating system platform (Windows or Linux)</param>
        /// <param name="preview">Whether to search for preview (beta) versions instead of stable releases</param>
        /// <param name="findLocale">Locale for the Minecraft download page (defaults to "en-us")</param>
        /// <exception cref="ArgumentException">Thrown if an unsupported platform is specified</exception>
        /// <exception cref="ArgumentNullException">Thrown if the factory parameter is null</exception>
        public Bedrock(IHttpClientFactory factory, OSPlatform findPlatform, bool preview = false, string findLocale = "en-us")
        {
            if (findPlatform != OSPlatform.Windows && findPlatform != OSPlatform.Linux)
                throw new ArgumentException("findPlatform must be either Windows or Linux", nameof(findPlatform));

            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.findPlatform = findPlatform;
            this.findLocale = findLocale;
            this.findPreview = preview;
        }

        /// <summary>
        /// Uses Playwright to load the Minecraft Bedrock server download page and execute JavaScript.
        /// </summary>
        /// <returns>The HTML content of the page after JavaScript execution</returns>
        /// <remarks>
        /// This method automates browser interactions including:
        /// - Loading the download page with proper user agent and locale settings
        /// - Waiting for dynamic content to load
        /// - Clicking appropriate UI elements based on preview settings
        /// - Accepting license agreements by clicking checkboxes
        /// - Retrieving the final rendered HTML
        /// </remarks>
        private async Task<string> GetHtmlWithJavaScriptExecutionAsync()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                // Headless = true // Run browser in headless mode (no GUI)
                Headless = false, // Run browser in gui mode
                Timeout = 60000 // Set a timeout for browser launch
            });

            // Create a new browser context
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
                Locale = "en-US"
            });

            // Open a new page
            var page = await context.NewPageAsync();

            // Navigate to the Minecraft server download page
            await page.GotoAsync($"https://www.minecraft.net/{findLocale}/download/server/bedrock");

            // Wait for the page to be fully loaded with JavaScript execution
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Wait for specific UI elements that might be dynamically loaded
            await page.WaitForSelectorAsync("a[href*=\"bedrockdedicatedserver/bin-\"]", new PageWaitForSelectorOptions
            {
                Timeout = 10000 // 10 seconds timeout
            });

            if (findPreview)
            {
                // Select preview versions if requested
                await page.ClickAsync("#MC_RadioGroupA_Server_1_input_1");
                await page.ClickAsync("#MC_RadioGroupA_Server_2_input_1");
            }

            // Accept license agreements by clicking checkboxes
            await page.ClickAsync("#MC_CheckboxA_Server_1_input_0 > input");
            await page.ClickAsync("#MC_CheckboxA_Server_2_input_0 > input");

            // Get the final HTML content after JavaScript execution
            var content = await page.ContentAsync();

            // Optionally save the content for debugging purposes
            File.WriteAllText("bedrock-page-with-js.html", content);

            return content;
        }

        /// <summary>
        /// Retrieves a list of available Bedrock server versions for the configured platform and preview setting.
        /// Downloads the Minecraft Bedrock server download page, parses available server versions,
        /// and returns those matching the specified platform and preview criteria.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, with a collection of <see cref="BedrockVersion"/> objects
        /// that match the current platform and preview settings.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if an unknown platform is encountered in the download links.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// Thrown if a regex match is unexpectedly null.
        /// </exception>
        /// <remarks>
        /// The method performs these main steps:
        /// 1. Retrieves the HTML content of the Minecraft download page with JavaScript execution
        /// 2. Uses regex to extract server version download links with metadata
        /// 3. Parses the extracted data into BedrockVersion objects
        /// 4. Filters the versions based on the configured platform and preview settings
        /// </remarks>
        public async Task<IEnumerable<BedrockVersion>> Versions()
        {
            // Use Playwright to get the HTML content with JavaScript execution
            var html = await GetHtmlWithJavaScriptExecutionAsync();

            // Use a more robust regex pattern that can match different HTML structures
            var re = new Regex(@"<a\s+[^>]*href=""(?<Link>https://www\.minecraft\.net/bedrockdedicatedserver/bin-(?<Platform>win|linux)(?<Preview>-preview|)/bedrock-server-(?<Version>\d{1,2}\.\d{1,2}\.\d{1,2}\.\d{1,2})\.zip)[^>]*""");

            var matches = re.Matches(html);
            var versions = new List<BedrockVersion>();
            foreach (var m in matches)
            {
                var match = m as Match;
                if (match is null)
                    continue;

                // Extract version information from regex match groups
                var link = match.Groups["Link"].Value;
                var platform = match.Groups["Platform"].Value;
                var version = match.Groups["Version"].Value;
                var preview = match.Groups["Preview"].Value;

                // Create BedrockVersion object with extracted data
                var bv = new BedrockVersion(factory)
                {
                    Link = link,
                    Platform = platform switch
                    {
                        "win" => OSPlatform.Windows,
                        "linux" => OSPlatform.Linux,
                        _ => throw new ArgumentException($"Unknown platform: {platform}", nameof(platform))
                    },
                    Version = version,
                    Preview = preview.Length > 0
                };

                // Filter versions based on configured platform and preview settings
                if (bv.Platform == OSPlatform.Windows && bv.Preview == findPreview && findPlatform == OSPlatform.Windows)
                {
                    versions.Add(bv);
                }
                else if (bv.Platform == OSPlatform.Linux && bv.Preview == findPreview && findPlatform == OSPlatform.Linux)
                {
                    versions.Add(bv);
                }
            }
            return versions;
        }
    }
}