using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Semantic_App_Using_LLAMA;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();

        // 1. Setup HttpClient & Services
        builder.Services.AddHttpClient("OllamaClient", client => {
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        // Register Tools with clear name
        builder.Plugins.AddFromType<AITools>("AITools");

        // Use Llama 3.1 or 3.2 (Important to avoid 400 Error)
        builder.AddOpenAIChatCompletion(
            modelId: "llama3.1",
            apiKey: "none",
            endpoint: new Uri("http://localhost:11434/v1")
        );

        var kernel = builder.Build();

        // --- MANUALLY TRIGGER FIRST (The part I missed) ---
        Console.WriteLine("--- Phase 1: Manual Tool Invocation ---");

        var answer = await kernel.InvokeAsync("AITools", "GetFileCount",
            new() { ["path"] = @"C:\Users\kusha\Downloads" });
        Console.WriteLine($"Direct Tool Answer (File Count): {answer}");

        // --- AGENT PROMPT TRIGGER ---
        Console.WriteLine("\n--- Phase 2: Agent Intent Execution ---");

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0
        };

        string prompt = "Tell me the current system time then Check file count in C:\\Users\\kusha\\Downloads and create 'Specpoint_Verified' folder there.";

        // Passing kernel here is MUST for auto-invocation
        var result = await kernel.InvokePromptAsync(prompt, new(settings));
        Console.WriteLine($"Agent Execution Result: {result}");

        // --- CONTINUOUS CHAT LOOP ---
        Console.WriteLine("\n--- Phase 3: Persistent Chat Agent ---");

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory(@"You are a cautious system agent. . Execute tools for file tasks. If not a file task, just chat.
        1. For organizing files, ALWAYS call 'GetOrganizationPreview' first.
        2. DO NOT call 'ExecuteMove' until the user explicitly confirms with 'Yes' or 'Proceed'.
        3. Be professional and wait for human confirmation for safety.");
        while (true)
        {
            Console.Write("\nYou: ");
            var userMessage = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userMessage)) break;

            history.AddUserMessage(userMessage);

            // 
            var response = await chatService.GetChatMessageContentAsync(
                history,
                executionSettings: settings,
                kernel: kernel); // Auto-invokes tools if AI decides to

            if (response.Content != null)
            {
                Console.WriteLine($"\nBot: {response.Content}");
                history.AddAssistantMessage(response.Content);
            }
        }
    }
}