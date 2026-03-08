using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Semantic_App_Using_LLAMA;

class Program
{
    static async Task Main()
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddHttpClient("OllamaClient", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        builder.AddOpenAIChatCompletion(
            modelId: "qwen2.5:7b",
            apiKey: "none",
            endpoint: new Uri("http://localhost:11434/v1")
        );

        var kernel = builder.Build();

        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Register tools
        kernel.Plugins.AddFromObject(new AITools(chatService), "AITools");

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.1
        };

        //var answer = await kernel.InvokeAsync("AITools", "GetFileCount",
        //   new() { ["path"] = @"C:\Users\kusha\Downloads" });
        //Console.WriteLine($"Direct Tool Answer (File Count): {answer}");

        //// --- AGENT PROMPT TRIGGER ---
        //Console.WriteLine("\n--- Phase 2: Agent Intent Execution ---");

        //string prompt = "Tell me the current system time then Check file count in C:\\Users\\kusha\\Downloads and create 'Specpoint_Verified' folder there.";

        //// Passing kernel here is MUST for auto-invocation
        //var result = await kernel.InvokePromptAsync(prompt, new(settings));
        //Console.WriteLine($"Agent Execution Result: {result}");

        var history = new ChatHistory(
        """
        You are Gemini, a high-performance AI agent for Kushal (Senior Developer).

        RULES:
        - Never show tool JSON to the user
        - Use AITools plugin for all file operations
        - Always find real filenames before summarizing
        - Never hallucinate file paths
        - Be concise and action oriented
        """
        );

        Console.WriteLine("🚀 Gemini File Agent Ready");

        while (true)
        {
            Console.Write("\nYou: ");
            var userMessage = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userMessage))
                break;

            history.AddUserMessage(userMessage);

            ChatMessageContent response;

            do
            {
                response = await chatService.GetChatMessageContentAsync(
                    history,
                    executionSettings: settings,
                    kernel: kernel
                );

                history.Add(response);

            } while (response.Items?.Any(i => i is FunctionCallContent) == true);

            if (!string.IsNullOrWhiteSpace(response.Content))
            {
                Console.WriteLine($"\nGemini: {response.Content}");
            }
        }
    }
}