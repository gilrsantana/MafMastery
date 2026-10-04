# 🌐 SmartRouter — Enterprise AI Gateway & Semantic Dynamic Router

> **Module 02 — Capstone Project**  
> Enterprise AI smart gateway built with **.NET 10 LTS**, **C# 14**, **Microsoft.Extensions.AI (MEAI 10.10.0)**, and **Polly v8**.

---

## 🎯 Architectural Objective

**SmartRouter** addresses key governance, FinOps, and resilience challenges in modern LLM-driven systems:

1. **Dynamic Cost/Performance Routing**:
   - Concise, trivial prompts (< 1,000 characters) are routed to the **Economic** tier (e.g., *Ollama / OpenRouter / DeepSeek-Chat*).
   - Long, complex prompts (>= 1,000 characters) or explicit directives (`"routeTierPreference": "Premium"`) are routed to the **Premium** tier (e.g., *OpenAI / Azure OpenAI / Gemini*).
2. **Semantic Resilience with Transparent Failover**:
   - Protected by a **Strategic Timeout** (15s default, configurable via `TimeoutSeconds`) and reactive **Circuit Breaker** powered by **Polly v8**.
   - Upon outages, extreme latency, or an open circuit on the primary provider (`EconomicProvider`), calls are transparently rerouted to the high-availability secondary provider (`PremiumProvider`).
3. **Observability and HTTP Telemetry**:
   - Response headers detail which provider fulfilled the request (`X-SmartRouter-Target: EconomicProvider` or `PremiumProvider`), whether failover was triggered (`X-SmartRouter-FallbackApplied: true/false`), and execution latency (`X-SmartRouter-Latency-Ms`).
4. **Reactive Streaming**:
   - Full support for **Server-Sent Events (SSE)** via the `POST /v1/chat/stream` endpoint, providing low-latency token streaming.
5. **Diagnostics and Health Probes**:
   - Dedicated endpoints for gateway health (`/health`), Circuit Breaker telemetry (`/health/circuit`), and provider health reports (`/health/providers`), with support for active real-time probing (`?live=true`).
6. **Zero-Config Local Run**:
   - Built-in high-fidelity simulated providers (`SimulatedProviderChatClient`) enable deterministic local execution and a 100% reliable test suite without requiring immediate paid API keys (`UseSimulatedClientsIfUnconfigured: true`).

---

## 🏛️ Clean Architecture Structure

```text
src/Module02/SmartRouter/
├── SmartRouter.slnx                       # .NET 10 Solution
├── SmartRouter.http                       # Interactive HTTP requests (VS Code / Rider / Visual Studio)
├── README.md                              # This document
├── src/
│   ├── SmartRouter.Domain/                # Pure business rules, enums, models, and contracts
│   │   ├── Enums/                         # RouteTier, ProviderKind
│   │   ├── Models/                        # RoutingDecision, ProviderHealthSnapshot, ProviderHealthStatus, RouterServiceAiKey
│   │   ├── Strategies/                    # IModelRouterStrategy
│   │   └── Services/                      # IProviderHealthTracker
│   │
│   ├── SmartRouter.Application/           # Use cases, orchestration, and DTOs
│   │   ├── Common/                        # ISmartRouterService
│   │   ├── DTOs/                          # ChatMessageDto, ChatRequestDto, ChatResponseDto, CircuitStatusDto,
│   │   │                                  # ProviderHealthItemDto, ProvidersHealthReportDto
│   │   ├── UseCases/                      # SmartRouterService
│   │   └── Extensions/                    # ServiceCollectionExtensions
│   │
│   ├── SmartRouter.Infrastructure/        # AI implementations, Polly v8, Clients, and Health
│   │   ├── Configuration/                 # SmartRouterOptions, AiProviderConfig
│   │   ├── Health/                        # ProviderHealthTracker (Thread-safe)
│   │   ├── Strategies/                    # HeuristicModelRouterStrategy
│   │   ├── Resilience/                    # SmartRouterResilienceFactory (Polly v8)
│   │   ├── Mock/                          # SimulatedProviderChatClient (Dev/Test Offline)
│   │   ├── Clients/                       # SmartRoutingChatClient (DelegatingChatClient)
│   │   └── Extensions/                    # ServiceCollectionExtensions
│   │
│   └── SmartRouter.Gateway/               # Minimal APIs, SSE, and Scalar/OpenAPI
│       ├── Endpoints/                     # ChatEndpoints, DiagnosticEndpoints
│       ├── appsettings.json               # Gateway Configuration (e.g., Ollama + OpenAI)
│       ├── appsettings.Development.json   # Development Configuration (e.g., OpenRouter + Gemini)
│       └── Program.cs                     # Web API Host Bootstrap
│
└── tests/
    └── SmartRouter.Tests/                 # Unit and integration tests (21 tests)
        ├── Configuration/                 # SmartRouterOptionsValidationTests
        ├── Strategies/                    # HeuristicModelRouterStrategyTests
        ├── Clients/                       # SmartRoutingChatClientTests
        ├── Resilience/                    # CircuitBreakerFallbackTests
        └── Endpoints/                     # GatewayIntegrationTests (WebApplicationFactory)
```

---

## 🚀 How to Run

### 1. Build the Solution

```bash
dotnet build src/Module02/SmartRouter/SmartRouter.slnx
```

### 2. Run Automated Tests

```bash
dotnet test src/Module02/SmartRouter/SmartRouter.slnx
```

> **Result:** 21/21 tests pass with 100% success covering options validation, heuristic routing, resilient fallback, circuit breaker, and end-to-end HTTP requests via `WebApplicationFactory`.

### 3. Launch Gateway API

```bash
dotnet run --project src/Module02/SmartRouter/src/SmartRouter.Gateway/SmartRouter.Gateway.csproj
```

The API will be available on the default local ports configured in `launchSettings.json` (e.g., `http://localhost:5178` or `http://localhost:5000`).

### 4. Interactive Documentation with Scalar

When running in the Development environment, open the modern **Scalar** interface:

- 🌐 **Scalar API Reference:** [`http://localhost:5178/scalar/v1`](http://localhost:5178/scalar/v1)
- 📄 **OpenAPI Spec (JSON):** [`http://localhost:5178/openapi/v1.json`](http://localhost:5178/openapi/v1.json)

---

## 📡 Endpoints and Call Examples (`curl`)

### 1. Gateway Readiness and Liveness

```bash
curl http://localhost:5178/health
```

**Sample Response:**
```json
{
  "status": "Healthy",
  "gateway": "SmartRouter Enterprise Gateway",
  "version": "2.0.0-LTS",
  "timestampUtc": "2026-10-04T10:00:00Z"
}
```

---

### 2. Standard Chat (Monolithic with Heuristic Routing)

#### Short Prompt (< 1,000 characters — Routed to Economy Tier):
```bash
curl -X POST http://localhost:5178/v1/chat/completions \
  -H "Content-Type: application/json" \
  -i \
  -d '{
    "messages": [
      { "role": "user", "content": "Explain Clean Architecture in two sentences." }
    ]
  }'
```

**Sample Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
X-SmartRouter-Target: EconomicProvider
X-SmartRouter-FallbackApplied: false
X-SmartRouter-Latency-Ms: 12

{
  "responseText": "[Simulated OpenRouter (deepseek/deepseek-chat)] Response successfully generated for: 'Explain Clean Architecture in two sentences.'",
  "targetProvider": "EconomicProvider",
  "model": "deepseek/deepseek-chat",
  "fallbackApplied": false,
  "latencyMs": 12,
  "inputTokens": 18,
  "outputTokens": 32
}
```

---

### 3. Chat with Explicit Premium Tier Directive

```bash
curl -X POST http://localhost:5178/v1/chat/completions \
  -H "Content-Type: application/json" \
  -i \
  -d '{
    "routeTierPreference": "Premium",
    "messages": [
      { "role": "user", "content": "I need an in-depth critical analysis." }
    ]
  }'
```

**Returned Headers:**
```http
X-SmartRouter-Target: PremiumProvider
X-SmartRouter-FallbackApplied: false
```

---

### 4. Reactive Streaming (Server-Sent Events)

```bash
curl -N -X POST http://localhost:5178/v1/chat/stream \
  -H "Content-Type: application/json" \
  -d '{
    "messages": [
      { "role": "user", "content": "Write a poem about software resilience." }
    ]
  }'
```

**Chunks Emitted via SSE:**
```text
data: {"delta":"[Simulated OpenRouter] "}

data: {"delta":"Processing "}

data: {"delta":"your request "}

data: {"delta":"resiliently: "}

data: {"delta":"'Write a poem about software resilience.'."}

data: [DONE]
```

---

### 5. Real-Time Circuit Breaker Telemetry

```bash
curl http://localhost:5178/health/circuit
```

**Sample Response:**
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

### 6. Provider Health Report and Active Probing (*Live Probe*)

#### Query Provider Telemetry:
```bash
curl http://localhost:5178/health/providers
```

#### Execute Real-Time Active Probe (Ping Providers with Latency Measurement):
```bash
curl "http://localhost:5178/health/providers?live=true"
```

**Sample Response:**
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

## ⚙️ Provider Configuration (`AiProviderConfig`)

SmartRouter uses the universal `AiProviderConfig` model in `SmartRouterOptions.Providers`. Configuration requires **exactly two configured providers**: one for the economic tier (`PremiumTier: false`) and one for the premium tier (`PremiumTier: true`).

### Configuration via `appsettings.json`

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
        "ApiKey": "sk-proj-your-openai-key",
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

### Configuration via Environment Variables

For production environments or Docker/Kubernetes containers, use standard ASP.NET Core hierarchical environment variables:

```bash
export SmartRouter__UseSimulatedClientsIfUnconfigured=false
export SmartRouter__TimeoutSeconds=15

# Economic Provider (Index 0) - e.g., OpenRouter
export SmartRouter__Providers__0__AiProviderName="OpenRouter"
export SmartRouter__Providers__0__Endpoint="https://openrouter.ai/api/v1"
export SmartRouter__Providers__0__ApiKey="sk-or-v1-your-key"
export SmartRouter__Providers__0__DefaultModel="deepseek/deepseek-chat"
export SmartRouter__Providers__0__PremiumTier=false

# Premium Provider (Index 1) - e.g., OpenAI or Azure OpenAI
export SmartRouter__Providers__1__AiProviderName="OpenAi"
export SmartRouter__Providers__1__Endpoint="https://api.openai.com/v1"
export SmartRouter__Providers__1__ApiKey="sk-proj-your-key"
export SmartRouter__Providers__1__DefaultModel="gpt-4o-mini"
export SmartRouter__Providers__1__PremiumTier=true
```

---

## 🧪 Interactive Testing (`SmartRouter.http`)

The repository includes a complete interactive request collection ready to run via the **REST Client** extension (VS Code) or integrated HTTP client (Rider / Visual Studio) in:

📄 [`src/Module02/SmartRouter/SmartRouter.http`](SmartRouter.http)
