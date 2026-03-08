using Azure;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
namespace Semantic_App_Using_LLAMA;

public static class FileSummarizer
{
    public static async Task<string> SummarizeFileAsync(string filePath, Kernel kernel, IChatCompletionService chatService, ChatHistory history)
    {
        if (!File.Exists(filePath))
            return $"❌ File not found: {filePath}";

        try
        {
            // Read file (limit to first 10k chars for large files)
            string content = await File.ReadAllTextAsync(filePath);
            if (content.Length > 10000)
                content = string.Concat(content.AsSpan(0, 10000), "\n\n[Content truncated...]");

            // Create summarization prompt
            history.AddSystemMessage(@"
            You are an expert document summarizer. Provide:
            1. 2-3 sentence summary of key points
            2. Main topics covered
            3. Any action items or next steps mentioned
            Keep it concise but comprehensive.
        ");
            history.AddUserMessage($"Summarize this document:\n\n{content}");

            var replies = await chatService.GetChatMessageContentsAsync(history, kernel: kernel);
            if (replies == null || replies.Count == 0 || string.IsNullOrEmpty(replies[0].Content))
            {
                throw new Exception("❌ No summary generated or content is empty.");
            }
            else
            {
                var assistantReplies = replies[0].Content!; // safe because of the previous check
                                                            // 4. AI ka jawab history mein save karein
                history.AddAssistantMessage(assistantReplies);
                return assistantReplies;
            }
        }
        catch (Exception ex)
        {
            return $"❌ Error reading file: {ex.Message}";
        }
    }
}