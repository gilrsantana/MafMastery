using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

// =========================================================================================
// MÓDULO 01 - QUICKSTART DIDÁTICO: FLEXIBILIDADE DO MAF (Microsoft.Extensions.AI)
// Demonstração prática de como a abstração unificada IChatClient elimina o Vendor Lock-in!
// =========================================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("==================================================================");
Console.WriteLine(" 🚀 MAF (Microsoft.Extensions.AI) — Chat Multi-Provider Quickstart ");
Console.WriteLine("==================================================================");
Console.WriteLine("Veja como uma única função C# conversa com múltiplos provedores de IA!\n");

// 1. Carrega configurações do appsettings.json e appsettings.Development.json
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? "Production";

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile(environment == "Development" ? "appsettings.Development.json" : "appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var openAiKey = config["AiProviders:OpenAi:ApiKey"] ?? string.Empty;
var openAiModel = config["AiProviders:OpenAi:ModelId"] ?? "gpt-4o-mini";

var geminiKey = config["AiProviders:Gemini:ApiKey"] ?? string.Empty;
var geminiEndpoint = config["AiProviders:Gemini:Endpoint"] ?? "https://generativelanguage.googleapis.com/v1beta/openai/";
var geminiModel = config["AiProviders:Gemini:ModelId"] ?? "gemini-3.5-flash-lite";

var ollamaEndpoint = config["AiProviders:Ollama:Endpoint"] ?? "http://localhost:11434/v1";
var ollamaModel = config["AiProviders:Ollama:ModelId"] ?? "llama3.2:3b";

// 2. Dicionário de Provedores: repare que TODOS são instanciados como 'IChatClient'
var providers = new Dictionary<string, Func<IChatClient>>()
{
    ["1. Ollama Local (API OpenAI /v1)"] = () =>
    {
        var options = new OpenAIClientOptions { Endpoint = new Uri(ollamaEndpoint) };
        return new OpenAIClient(new ApiKeyCredential("ollama-local"), options)
            .GetChatClient(ollamaModel)
            .AsIChatClient();
    },

    ["2. OpenAI Cloud (gpt-4o-mini)"] = () =>
    {
        if (string.IsNullOrWhiteSpace(openAiKey))
            throw new InvalidOperationException("ApiKey da OpenAI não configurada no appsettings.");
        return new OpenAIClient(openAiKey)
            .GetChatClient(openAiModel)
            .AsIChatClient();
    },

    ["3. Google Gemini (via OpenAI Endpoint)"] = () =>
    {
        if (string.IsNullOrWhiteSpace(geminiKey))
            throw new InvalidOperationException("ApiKey do Gemini não configurada no appsettings.");
        var options = new OpenAIClientOptions { Endpoint = new Uri(geminiEndpoint) };
        return new OpenAIClient(new ApiKeyCredential(geminiKey), options)
            .GetChatClient(geminiModel)
            .AsIChatClient();
    }
};

// 3. Menu Interativo simples no Console
Console.WriteLine("Escolha o provedor para testar:");
var providerKeys = providers.Keys.ToList();
for (int i = 0; i < providerKeys.Count; i++)
{
    Console.WriteLine($"  [{i + 1}] {providerKeys[i]}");
}
Console.WriteLine($"  [{providerKeys.Count + 1}] Disparar o mesmo prompt para TODOS os provedores em sequência!");
Console.WriteLine("  [0] Sair\n");

Console.Write("Digite sua opção: ");
var input = Console.ReadLine();

if (input == "0" || string.IsNullOrWhiteSpace(input))
{
    Console.WriteLine("Encerrando demonstração.");
    return;
}

Console.Write("\nDigite sua pergunta para a IA (ou pressione [Enter] para a pergunta padrão): ");
var userPrompt = Console.ReadLine();
if (string.IsNullOrWhiteSpace(userPrompt))
{
    userPrompt = "Em apenas uma frase, qual é a principal vantagem de usar Microsoft.Extensions.AI no .NET?";
}

Console.WriteLine($"\nPrompt enviado: \"{userPrompt}\"");

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
    Console.WriteLine("Opção inválida.");
}

Console.WriteLine("\n==================================================================");
Console.WriteLine(" ✅ Demonstração concluída!");
Console.WriteLine("==================================================================");


// =========================================================================================
// 🌟 A MÁGICA DA ABSTRAÇÃO (MAF / MEAI):
// Esta função recebe apenas a interface 'IChatClient' e desconhece completamente se
// está falando com OpenAI, Gemini, Ollama ou um mock de testes.
// =========================================================================================
static async Task ExecuteChatAsync(string providerName, Func<IChatClient> clientFactory, string prompt)
{
    Console.WriteLine($"\n------------------------------------------------------------");
    Console.WriteLine($" Conectando ao provedor: {providerName}...");
    Console.WriteLine($"------------------------------------------------------------");

    try
    {
        using var client = clientFactory();
        await SendStreamingChatAsync(client, prompt);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[Aviso de Conexão]: {ex.Message}");
        Console.ResetColor();
    }
}

static async Task SendStreamingChatAsync(IChatClient client, string prompt)
{
    Console.Write("Resposta (Streaming): ");


    // É possível modificar a forma como a IA responde alterando os diversos parâmetros de chatOptions
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

    // Consome tokens em tempo real via streaming assíncrono padronizado do MEAI
    await foreach (var update in client.GetStreamingResponseAsync(prompt, chatOptions))
    {
        Console.Write(update.Text);
    }
    Console.WriteLine();
}
