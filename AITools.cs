using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Semantic_App_Using_LLAMA
{
    public class AITools
    {
        [KernelFunction] // Ye attribute AI ko batata hai ki ye "Tool" hai
        [Description("Counts the number of files in a given folder path.")]
        public static int GetFileCount([Description("The full path of the folder")] string path)
        {
            Console.WriteLine($"[INTERNAL LOG]: Executing GetFileCount Function");
            return Directory.GetFiles(path).Length;
        }

        [KernelFunction]
        [Description("Gets the current system time.")]
        public static string GetCurrentTime() 
        {
            Console.WriteLine($"[INTERNAL LOG]: Executing GetCurrentTime Function");
            return DateTime.Now.ToString("F");
        }

        [KernelFunction]
        [Description("Creates a new folder at the specified path.")]
        public static string CreateFolder([Description("The full path of the folder to create")] string path)
        {
            // THIS LOG PROVES IF THE CODE IS ACTUALLY RUNNING
            Console.WriteLine($"[INTERNAL LOG]: Executing CreateDirectory for path: {path}");

            try
            {
                if (Directory.Exists(path)) return "Directory already exists.";
                Directory.CreateDirectory(path);
                return $"SUCCESS: Directory created at {path}";
            }
            catch (Exception ex)
            {
                return $"ERROR: Could not create directory. {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Finds and lists files with a specific extension in a directory.")]
        public List<string> GetFilesByExtension(
        [Description("Full path to the directory")] string path,
        [Description("The file extension to look for (e.g., .cs, .json, .pdf)")] string extension)
        {
            Console.WriteLine($"[INTERNAL LOG]: Searching for {extension} files in {path}");
            var files = Directory.GetFiles(path, $"*{extension}")
                                 .Select(Path.GetFileName)
                                 .ToList();
            return files;
        }

        [KernelFunction]
        [Description("Scans a folder and returns a count of files and a plan for organization. Call this BEFORE moving files.")]
        public string GetOrganizationPreview([Description("The path to the folder")] string path)
        {
            if (!Directory.Exists(path)) return "Path not found.";

            Console.WriteLine("[INTERNAL LOG]: Generating Organization Preview...");
            var files = Directory.GetFiles(path);
            var counts = files.GroupBy(f => Path.GetExtension(f).ToLower())
                              .Select(g => $"{g.Key}: {g.Count()} files")
                              .ToList();

            return $"I found {files.Length} files. Plan: Move them by extension into subfolders. " +
                   $"Breakdown: {string.Join(", ", counts)}. Should I proceed with the move?";
        }

        [KernelFunction]
        [Description("Actually moves files into categorized subfolders. ONLY call this after the user says 'Yes' or 'Proceed'.")]
        public string ExecuteMove([Description("The path to the folder")] string path)
        {
            if (!Directory.Exists(path)) return "Path not found.";

            Console.WriteLine("[INTERNAL LOG]: Executing Actual File Move...");
            var files = Directory.GetFiles(path);
            int count = 0;

            foreach (var file in files)
            {
                string ext = Path.GetExtension(file).ToLower();
                string folderName = ext switch
                {
                    ".jpg" or ".png" => "Photos",
                    ".pdf" or ".docx" or ".txt" => "Documents",
                    ".cs" or ".json" => "Specpoint_Dev",
                    _ => "Others"
                };

                string targetDir = Path.Combine(path, folderName);
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                File.Move(file, Path.Combine(targetDir, Path.GetFileName(file)));
                count++;
            }
            return $"Success! {count} files moved to categorized folders.";
        }
    }
}
