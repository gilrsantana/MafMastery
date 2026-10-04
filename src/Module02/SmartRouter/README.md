# 🌐 SmartRouter — Enterprise AI Gateway & Semantic Dynamic Router

> **Módulo 02 — Projeto Integrador**  
> Gateway inteligente de IA corporativa construído com **.NET 10 LTS**, **C# 14**, **Microsoft.Extensions.AI (MEAI 10.10.0)** e **Polly v8**.

---

## 🎯 Objetivo Arquitetural

O **SmartRouter** resolve os principais desafios de governança, FinOps e resiliência em sistemas modernos orientados a LLMs:

1. **Roteamento Dinâmico de Custo/Performance**:
   - Prompts concisos e triviais (< 1.000 caracteres) são direcionados ao tier **Economic** (ex.: *Ollama / OpenRouter / DeepSeek-Chat*).
   - Prompts extensos, complexos (>= 1.000 caracteres) ou com diretiva explícita (`"routeTierPreference": "Premium"`) são roteados ao tier **Premium** (ex.: *OpenAI / Azure OpenAI / Gemini*).
2. **Resiliência Semântica com Failover Transparente**:
   - Proteção com **Timeout Estratégico** (15s por padrão, configurável via `TimeoutSeconds`) e **Circuit Breaker** reativo via **Polly v8**.
   - Em caso de indisponibilidade, latência extrema ou circuito aberto no provedor primário (`EconomicProvider`), a chamada é desviada de forma transparente para o provedor secundário de alta disponibilidade (`PremiumProvider`).
3. **Observabilidade e Telemetria HTTP**:
   - Headers de resposta informam qual provedor atendeu a chamada (`X-SmartRouter-Target: EconomicProvider` ou `PremiumProvider`), se o failover foi aplicado (`X-SmartRouter-FallbackApplied: true/false`) e a latência de execução (`X-SmartRouter-Latency-Ms`).
4. **Streaming Reativo**:
   - Suporte completo a **Server-Sent Events (SSE)** via endpoint `POST /v1/chat/stream`, garantindo baixa latência na emissão de tokens.
5. **Diagnósticos e Health Probes**:
   - Endpoints dedicados para checagem de integridade do Gateway (`/health`), telemetria do Circuit Breaker (`/health/circuit`) e relatório de saúde dos provedores (`/health/providers`), com suporte a sondagem ativa em tempo real (`?live=true`).
6. **Zero-Config Local Run**:
   - Provedores simulados de alta fidelidade (`SimulatedProviderChatClient`) integrados permitem execução local e suíte de testes 100% determinística sem necessidade de cadastrar chaves pagas imediatas (`UseSimulatedClientsIfUnconfigured: true`).

---

## 🏛️ Estrutura Clean Architecture

```text
src/Module02/SmartRouter/
├── SmartRouter.slnx                       # Solution .NET 10
├── SmartRouter.http                       # Requisições HTTP interativas (VS Code / Rider / Visual Studio)
├── README.md                              # Este documento
├── src/
│   ├── SmartRouter.Domain/                # Regras de negócio puras, enums, modelos e contratos
│   │   ├── Enums/                         # RouteTier, ProviderKind
│   │   ├── Models/                        # RoutingDecision, ProviderHealthSnapshot, ProviderHealthStatus, RouterServiceAiKey
│   │   ├── Strategies/                    # IModelRouterStrategy
│   │   └── Services/                      # IProviderHealthTracker
│   │
│   ├── SmartRouter.Application/           # Casos de uso, orquestração e DTOs
│   │   ├── Common/                        # ISmartRouterService
│   │   ├── DTOs/                          # ChatMessageDto, ChatRequestDto, ChatResponseDto, CircuitStatusDto,
│   │   │                                  # ProviderHealthItemDto, ProvidersHealthReportDto
│   │   ├── UseCases/                      # SmartRouterService
│   │   └── Extensions/                    # ServiceCollectionExtensions
│   │
│   ├── SmartRouter.Infrastructure/        # Implementações de IA, Polly v8, Clientes e Health
│   │   ├── Configuration/                 # SmartRouterOptions, AiProviderConfig
│   │   ├── Health/                        # ProviderHealthTracker (Thread-safe)
│   │   ├── Strategies/                    # HeuristicModelRouterStrategy
│   │   ├── Resilience/                    # SmartRouterResilienceFactory (Polly v8)
│   │   ├── Mock/                          # SimulatedProviderChatClient (Dev/Test Offline)
│   │   ├── Clients/                       # SmartRoutingChatClient (DelegatingChatClient)
│   │   └── Extensions/                    # ServiceCollectionExtensions
│   │
│   └── SmartRouter.Gateway/               # Minimal APIs, SSE e Scalar/OpenAPI
│       ├── Endpoints/                     # ChatEndpoints, DiagnosticEndpoints
│       ├── appsettings.json               # Configurações do Gateway (ex.: Ollama + OpenAI)
│       ├── appsettings.Development.json   # Configurações de desenvolvimento (ex.: OpenRouter + Gemini)
│       └── Program.cs                     # Inicialização da Web API
│
└── tests/
    └── SmartRouter.Tests/                 # Testes unitários e de integração (21 testes)
        ├── Configuration/                 # SmartRouterOptionsValidationTests
        ├── Strategies/                    # HeuristicModelRouterStrategyTests
        ├── Clients/                       # SmartRoutingChatClientTests
        ├── Resilience/                    # CircuitBreakerFallbackTests
        └── Endpoints/                     # GatewayIntegrationTests (WebApplicationFactory)
```

---

## 🚀 Como Executar

### 1. Compilação da Solução

```bash
dotnet build src/Module02/SmartRouter/SmartRouter.slnx
```

### 2. Execução dos Testes Automatizados

```bash
dotnet test src/Module02/SmartRouter/SmartRouter.slnx
```

> **Resultado:** 21/21 testes executados com 100% de sucesso cobrindo validação de opções, roteamento heurístico, fallback resiliente, circuit breaker e requisições HTTP end-to-end com `WebApplicationFactory`.

### 3. Subir a API Gateway

```bash
dotnet run --project src/Module02/SmartRouter/src/SmartRouter.Gateway/SmartRouter.Gateway.csproj
```

A API estará disponível por padrão nas portas locais configuradas no `launchSettings.json` (ex.: `http://localhost:5178` ou `http://localhost:5000`).

### 4. Documentação Interativa com Scalar

Com a aplicação em execução no ambiente de desenvolvimento, acesse a interface moderna do **Scalar**:

- 🌐 **Scalar API Reference:** [`http://localhost:5178/scalar/v1`](http://localhost:5178/scalar/v1)
- 📄 **OpenAPI Spec (JSON):** [`http://localhost:5178/openapi/v1.json`](http://localhost:5178/openapi/v1.json)

---

## 📡 Endpoints e Exemplos de Chamadas (`curl`)

### 1. Prontidão e Liveness do Gateway

```bash
curl http://localhost:5178/health
```

**Exemplo de Resposta:**
```json
{
  "status": "Healthy",
  "gateway": "SmartRouter Enterprise Gateway",
  "version": "2.0.0-LTS",
  "timestampUtc": "2026-10-04T10:00:00Z"
}
```

---

### 2. Chat Tradicional (Monolítico com Roteamento Heurístico)

#### Prompt Curto (< 1.000 caracteres — Roteado para o Tier Econômico):
```bash
curl -X POST http://localhost:5178/v1/chat/completions \
  -H "Content-Type: application/json" \
  -i \
  -d '{
    "messages": [
      { "role": "user", "content": "Explique o que é Clean Architecture em duas linhas." }
    ]
  }'
```

**Exemplo de Resposta:**
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
X-SmartRouter-Target: EconomicProvider
X-SmartRouter-FallbackApplied: false
X-SmartRouter-Latency-Ms: 12

{
  "responseText": "[Simulated OpenRouter (deepseek/deepseek-chat)] Resposta gerada com sucesso para: 'Explique o que é Clean Architecture em duas linhas.'",
  "targetProvider": "EconomicProvider",
  "model": "deepseek/deepseek-chat",
  "fallbackApplied": false,
  "latencyMs": 12,
  "inputTokens": 18,
  "outputTokens": 32
}
```

---

### 3. Chat com Forçamento de Rota Premium

```bash
curl -X POST http://localhost:5178/v1/chat/completions \
  -H "Content-Type: application/json" \
  -i \
  -d '{
    "routeTierPreference": "Premium",
    "messages": [
      { "role": "user", "content": "Preciso de uma análise crítica profunda." }
    ]
  }'
```

**Headers Retornados:**
```http
X-SmartRouter-Target: PremiumProvider
X-SmartRouter-FallbackApplied: false
```

---

### 4. Streaming Reativo (Server-Sent Events)

```bash
curl -N -X POST http://localhost:5178/v1/chat/stream \
  -H "Content-Type: application/json" \
  -d '{
    "messages": [
      { "role": "user", "content": "Escreva um poema sobre resiliência de software." }
    ]
  }'
```

**Chunks Emitidos via SSE:**
```text
data: {"delta":"[Simulated OpenRouter] "}

data: {"delta":"Processando "}

data: {"delta":"sua requisição "}

data: {"delta":"de forma resiliente: "}

data: {"delta":"'Escreva um poema sobre resiliência de software.'."}

data: [DONE]
```

---

### 5. Telemetria do Circuit Breaker em Tempo Real

```bash
curl http://localhost:5178/health/circuit
```

**Exemplo de Resposta:**
```json
{
  "provider": "EconomicProvider",
  "circuitState": "Closed",
  "isOpen": false,
  "consecutiveFailures": 0,
  "totalSuccesses": 4,
  "totalFailures": 0,
  "lastFailureReason": null,
  "lastEvaluatedUtc": "2026-10-04T10:00:00Z"
}
```

---

### 6. Relatório de Saúde dos Provedores e Sondagem Ativa (*Live Probe*)

#### Consulta Telemetria dos Provedores:
```bash
curl http://localhost:5178/health/providers
```

#### Executa Sonda Ativa em Tempo Real (Ping nos Provedores com Medição de Latência):
```bash
curl "http://localhost:5178/health/providers?live=true"
```

**Exemplo de Resposta:**
```json
{
  "overallStatus": "Healthy",
  "timestampUtc": "2026-10-04T10:00:00Z",
  "providers": [
    {
      "provider": "EconomicProvider",
      "tier": "Economic",
      "circuitState": "Closed",
      "isCircuitOpen": false,
      "consecutiveFailures": 0,
      "totalSuccesses": 10,
      "totalFailures": 0,
      "lastFailureReason": null,
      "lastEvaluatedUtc": "2026-10-04T10:00:00Z",
      "probeLatencyMs": 15,
      "isAlive": true
    },
    {
      "provider": "PremiumProvider",
      "tier": "Premium",
      "circuitState": "Closed",
      "isCircuitOpen": false,
      "consecutiveFailures": 0,
      "totalSuccesses": 2,
      "totalFailures": 0,
      "lastFailureReason": null,
      "lastEvaluatedUtc": "2026-10-04T10:00:00Z",
      "probeLatencyMs": 28,
      "isAlive": true
    }
  ]
}
```

---

## ⚙️ Configuração dos Provedores (`AiProviderConfig`)

O SmartRouter utiliza o modelo universal `AiProviderConfig` em `SmartRouterOptions.Providers`. A configuração exige **exatamente dois provedores homologados**: um para o tier econômico (`PremiumTier: false`) e outro para o tier premium (`PremiumTier: true`).

### Configuração via `appsettings.json`

```json
{
  "SmartRouter": {
    "Providers": [
      {
        "AiProviderName": "Ollama",
        "Endpoint": "http://192.168.1.100:11434/v1",
        "ApiKey": "ollama",
        "DefaultModel": "llama3.1:8b-instruct",
        "HttpReferer": "https://mafmastery.local",
        "AppTitle": "MafMastery-SmartRouter",
        "PremiumTier": false
      },
      {
        "AiProviderName": "OpenAi",
        "Endpoint": "https://api.openai.com/v1",
        "ApiKey": "sk-proj-sua-chave-openai",
        "DefaultModel": "gpt-4o-mini",
        "HttpReferer": "https://mafmastery.local",
        "AppTitle": "MafMastery-SmartRouter",
        "PremiumTier": true
      }
    ],
    "CharacterThresholdForPremiumTier": 1000,
    "CircuitBreakerFailureRatio": 0.5,
    "CircuitBreakerSamplingDurationSeconds": 30,
    "CircuitBreakerBreakDurationSeconds": 30,
    "CircuitBreakerMinimumThroughput": 5,
    "TimeoutSeconds": 15,
    "UseSimulatedClientsIfUnconfigured": true
  }
}
```

### Configuração via Variáveis de Ambiente

Para ambientes de produção ou contêineres Docker/Kubernetes, utilize o formato hierárquico padrão do ASP.NET Core:

```bash
export SmartRouter__UseSimulatedClientsIfUnconfigured=false
export SmartRouter__TimeoutSeconds=15

# Provedor Econômico (Índice 0) - Ex.: OpenRouter
export SmartRouter__Providers__0__AiProviderName="OpenRouter"
export SmartRouter__Providers__0__Endpoint="https://openrouter.ai/api/v1"
export SmartRouter__Providers__0__ApiKey="sk-or-v1-sua-chave"
export SmartRouter__Providers__0__DefaultModel="deepseek/deepseek-chat"
export SmartRouter__Providers__0__PremiumTier=false

# Provedor Premium (Índice 1) - Ex.: OpenAI ou Azure OpenAI
export SmartRouter__Providers__1__AiProviderName="OpenAi"
export SmartRouter__Providers__1__Endpoint="https://api.openai.com/v1"
export SmartRouter__Providers__1__ApiKey="sk-proj-sua-chave"
export SmartRouter__Providers__1__DefaultModel="gpt-4o-mini"
export SmartRouter__Providers__1__PremiumTier=true
```

---

## 🧪 Testes Interativos (`SmartRouter.http`)

O repositório inclui a suíte completa de requisições interativas pronta para execução via extensão **REST Client** (VS Code) ou cliente HTTP integrado (Rider / Visual Studio) no arquivo:

📄 [`src/Module02/SmartRouter/SmartRouter.http`](SmartRouter.http)
