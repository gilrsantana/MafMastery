# Module 01: SimpleChatDemo — Educational Demo of MAF (Microsoft.Extensions.AI)

> Implemented in **.NET 10 (LTS)** and **C# 14**, based on the official **`Microsoft.Extensions.AI` (MEAI / MAF)** package.

---

## 🎯 Project Objective

> **Show developers how a single C# interface (`IChatClient`) and a single generic function can communicate with multiple AI providers (OpenAI, Google Gemini, Local Ollama, and Offline Simulated) without changing a single line of business code.**

---

## 💡 The Essence of MAF in a Few Lines

Historically, each provider required its own proprietary SDK with different namespaces, DTOs, and method signatures (Vendor Lock-in). With **MAF (`Microsoft.Extensions.AI`)**, all providers become an instance of **`IChatClient`**:

```csharp
// 1. Local Ollama Provider (via OpenAI REST API /v1)
IChatClient client1 = new OpenAIClient(new ApiKeyCredential("local"), new OpenAIClientOptions { Endpoint = new Uri("http://localhost:11434/v1") })
    .GetChatClient("llama3.2:3b").AsIChatClient();

// 2. OpenAI Cloud
IChatClient client2 = new OpenAIClient(apiKey).GetChatClient("gpt-4o-mini").AsIChatClient();

// 3. Google Gemini (via OpenAI Compatibility Gateway)
IChatClient client3 = new OpenAIClient(new ApiKeyCredential(geminiKey), new OpenAIClientOptions { Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/") })
    .GetChatClient("gemini-2.0-flash").AsIChatClient();
```

### The Generic Business Function:
```csharp
// Notice: this function has zero knowledge of which specific AI provider is behind the interface!
static async Task SendChatAsync(IChatClient client, string prompt)
{
    await foreach (var update in client.GetStreamingResponseAsync(prompt))
    {
        Console.Write(update.Text);
    }
}
```

---

## ⚡ How to Run

### 1. Immediate Execution (Zero-config or with environment keys)
```bash
dotnet run --project src/Module01/SimpleChatDemo
```

When executed, you will see the interactive menu:
```text
==================================================================
 🚀 MAF (Microsoft.Extensions.AI) — Multi-Provider Chat Quickstart 
==================================================================
See how a single C# function talks to multiple AI providers!

Choose a provider to test:
  [1] 1. Local Ollama (OpenAI API /v1)
  [2] 2. OpenAI Cloud (gpt-4o-mini)
  [3] 3. Google Gemini (via OpenAI Endpoint)
  [4] Dispatch the same prompt to ALL providers in sequence!
  [0] Exit

Enter your option: 1
```

You can:
- Select option `[1]` to test local Ollama (requires Ollama running locally).
- Select option `[2]` or `[3]` with keys configured in `appsettings.Development.json` or environment variables.
- Select option `[4]` to watch all configured providers respond to the same prompt one after another.

---

## ⚙️ Configuration (`appsettings.json` and `appsettings.Development.json`)

Configuration files are located in:
- `appsettings.json`: Safe base template file committed to the repository.
- `appsettings.Development.json`: Local file ignored by `.gitignore` containing development credentials:

```json
{
  "AiProviders": {
    "OpenAi": {
      "ApiKey": "your-openai-key",
      "ModelId": "gpt-4o-mini"
    },
    "Gemini": {
      "ApiKey": "your-gemini-key",
      "Endpoint": "https://generativelanguage.googleapis.com/v1beta/openai/",
      "ModelId": "gemini-2.0-flash"
    },
    "Ollama": {
      "Endpoint": "http://localhost:11434/v1",
      "ModelId": "llama3.2:3b"
    }
  }
}
```
