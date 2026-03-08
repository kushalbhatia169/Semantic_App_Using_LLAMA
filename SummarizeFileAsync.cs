using Microsoft.SemanticKernel.ChatCompletion;

namespace Semantic_App_Using_LLAMA;

public static class FileSummarizer
{
    public static async Task<string> SummarizeFileAsync(
        string filePath,
        IChatCompletionService chatService)
    {
        if (!File.Exists(filePath))
            return "File not found.";

        try
        {
            string content = await File.ReadAllTextAsync(filePath);

            if (content.Length > 8000)
                content = content[..8000];

            var history = new ChatHistory(
                "You summarize files in 2 lines and list key action items."
            );

            history.AddUserMessage(content);

            var result = await chatService.GetChatMessageContentAsync(history);

            return result.Content ?? "Summary failed.";
        }
        catch (Exception ex)
        {
            return $"Error summarizing file: {ex.Message}";
        }
    }
}