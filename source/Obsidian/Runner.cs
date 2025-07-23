using System.Diagnostics;

namespace Obsidian;

/// <summary>
/// Provides functionality to host and launch the Obsidian application in a specified directory.
/// Acts as a wrapper for process management related to Obsidian execution.
/// </summary>
/// <remarks>
/// This class is responsible for verifying directory existence and proper process initialization.
/// Error handling is included for process startup failures.
/// </remarks>
public class Host()
{
    /// <summary>
    /// Launches the Obsidian application in the specified directory.
    /// </summary>
    /// <param name="directory">The target directory where Obsidian should run. 
    /// This directory must exist for successful execution.</param>
    /// <remarks>
    /// The method performs the following steps:
    /// 1. Validates the existence of the specified directory
    /// 2. Sets up a Process with appropriate working directory
    /// 3. Attempts to start the Obsidian process
    /// 4. Handles and logs any exceptions that occur during startup
    /// 
    /// The method assumes that the "obsidian" executable is available in the system PATH
    /// or alternatively, the FileName property can be modified to use a full path.
    /// </remarks>
    public void Run(string directory)
    {
        // Validate the input directory parameter
        if (string.IsNullOrWhiteSpace(directory))
        {
            Console.WriteLine("The specified directory is invalid. Please provide a non-empty directory path.");
            return;
        }
        
        // Verify that the target directory exists before attempting to run
        if (Directory.Exists(directory))
        {
            Console.WriteLine($"Directory {directory} exists. Running Obsidian in {directory}.");
            
            // Configure process settings for Obsidian execution
            Process process = new Process();
            process.StartInfo.WorkingDirectory = directory; // Set working directory for the process
            process.StartInfo.FileName = "obsidian"; // Executable name (should be in PATH or use full path)
            
            // Attempt to start the process with exception handling
            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                // Log error if process fails to start
                Console.WriteLine($"Failed to start the process: {ex.Message}");
            }
        }
        else
        {
            // Notify user if the specified directory doesn't exist
            Console.WriteLine($"Directory {directory} does not exist. Please create it before running Obsidian.");
        }
    }
}
