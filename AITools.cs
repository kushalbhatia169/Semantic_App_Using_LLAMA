using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Diagnostics;

namespace Semantic_App_Using_LLAMA;

public class AITools(IChatCompletionService chatService)
{
    private readonly IChatCompletionService _chatService = chatService;

    [KernelFunction]
    [Description("Counts the number of files in a folder")]
    public static int GetFileCount(string path)
    {
        Console.WriteLine("[TOOL] GetFileCount");
        return Directory.Exists(path) ? Directory.GetFiles(path).Length : 0;
    }

    [KernelFunction]
    [Description("Gets the current system time")]
    public static string GetCurrentTime()
    {
        Console.WriteLine("[TOOL] GetCurrentTime");
        return DateTime.Now.ToString("F");
    }

    [KernelFunction]
    [Description("Creates a new folder")]
    public static string CreateFolder(string path)
    {
        try
        {
            if (Directory.Exists(path))
                return "Directory already exists.";

            Directory.CreateDirectory(path);
            return $"Folder created at {path}";
        }
        catch (Exception ex)
        {
            return $"Error creating folder: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Finds files with a given extension")]
    public static List<string?> GetFilesByExtension(string path, string extension)
    {
        Console.WriteLine($"[TOOL] Searching {extension} in {path}");

        if (!Directory.Exists(path))
            return [];

        return [.. Directory
            .EnumerateFiles(path, $"*{extension}")
            .Select(Path.GetFileName)];
    }

    [KernelFunction]
    [Description("Generates a folder organization preview")]
    public static string GetOrganizationPreview(string path)
    {
        if (!Directory.Exists(path))
            return "Path not found.";

        var files = Directory.GetFiles(path);

        if (files.Length == 0)
            return "Folder is empty.";

        var counts = files
            .GroupBy(f => Path.GetExtension(f).ToLower())
            .Select(g => $"{g.Key}: {g.Count()} files");

        return $"Found {files.Length} files. Plan: {string.Join(", ", counts)}. Should I proceed?";
    }

    [KernelFunction]
    [Description("Executes the organization plan")]
    public static string ExecuteMove(string path)
    {
        if (!Directory.Exists(path))
            return "Path not found.";

        var files = Directory.GetFiles(path);
        int moved = 0;

        foreach (var file in files)
        {
            string ext = Path.GetExtension(file).ToLower();

            string folder = ext switch
            {
                ".jpg" or ".png" => "Photos",
                ".pdf" or ".docx" or ".txt" => "Documents",
                ".cs" or ".json" => "Specpoint_Dev",
                _ => "Others"
            };

            string target = Path.Combine(path, folder);
            Directory.CreateDirectory(target);

            File.Move(file, Path.Combine(target, Path.GetFileName(file)), true);
            moved++;
        }

        return $"Moved {moved} files.";
    }

    [KernelFunction]
    [Description("Summarizes a file")]
    public async Task<string> SummarizeFile(string filePath)
    {
        Console.WriteLine($"[TOOL] Summarizing {filePath}");

        return await FileSummarizer.SummarizeFileAsync(filePath, _chatService);
    }

    [KernelFunction]
    [Description("Opens a folder")]
    public static string OpenFolder(string path)
    {
        if (!Directory.Exists(path))
            return "Folder not found.";

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });

        return $"Opened folder {path}";
    }

    [KernelFunction]
    [Description("Launches a file, folder, or app")]
    public static string Launch(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });

            return $"Opened {path}";
        }
        catch
        {
            return "Failed to open.";
        }
    }
}