using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian
{
    public class Host()
    {
        public void Run(string directory)
        {
            if (Directory.Exists(directory))
            {
                Console.WriteLine($"Directory {directory} exists. Running Obsidian in {directory}.");
                Process process = new Process();
                process.StartInfo.WorkingDirectory = directory; // Assuming Obsidian is a .NET application
                process.StartInfo.FileName = "obsidian"; // Assuming obsidian is in PATH or you can specify the full path
                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to start the process: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Directory {directory} does not exist. Please create it before running Obsidian.");
            }
        }
    }
}
