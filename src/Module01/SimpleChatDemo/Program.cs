using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

// =========================================================================================
// MODULE 01 - EDUCATIONAL QUICKSTART: MAF FLEXIBILITY (Microsoft.Extensions.AI)
// Practical demonstration of how the unified IChatClient abstraction eliminates Vendor Lock-in!
// =========================================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("==================================================================");
Console.WriteLine(" 🚀 MAF (Microsoft.Extensions.AI) — Multi-Provider Chat Quickstart ");
Console.WriteLine("==================================================================");
Console.WriteLine("See how a single C# function talks to multiple AI providers!\n");

// 1. Loads configuration from appsettings.json and appsettings.Development.json
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? "Production";

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var openAiKey = config["AiProviders:OpenAi:ApiKey"] ?? string.Empty;
var openAiModel = config["AiProviders:OpenAi:ModelId"] ?? "gpt-4o-mini";

var geminiKey = config["AiProviders:Gemini:ApiKey"] ?? string.Empty;
var geminiEndpoint = config["AiProviders:Gemini:Endpoint"] ?? "https://generativelanguage.googleapis.com/v1beta/openai/";
var geminiModel = config["AiProviders:Gemini:ModelId"] ?? "gemini-3.5-flash-lite";

var ollamaEndpoint = config["AiProviders:Ollama:Endpoint"] ?? "http://localhost:11434/v1";
var ollamaModel = config["AiProviders:Ollama:ModelId"] ?? "llama3.2:3b";

// 2. Provider Dictionary: notice that ALL are instantiated as 'IChatClient'
var providers = new Dictionary<string, Func<IChatClient>>()
{
    ["1. Local Ollama (OpenAI API /v1)"] = () =>
    {
        var options = new OpenAIClientOptions { Endpoint = new Uri(ollamaEndpoint) };
        return new OpenAIClient(new ApiKeyCredential("ollama-local"), options)
            .GetChatClient(ollamaModel)
            .AsIChatClient();
    },

    ["2. OpenAI Cloud (gpt-4o-mini)"] = () =>
    {
        if (string.IsNullOrWhiteSpace(openAiKey))
            throw new InvalidOperationException("OpenAI ApiKey is not configured in appsettings.");
        return new OpenAIClient(openAiKey)
            .GetChatClient(openAiModel)
            .AsIChatClient();
    },

    ["3. Google Gemini (via OpenAI Endpoint)"] = () =>
    {
        if (string.IsNullOrWhiteSpace(geminiKey))
            throw new InvalidOperationException("Gemini ApiKey is not configured in appsettings.");
        var options = new OpenAIClientOptions { Endpoint = new Uri(geminiEndpoint) };
        return new OpenAIClient(new ApiKeyCredential(geminiKey), options)
            .GetChatClient(geminiModel)
            .AsIChatClient();
    }
};

// 3. Simple Interactive Console Menu
Console.WriteLine("Choose a provider to test:");
var providerKeys = providers.Keys.ToList();
for (int i = 0; i < providerKeys.Count; i++)
{
    Console.WriteLine($"  [{i + 1}] {providerKeys[i]}");
}
Console.WriteLine($"  [{providerKeys.Count + 1}] Dispatch the same prompt to ALL providers in sequence!");
Console.WriteLine("  [0] Exit\n");

Console.Write("Enter your option: ");
var input = Console.ReadLine();

if (input == "0" || string.IsNullOrWhiteSpace(input))
{
    Console.WriteLine("Exiting demo.");
    return;
}

Console.Write("\nEnter your prompt for the AI (or press [Enter] for default prompt): ");
var userPrompt = Console.ReadLine();
if (string.IsNullOrWhiteSpace(userPrompt))
{
    userPrompt = "In just one sentence, what is the main benefit of using Microsoft.Extensions.AI in .NET?";
}

Console.WriteLine($"\nSent prompt: \"{userPrompt}\"");

if (int.TryParse(input, out int choice) && choice >= 1 && choice <= providerKeys.Count)
{
    var selectedName = providerKeys[choice - 1];
    await ExecuteChatAsync(selectedName, providers[selectedName], userPrompt);
}
else if (choice == providerKeys.Count + 1)
{
    foreach (var (name, factory) in providers)
    {
        await ExecuteChatAsync(name, factory, userPrompt);
    }
}
else
{
    Console.WriteLine("Invalid option.");
}

Console.WriteLine("\n==================================================================");
Console.WriteLine(" ✅ Demo completed!");
Console.WriteLine("==================================================================");


// =========================================================================================
// 🌟 THE MAGIC OF ABSTRACTION (MAF / MEAI):
// This function only receives the 'IChatClient' interface and is completely agnostic
// as to whether it is talking to OpenAI, Gemini, Ollama, or a test mock.
// =========================================================================================
static async Task ExecuteChatAsync(string providerName, Func<IChatClient> clientFactory, string prompt)
{
    Console.WriteLine($"\n------------------------------------------------------------");
    Console.WriteLine($" Connecting to provider: {providerName}...");
    Console.WriteLine($"------------------------------------------------------------");

    try
    {
        using var client = clientFactory();
        await SendStreamingChatAsync(client, prompt);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[Connection Warning]: {ex.Message}");
        Console.ResetColor();
    }
}

static async Task SendStreamingChatAsync(IChatClient client, string prompt)
{
    Console.Write("Response (Streaming): ");

    // You can customize how the AI responds by modifying various chatOptions parameters
    var chatOptions = new ChatOptions
    {
        Temperature = 0.1f
    };
    //chatOptions.TopK = 40;
    //chatOptions.TopP = 0.9f;
    //chatOptions.PresencePenalty = 1.1f;
    //chatOptions.FrequencyPenalty = 1.1f;
    //chatOptions.MaxOutputTokens = 1024;
    //chatOptions.Seed = 123;
    //chatOptions.ResponseFormat = ChatResponseFormat.Text;

    // Consumes tokens in real time via standardized MEAI asynchronous streaming
    await foreach (var update in client.GetStreamingResponseAsync(prompt, chatOptions))
    {
        Console.Write(update.Text);
    }
    Console.WriteLine();
}
