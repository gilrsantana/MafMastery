# Module 01: AiProviderBenchmarker — AI Provider Benchmarker

> **Capstone Project for Module 01** of the **MafMastery** program.  
> Implemented in **.NET 10 (LTS)** and **C# 14**, based on the official **`Microsoft.Extensions.AI` (MEAI)** library, **Clean Architecture**, **Twelve-Factor App & Agent** principles, and a rich console interface powered by **Spectre.Console**.

---

## 🎯 Project Objective

The **AiProviderBenchmarker** is an enterprise-grade command-line interface (CLI) tool designed to simultaneously submit the same prompt to multiple Large Language Model (LLM) providers — including commercial cloud providers (OpenAI, Azure OpenAI), local models (Ollama), and offline simulated engines —, rigorously measuring and comparing:

1. **TTFT (Time To First Token)**: Elapsed time until the arrival of the very first token via streaming (a key metric for perceived response speed).
2. **Total Latency**: End-to-end duration of the completion generation.
3. **Generation Throughput (TPS)**: Real token generation rate (output tokens per second), excluding TTFT from the equation to isolate the inference engine's actual throughput.
4. **FinOps Cost Estimation ($ USD)**: Financial inference cost calculation based on input/output token consumption against public pricing tables per million tokens.

---

## 🏛️ Solution Architecture

The solution strictly adheres to **Clean Architecture** principles, maintaining a clean separation of concerns:

```
src/Module01/AiProviderBenchmarker/
├── AiProviderBenchmarker.slnx              # Modern .NET 10 solution file
├── src/
│   ├── AiProviderBenchmarker.Domain/        # Domain Layer: pure business rules, zero I/O or frameworks
│   │   ├── Model/
│   │   │   ├── ProviderType.cs              # Enum: OpenAi, AzureOpenAi, Ollama, Simulated
│   │   │   ├── ModelPricing.cs              # FinOps Value Object (cost per 1M tokens)
│   │   │   └── ProviderMetric.cs            # Result Entity/DTO with TPS calculation
│   │   └── Services/
│   │       └── ICostEstimator.cs            # Contract for inference cost estimation
│   │
│   ├── AiProviderBenchmarker.Application/   # Use Cases and Orchestration
│   │   ├── Common/
│   │   │   └── IChatClientFactory.cs        # Factory contract for IChatClient abstraction
│   │   └── UseCases/
│   │       ├── RunBenchmarkCommand.cs       # Benchmark input command DTO
│   │       ├── IRunBenchmarkUseCase.cs      # Use case contract
│   │       └── RunBenchmarkHandler.cs       # Parallel execution (Task.WhenAll), TTFT & resilience
│   │
│   ├── AiProviderBenchmarker.Infrastructure/ # External AI Adapters and Pricing
│   │   ├── Configuration/
│   │   │   ├── AiProviderConfig.cs      # Extensible base model with AiProviderName
│   │   │   └── AiProvidersOptions.cs    # Provider collection (List<AiProviderConfig>)
│   │   ├── Mock/
│   │   │   └── SimulatedChatClient.cs       # Deterministic, offline IChatClient implementation
│   │   ├── Factories/
│   │   │   ├── Strategies/                  # Strategy Pattern (SOLID/OCP) for client instantiation
│   │   │   │   ├── IChatClientStrategy.cs   # Strategy interface
│   │   │   │   ├── SimulatedClientStrategy.cs # Strategy for simulated engine and fallback
│   │   │   │   ├── AzureOpenAiClientStrategy.cs # Strategy for Azure OpenAI
│   │   │   │   └── OpenAiCompatibleClientStrategy.cs # Strategy for OpenAI / Ollama / Gemini / Grok
│   │   │   └── ChatClientFactory.cs         # Decoupled orchestrating factory (Strategy Pattern)
│   │   └── Pricing/
│   │       └── CostEstimator.cs             # Dynamic cost estimator based on appsettings.json
│   │
│   └── AiProviderBenchmarker.Cli/           # Presentation Layer (Console)
│       ├── appsettings.json                 # Model, endpoint, and credential configuration
│       ├── Configuration/
│       │   └── ServiceCollectionExtensions.cs # Dependency Injection (IoC) and Configuration
│       ├── Arguments/
│       │   ├── CliParsedOptions.cs          # Model with parsed command-line options
│       │   └── CliArgumentParser.cs         # Isolated parser for flags (-p, --providers, -m, -h)
│       ├── Runners/
│       │   ├── IBenchmarkRunner.cs          # Benchmark runner contract
│       │   ├── InteractiveBenchmarkRunner.cs # Guided interactive workflow (Spectre.Console)
│       │   ├── NonInteractiveBenchmarkRunner.cs # Autonomous workflow for CI/CD
│       │   └── CliApp.cs                    # High-level application orchestrator
│       ├── UI/
│       │   ├── TableRenderer.cs             # Renders tables, trees, podium cards, and status
│       │   └── CliHelpRenderer.cs           # Help screen renderer (--help)
│       └── Program.cs                       # Minimalist entrypoint (< 35 lines)
│
└── tests/
    └── AiProviderBenchmarker.Tests/         # Automated tests (xUnit, FluentAssertions, NSubstitute)
        ├── Domain/                          # Unit tests for metric formulas (TPS)
        ├── Infrastructure/                  # Tests for cost catalog and pricing
        └── Application/                     # Concurrency and fault-tolerance tests
```

---

## ⚡ How to Run

### ⚠️ Important Rule for `dotnet run` Arguments

When running a console application with `dotnet run`, **you must supply the `--` delimiter before passing arguments to your application**.  
Otherwise, the `dotnet run` host utility will intercept arguments such as `--help`, `-p`, or `--providers` as if they were flags for the .NET SDK itself!

```bash
# ❌ INCORRECT (dotnet run intercepts the flag and displays .NET SDK help):
dotnet run --project src/Module01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli --help

# ✔️ CORRECT (arguments after '--' are forwarded directly to AiProviderBenchmarker):
dotnet run --project src/Module01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- --help
```

---

### 1. Interactive Mode (Menus and Selectors)

If no arguments are provided and the terminal is an interactive TTY, the application starts an interactive wizard powered by `Spectre.Console`:

```bash
dotnet run --project src/Module01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli
```

In interactive mode you can:
- Choose from predefined test prompts (C# .NET 10 Programming, Architectural Analysis, Technical Translation) or type a custom prompt.
- Use the spacebar to select which providers to benchmark in the current round.
- Set the desired output `max_tokens`.
- Run consecutive rounds without leaving the application.

---

### 2. Non-Interactive / CLI Mode (CI/CD or Automation)

Ideal for scripting, CI pipelines, or quick comparisons:

```bash
dotnet run --project src/Module01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- [arguments]
```

#### CLI Arguments Table:

| Argument | Short Flag | Description | Default Value | Example |
| :--- | :--- | :--- | :--- | :--- |
| `--prompt <text>` | `-p` | Prompt text to send to models | Predefined C# 14 question | `-p "Explain Clean Architecture"` |
| `--providers <list>` | *(none)* | Comma-separated list of providers | `Simulated` (+ active providers) | `--providers Ollama,Simulated` |
| `--max-tokens <num>` | `-m` | Maximum output token limit | `300` | `-m 200` |
| `--help` | `-h` | Display the application help screen | *(none)* | `--help` |

#### Supported Providers in the `--providers` Flag:
- `Simulated`: Built-in offline engine (high-fidelity deterministic mock; requires no internet connection or API keys).
- `Ollama`: Local inference engine via OpenAI-compatible API (e.g., `http://localhost:11434/v1` running models such as `llama3.2:3b`, `phi4`).
- `OpenAi`: OpenAI API (uses the API key configured in `appsettings.json` or environment variable).
- `AzureOpenAi`: Azure OpenAI Service resource (uses endpoint and deployment key).
- `Gemini`: Google Gemini via OpenAI-compatible endpoint (`https://generativelanguage.googleapis.com/v1beta/openai/`).
- `Grok`: xAI Grok via OpenAI-compatible endpoint (`https://api.x.ai/v1`).
- *Any other provider*: Any provider configured in the `Providers` list of `appsettings.json` with an `AiProviderName` can be referenced directly by name!

---

### Practical Execution Examples

#### Example A: Test Local and Offline Providers (Ollama and Simulated)
```bash
dotnet run --project src/Module01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- --providers Ollama,Simulated --max-tokens 200
```

#### Example B: Run with a Custom Prompt
```bash
dotnet run --project src/Module01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- -p "Write an example of the Factory pattern in C# 14 with pattern matching." --providers Simulated,OpenAi -m 250
```

#### Example C: Display CLI Help
```bash
dotnet run --project src/Module01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- --help
```

---

## ⚙️ Provider Configuration (`appsettings.json`)

Connection and model settings are located in `src/AiProviderBenchmarker.Cli/appsettings.json` and can be overridden via Environment Variables:

```json
{
  "AiProviders": {
    "Providers": [
      {
        "AiProviderName": "OpenAi",
        "Endpoint": "https://api.openai.com/v1",
        "ApiKey": "",
        "DeploymentName": "gpt-4o-mini",
        "InputPricePerMillion": 0.15,
        "OutputPricePerMillion": 0.60,
        "Enabled": true
      },
       ...
      {
        "AiProviderName": "Simulated",
        "DeploymentName": "simulated-fast-llm",
        "InputPricePerMillion": 0.10,
        "OutputPricePerMillion": 0.40,
        "Enabled": true,
        "MinTtftMs": 160,
        "MaxTtftMs": 350,
        "TokensPerSecond": 50
      }
    ]
  }
}
```

> 💡 **Resilience Note**: If an AI Provider key is empty or not configured, the application transparently activates the **Simulated Fallback**, allowing you to run and demo the tool without unexpected errors.

---

## 📊 Understanding Benchmark Report Metrics

At the end of each round, the application produces a consolidated table with the following columns:

| Metric | Meaning and Calculation |
| :--- | :--- |
| **Status** | Shows `✔ OK` for successful runs or `✖ Failed` if the provider returned an error (timeout, missing credentials, etc.). |
| **TTFT** | **Time To First Token**: Measured from request dispatch until the arrival of the first streaming chunk (`update.Text`). |
| **Latency** | Total end-to-end duration of the invocation. |
| **Tokens I/O** | Estimated Input Tokens (prompt) and Output Tokens (completion). |
| **TPS (Tokens/s)** | Real generation throughput: $$\text{TPS} = \frac{\text{Output Tokens}}{\text{Total Latency} - \text{TTFT}}$$ |
| **Cost ($ USD)** | Monetary estimate based on the official rate per 1 million tokens. Local providers such as **Ollama** always report `$0.000000`. |

Below the table, the CLI displays:
- **Response Preview**: Tree view displaying the first characters of each model's synthesized response.
- **Benchmark Podium**: Highlight cards displaying the lowest TTFT winner (fastest start), the highest Throughput winner (fastest generation rate), and the most economical option (FinOps champion).

---

## 🧪 Automated Test Suite

The unit test suite was built with **xUnit**, **FluentAssertions**, and **NSubstitute**:

```bash
# Run all unit tests in the solution
dotnet test src/Module01/AiProviderBenchmarker/AiProviderBenchmarker.slnx
```

### What the Tests Cover:
1. **`ProviderMetricTests`**:
   - Validates accurate TPS calculation excluding TTFT from total latency.
   - Handles edge cases such as zero tokens or TTFT exceeding total time without division by zero.
2. **`CostEstimatorTests`**:
   - Validates weighted cost estimations for OpenAI models (`gpt-4o`, `gpt-4o-mini`, etc.).
   - Asserts that local **Ollama** inferences always evaluate to zero cost ($0.00).
   - Validates fallback cost calculation for uncataloged custom models.
3. **`RunBenchmarkHandlerTests`**:
   - Tests simultaneous multi-provider orchestration using `Task.WhenAll`.
   - Validates **fault isolation**: if a provider throws an `HttpRequestException` or times out, it is flagged as an individual failure while remaining providers continue execution and yield intact success metrics.
   - Tests real-time streaming progress notifications via `IProgress<T>`.
