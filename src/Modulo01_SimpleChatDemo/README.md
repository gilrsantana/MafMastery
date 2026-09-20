# Módulo 01: SimpleChatDemo — Demonstração Didática do MAF (Microsoft.Extensions.AI)

> Implementado em **.NET 10 (LTS)** e **C# 14**, baseado no pacote oficial **`Microsoft.Extensions.AI` (MEAI / MAF)**.

---

## 🎯 Objetivo Deste Projeto

> **Mostrar ao desenvolvedor como uma única interface C# (`IChatClient`) e uma única função genérica conseguem conversar com múltiplos provedores de IA (OpenAI, Google Gemini, Ollama Local e Simulado Offline) sem alterar uma única linha de código de negócio.**

---

## 💡 A Essência do MAF em Poucas Linhas

Historicamente, cada provedor exigia seu próprio SDK proprietário com namespaces, DTOs e métodos diferentes (Vendor Lock-in). Com o **MAF (`Microsoft.Extensions.AI`)**, todos os provedores se tornam uma instância de **`IChatClient`**:

```csharp
// 1. Provedor Local Ollama (via API REST OpenAI /v1)
IChatClient client1 = new OpenAIClient(new ApiKeyCredential("local"), new OpenAIClientOptions { Endpoint = new Uri("http://localhost:11434/v1") })
    .GetChatClient("llama3.2:3b").AsIChatClient();

// 2. OpenAI Cloud
IChatClient client2 = new OpenAIClient(apiKey).GetChatClient("gpt-4o-mini").AsIChatClient();

// 3. Google Gemini (via OpenAI Compatibility Gateway)
IChatClient client3 = new OpenAIClient(new ApiKeyCredential(geminiKey), new OpenAIClientOptions { Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/") })
    .GetChatClient("gemini-2.0-flash").AsIChatClient();
```

### A Função de Negócio Genérica:
```csharp
// Repare: esta função desconhece completamente quem é o provedor de IA!
static async Task SendChatAsync(IChatClient client, string prompt)
{
    await foreach (var update in client.GetStreamingResponseAsync(prompt))
    {
        Console.Write(update.Text);
    }
}
```

---

## ⚡ Como Executar

### 1. Execução Imediata (Sem configurações ou com chaves do ambiente)
```bash
dotnet run --project src/Modulo01_SimpleChatDemo
```

Ao executar, você verá o menu interativo:
```text
==================================================================
 🚀 MAF (Microsoft.Extensions.AI) — Chat Multi-Provider Quickstart 
==================================================================
Veja como uma única função C# conversa com múltiplos provedores de IA!

Escolha o provedor para testar:
  [1] 1. Simulado (Offline / Zero Config)
  [2] 2. Ollama Local (API OpenAI /v1)
  [3] 3. OpenAI Cloud (gpt-4o-mini)
  [4] 4. Google Gemini (via OpenAI Endpoint)
  [5] Disparar o mesmo prompt para TODOS os provedores em sequência!
  [0] Sair

Digite sua opção: 1
```

Você pode:
- Testar a opção `[1]` para ver a resposta imediata via streaming mesmo sem internet ou sem chaves de API.
- Testar a opção `[3]` ou `[4]` com as chaves configuradas em `appsettings.Development.json`.
- Testar a opção `[5]` para ver todos os provedores respondendo ao mesmo prompt um após o outro.

---

## ⚙️ Configurações (`appsettings.json` e `appsettings.Development.json`)

As configurações residem em:
- `appsettings.json`: Arquivo base de template seguro para commit no repositório.
- `appsettings.Development.json`: Arquivo local ignorado pelo `.gitignore` contendo as credenciais de desenvolvimento:

```json
{
  "AiProviders": {
    "OpenAi": {
      "ApiKey": "sua-chave-openai",
      "ModelId": "gpt-4o-mini"
    },
    "Gemini": {
      "ApiKey": "sua-chave-gemini",
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
