# MafMastery: AI Engineering Learning Track with .NET & C#

> **Building Enterprise, Resilient, and Scalable Artificial Intelligence Systems Through a Software Engineering Lens.**

---

## 0. Introduction: The Role of Software Engineering in AI with .NET

### 0.1 Context and Curriculum Rationale
The contemporary Generative Artificial Intelligence ecosystem is largely dominated by libraries, tutorials, and materials tailored for Python and JavaScript/TypeScript. While those languages excel at rapid prototyping, scientific experimentation, and notebook scripting, the reality of **large-scale enterprise production engineering** introduces substantially different engineering challenges:

- **Tight coupling**: scripts that interweave direct AI API calls with core business logic.
- **Lack of Type Safety**: fragile schema validation and runtime failures on LLM responses due to lack of compile-time static typing.
- **Operational fragility**: missing robust resilience policies (retries, fallbacks, circuit breakers) and decoupled dependency injection.
- **Naive RAG**: oversimplified implementations relying on arbitrary chunking and cosine vector searches that fall short in complex enterprise domains.
- **Unstructured prose instructions**: monolithic free-text prompts causing high rates of constraint violations (*Rule Violations*) and runaway token consumption.
- **Absence of Execution Contracts**: lack of a formal boundary separating the execution runtime from agent business invariants and acceptance criteria.
- **Governance and security deficits**: pipelines lacking auditable data trails, standardized telemetry, and protection against emerging AI-specific threats (OWASP Top 10 for LLMs).

The **.NET (C#)** ecosystem is historically recognized for its enterprise-grade excellence in mission-critical systems, high-performance asynchronous concurrency, strong typing, and first-class architectural patterns. With Microsoft's recent investments — notably **`Microsoft.Extensions.AI` (MEAI)**, **`Microsoft.Extensions.AI.Evaluation`**, the **Microsoft Agent Framework (MAF)**, **Semantic Kernel**, and **ML.NET** —, the .NET platform provides a unified, enterprise-ready toolchain.

This repository outlines a progressive, hands-on journey across **16 Modules** designed to train developers to operate as **AI Engineers with .NET**, covering the full development lifecycle end-to-end without superficial shortcuts.

---

### 0.2 Architectural Pillars of the Curriculum

All modules and capstone projects in this curriculum are built upon four non-negotiable engineering pillars:

```text
+---------------------------------------------------------------------------------------+
|                                CLEAN ARCHITECTURE / DDD                               |
+--------------------------+---------------------------+--------------------------------+
|     SOLID PRINCIPLES     |      DESIGN PATTERNS      |       TWELVE-FACTOR APP        |
|  - Single Responsibility |  - Decorator / Pipeline   |  - Stateless Processes (VI)    |
|  - Open/Closed           |  - Strategy / Factory     |  - Backing Services (IV)       |
|  - Liskov Substitution   |  - Adapter (MCP)          |  - Streamed Telemetry (XI)     |
|  - Interface Segregation |  - Circuit Breaker (Polly)|  - Config via Environment (III)|
|  - Dependency Inversion  |  - Saga / DAGs (Workflows)|  - Concurrency (VIII)          |
+--------------------------+---------------------------+--------------------------------+
|                              THE TWELVE-FACTOR AGENT                                  |
|  1. Model vs Deterministic Logic Separation      7. Tenant & Credential Isolation     |
|  2. 100% Stateless Agent Runtime                 8. Observability & Traceability      |
|  3. Graph / Event-Driven State Transitions       9. Continuous Evals & Test Suites    |
|  4. Structured I/O & TON/JSON Formats           10. Idempotency & Fault Tolerance     |
|  5. Deterministic Guardrails vs Injection       11. Human-in-the-Loop & Approvals     |
|  6. Tool Abstraction via MCP Protocol           12. Declarative Specifications (DbC)  |
+---------------------------------------------------------------------------------------+
```

1. **SOLID Principles Applied to AI**:
   - **SRP (Single Responsibility)**: Strict separation between inference models, structured prompt orchestration, defensive guardrails, and conversation history persistence.
   - **OCP (Open/Closed)**: Extensibility of the chat execution pipeline via delegating handlers (chat middlewares) without modifying underlying clients.
   - **LSP (Liskov Substitution)**: Seamless swapping across model providers (local Ollama, OpenAI, Azure OpenAI, Anthropic) while preserving domain contracts.
   - **ISP (Interface Segregation)**: Focused, single-purpose abstractions (`IChatClient`, `IEmbeddingGenerator`, `IToolRegistry`, `IEvaluator`).
   - **DIP (Dependency Inversion)**: Business logic depends solely on .NET abstractions, never on proprietary vendor SDKs.

2. **Structural and Behavioral Design Patterns**:
   - **Decorator / Chain of Responsibility**: Implemented via the `DelegatingChatClient` pipeline for audit logging, telemetry, caching, rate limiting, and moderation.
   - **Strategy & Factory**: Dynamic selection and instantiation of the optimal model or provider based on cost, latency, or context window constraints.
   - **Adapter**: Integration of legacy systems and enterprise APIs into the universal Model Context Protocol (MCP) format.
   - **Directed Acyclic Graph (DAG) / Saga**: Deterministic multi-agent coordination across workflows with branching and compensatory actions.
   - **Design by Contract (DbC)**: Formal specification of Preconditions, Invariants, and Postconditions/Acceptance Criteria for dependable agent execution.

3. **The Twelve-Factor App & The Twelve-Factor Agent**:
   - Adoption of stateless processes, decoupled backing services (for vector databases and caches), isolated configuration via environment variables, and telemetry treated as continuous event streams.
   - Implementation of the 12 tenets for autonomous agents: deterministic control over stochastic model outputs, tenant and credential isolation, compact schemas (JSON/TON), tool idempotency, and explicit Human-in-the-Loop approval gates.

---

## 1. Modules and Capstone Projects Syllabus

---

### Module 01: Connectivity Fundamentals and Unified Abstraction with `Microsoft.Extensions.AI`
- **Context & Goals**: Consume multiple AI providers (OpenAI, Azure OpenAI, Anthropic, Gemini) without vendor lock-in; master inference hyperparameters (`Temperature`, `TopK`, `TopP`, `MaxTokens`, `StopSequences`); measure and benchmark latency, cost (price per 1K tokens), and throughput (tokens per second).
- **Abstractions & Frameworks**: `Microsoft.Extensions.AI`, `IChatClient`, `ChatOptions`, `ChatResponse`, `OpenAI` client, `Azure.AI.OpenAI`.
- **Principles & Patterns**: Dependency Injection (.NET ServiceCollection), Strategy Pattern, Factory Pattern, Single Responsibility Principle (SRP).
- **Microsoft Learn References**:
  - [IChatClient concepts in .NET](https://learn.microsoft.com/dotnet/ai/ichatclient)
  - [API Reference: IChatClient](https://learn.microsoft.com/dotnet/api/microsoft.extensions.ai.ichatclient)
- **Capstone Project 01**:
  - *Name*: `AiProviderBenchmarker.Cli`
  - *Path*: `src/Module01/AiProviderBenchmarker/` (with a quick demo in `src/Module01/SimpleChatDemo/`)
  - *Run & Test*:
    - Quick Demo: `dotnet run --project src/Module01/SimpleChatDemo`
    - Interactive CLI: `dotnet run --project src/Module01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli`
    - Test Suite: `dotnet test src/Module01/AiProviderBenchmarker/AiProviderBenchmarker.slnx`
  - *Scope*: C# Console Application built with Clean Architecture that injects an `IChatClientFactory`. The application dispatches an identical batch of prompts across multiple providers concurrently, rendering real-time token-by-token streaming and a comparative table with metrics: TTFT (Time To First Token), total tokens generated, elapsed latency, and estimated FinOps cost.

---

### Module 02: Unified Model Gateway, Dynamic Routing, and OpenRouter
- **Context & Goals**: Build an enterprise AI gateway using aggregators like OpenRouter alongside internal intelligent routing; switch models dynamically at runtime based on payload size, cost, or latency thresholds; implement automated failover with Polly circuit breakers and retries.
- **Abstractions & Frameworks**: `Microsoft.Extensions.AI`, `DelegatingChatClient`, `Polly` (resilience in .NET), `System.Net.Http`.
- **Principles & Patterns**: Decorator Pattern (ChatClient Middleware), Circuit Breaker, Fallback Strategy, Twelve-Factor: Factor IV (Backing Services treated as attached resources).
- **Microsoft Learn References**:
  - [Custom IChatClient Middleware](https://learn.microsoft.com/dotnet/ai/ichatclient#custom-ichatclient-middleware)
  - [HTTP Resilience in .NET with Polly](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- **Capstone Project 02**:
  - *Name*: `SmartRouter.Gateway`
  - *Path*: `src/Module02/SmartRouter/`
  - *Run & Test*:
    - Minimal API: `dotnet run --project src/Module02/SmartRouter/src/SmartRouter.Gateway`
    - Test Suite: `dotnet test src/Module02/SmartRouter/SmartRouter.slnx`
  - *Scope*: C# ASP.NET Core Minimal API encapsulating a `SmartRoutingChatClient` inheriting from `DelegatingChatClient`. The client dynamically evaluates requests: short, cost-sensitive queries route to an economic tier (e.g., OpenRouter / Ollama); upon network errors, timeouts, or circuit trip, resilient failover automatically redirects calls to the premium high-availability tier (e.g., Azure OpenAI / OpenAI). Includes SSE streaming and health diagnostics.

---

### Module 03: LangChain in C# (LangChain.NET) vs. Microsoft Official Ecosystem: Critical Review and Comparative Engineering
- **Context & Goals**: Rigorous engineering evaluation of the LangChain ecosystem in .NET (`LangChain.NET` from the tryAGI community); conceptual mapping of *Prompts*, *Chains*, *Memory*, *Tools*, and *Agents* to Microsoft's canonical abstractions (`Microsoft.Extensions.AI`, `Semantic Kernel`, and `Microsoft Agent Framework`); in-depth analysis of architectural trade-offs, limitations, and risks in adopting community LangChain ports in production (violation of .NET idiomatic conventions and SOLID principles, governance and abandonware risk, lack of enterprise SLAs, leaky abstractions, GC/heap allocation overhead, absence of native OpenTelemetry/.NET Aspire integration and Native AOT support); decision matrix: when to consider LangChain for rapid prototyping and why Microsoft's official triad should be prioritized for enterprise production.
- **Abstractions & Frameworks**: `LangChain` (tryAGI), `Microsoft.Extensions.AI` (`IChatClient`, `DelegatingChatClient`), `Microsoft.SemanticKernel`, `Microsoft.Agents.AI`, `System.Diagnostics.DiagnosticSource` / `ActivitySource`.
- **Principles & Patterns**: Interface Segregation Principle (ISP), Dependency Inversion Principle (DIP), Decorator Pattern vs. Monolithic Chains, Benchmark & Comparative Analysis Pattern, Twelve-Factor Agent: Factor 1 (Deterministic Logic vs Stochastic Model) and Factor 8 (Native Observability).
- **Microsoft Learn References**:
  - [Overview of AI in .NET](https://learn.microsoft.com/dotnet/ai/)
  - [Unified Abstractions with Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/ichatclient)
  - [Semantic Kernel Concepts and Framework Migration](https://learn.microsoft.com/semantic-kernel/overview/)
- **Capstone Project 03**:
  - *Name*: `FrameworkComparison.Benchmarker`
  - *Path*: `src/Module03/FrameworkComparison/`
  - *Run & Test*:
    - Comparative CLI: `dotnet run --project src/Module03/FrameworkComparison/src/FrameworkComparison.Cli`
    - Test Suite: `dotnet test src/Module03/FrameworkComparison/FrameworkComparison.slnx`
  - *Scope*: Architectural and performance benchmark application running the same enterprise workload (order triage with structured prompt, validation pipeline, and telemetry instrumentation) using two distinct implementations side-by-side: `LangChain.NET` vs. `Microsoft.Extensions.AI`. Generates comparative CLI and Markdown reports evaluating: execution time, memory allocation (GC Gen0/Gen1/Gen2 allocations via `GC.GetAllocatedBytesForCurrentThread`), call stack depth, OpenTelemetry tracing coverage, and .NET Dependency Injection compliance.

---

### Module 04: User Sessions, Resilient History, and Agent Memory Management
- **Context & Goals**: State management architecture for conversational agents and chat systems; strict separation between short-term working memory and long-term episodic memory; context compaction strategies, sliding windows with exact token counting, and periodic background summarization; guaranteeing 100% stateless agents persisted in external backing stores.
- **Abstractions & Frameworks**: `Microsoft.Extensions.Caching.Distributed`, `StackExchange.Redis`, `System.Text.Json`, `Microsoft.Extensions.AI` (`ChatMessage`, `ChatRole`).
- **Principles & Patterns**: Repository Pattern, Outbox/Event-Driven Pattern for memory consolidation, Twelve-Factor: Factor VI (Stateless Processes), Twelve-Factor Agent: Factor 2 (Stateless Agent Runtime).
- **Microsoft Learn References**:
  - [Conversation and Session Management in Agent Framework](https://learn.microsoft.com/agent-framework/concepts/agents/conversations/session)
  - [Distributed Caching in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/distributed)
- **Capstone Project 04**:
  - *Name*: `ResilientMemory.Core`
  - *Path*: `src/Module04/ResilientMemory/`
  - *Scope*: Multi-tenant conversational session management service. Employs a background pipeline monitoring session token limits in Redis: when reaching 75% of model context capacity, it triggers an asynchronous summarization job to condense older turns, preserving conversational flow, user identity, and bounded prompt costs.

---

### Module 05: Enterprise RAG: Advanced Architecture, Hybrid Search, GraphRAG, and Evals
- **Context & Goals**: Overcoming the pitfalls of *Naive RAG*; parsing complex enterprise documents (PDFs with financial tables, hierarchical Markdown); advanced chunking methods (Semantic Chunking, Parent-Document Retriever, Sentence-Window); **Hybrid Search** uniting dense vector search with sparse lexical search (BM25/Full-Text) through Reciprocal Rank Fusion (RRF); **Re-Ranking** with Cross-Encoders for noise reduction and context filtering; **GraphRAG** principles (Microsoft Research) with entity graph extraction and community summarization; RAG Triad evaluation (*Context Relevance*, *Groundedness / Faithfulness*, *Answer Relevance*) using `Microsoft.Extensions.AI.Evaluation`.
- **Abstractions & Frameworks**: `IEmbeddingGenerator<TInput, TEmbedding>`, `Microsoft.Extensions.AI.Evaluation` (`GroundednessEvaluator`, `RelevanceEvaluator`, `RetrievalEvaluator`), `Microsoft.SemanticKernel.Connectors.Qdrant` / `AzureAISearch`, `Testcontainers`.
- **Principles & Patterns**: Pipeline Pattern, Strategy Pattern for hybrid retrieval, Continuous Evaluation (CI/CD for AI), Twelve-Factor Agent: Factor 9 (Testability & Evaluation Suites).
- **Microsoft Learn References**:
  - [The Microsoft.Extensions.AI.Evaluation libraries](https://learn.microsoft.com/dotnet/ai/evaluation/libraries)
  - [Evaluating Groundedness and Safety in .NET](https://learn.microsoft.com/dotnet/ai/evaluation/evaluate-safety)
  - [Vector Store Connectors in Semantic Kernel](https://learn.microsoft.com/semantic-kernel/concepts/vector-store-connectors/)
  - [Microsoft Research GraphRAG Overview](https://learn.microsoft.com/azure/developer/ai/intro-graphrag)
- **Capstone Project 05**:
  - *Name*: `EnterpriseKnowledge.AdvancedRag`
  - *Path*: `src/Module05/EnterpriseKnowledge/`
  - *Scope*: Production-grade enterprise RAG engine over complex technical documentation. Ingests hierarchical documents (Parent-Child), executes hybrid search (dense + sparse) with RRF fusion, applies a Cross-Encoder model to rerank the top 5 relevant passages, and synthesizes answers with verified citations. Accompanied by automated test suites using `Microsoft.Extensions.AI.Evaluation` to grade groundedness and fail builds on hallucination.

---

### Module 06: Sovereign Local AI with Ollama vs. Cloud: Trade-off Analysis and Hybrid Strategy
- **Context & Goals**: Run local open-source LLMs (Phi-3/4, Llama 3, DeepSeek) through Ollama with native .NET integration; comparative benchmarking: strict data privacy and local latency vs. cloud concurrency and compute capacity; hybrid architecture design (local-first with seamless cloud escalation).
- **Abstractions & Frameworks**: `Microsoft.Extensions.AI.Ollama`, Ollama API, Docker.
- **Principles & Patterns**: Liskov Substitution Principle (LSP - switching between Ollama and Azure without violating domain rules), Interface Segregation.
- **Microsoft Learn References**:
  - [Using an Ollama Chat Client in .NET](https://learn.microsoft.com/dotnet/ai/ichatclient#use-an-ollama-chat-client)
- **Capstone Project 06**:
  - *Name*: `HybridLocalCloud.Engine`
  - *Path*: `src/Module06/HybridLocalCloud/`
  - *Scope*: Document processing engine running locally on Ollama (`phi-4` or `llama3.2`). When a task requires advanced reasoning that exceeds local hardware, the application requests approval and dispatches sanitized data to cloud providers, generating a comparative telemetry report (VRAM/RAM vs billed tokens).

---

### Module 07: Advanced Prompt Engineering, Structured Notations (JSON/YAML/TON), and Multimodality
- **Context & Goals**:
  - Metaprompting techniques, Few-Shot exemplars, Chain-of-Thought (CoT), and strict separation of system and user message roles;
  - **Structured Instruction Architecture**: Rigorous comparison between Free Prose vs Markdown vs XML Tags vs Structured Schemas (JSON & YAML);
  - **Token Object Notation (TON)**: Fundamentals of TON as a compact data notation engineered to maximize information density for LLMs, stripping redundant delimiters and brackets, cutting token usage by 30%–40% while preserving hierarchy and strong typing;
  - **Negative Constraint Adherence**: How structured formats (JSON/YAML/TON) mitigate attention drift and enforce strict obedience to business invariants and security boundaries;
  - **Structured Outputs in .NET**: Deterministic output generation with `ChatResponseFormat.ForJsonSchema<T>`, strict schema validation with `System.Text.Json.Schema`, and strongly-typed C# DTOs;
  - **Multimodality**: Multimodal analysis uniting text, image inputs, speech-to-text transcription (Whisper), and computer vision.
- **Abstractions & Frameworks**: `ChatResponseFormat.ForJsonSchema<T>`, `System.Text.Json.Schema`, Azure AI Speech SDK, custom TON/YAML serializers for C#.
- **Principles & Patterns**: Type Safety, Strongly-typed Data Transfer Objects (DTOs), Twelve-Factor Agent: Factor 4 (Structured I/O & TON/JSON Formats).
- **Microsoft Learn References**:
  - [Structured Output with IChatClient](https://learn.microsoft.com/dotnet/ai/ichatclient#structured-output)
  - [Structured Outputs with Azure OpenAI](https://learn.microsoft.com/azure/ai-services/openai/how-to/structured-outputs)
- **Capstone Project 07**:
  - *Name*: `PromptArchitectAndTriage.Service`
  - *Path*: `src/Module07/PromptArchitect/`
  - *Scope*: Corporate claims triage service ingesting multiple inputs (customer voice notes via Whisper, photo evidence via vision models) and generating an expert insurance assessment validated against a C# `record SinistroReport(...)`. Includes an automated benchmark comparing System Prompt execution across Prose, JSON, YAML, and TON, reporting: tokens consumed, latency, and compliance rate across 10 complex constraints.

---

### Module 08: Governance, Usage Contracts, Privacy, and Zero Data Retention
- **Context & Goals**: Deep-dive into provider terms of service (OpenAI Enterprise, Azure OpenAI Data & Privacy, Anthropic Commercial Terms); data transit auditing, ephemeral retention policies, and opting out of customer data training (Zero Data Retention / HIPAA / GDPR).
- **Abstractions & Frameworks**: Azure OpenAI Customer Managed Keys (CMK), Virtual Network (VNet) private endpoints, DLP (Data Loss Prevention) patterns.
- **Principles & Patterns**: Privacy by Design, Zero Trust Architecture, Twelve-Factor: Factor X (Dev/Prod Security Parity).
- **Microsoft Learn References**:
  - [Data, Privacy, and Security for Azure OpenAI](https://learn.microsoft.com/legal/cognitive-services/openai/data-privacy)
  - [Azure Security Baseline for AI Services](https://learn.microsoft.com/azure/ai-services/openai/concepts/security-baseline)
- **Capstone Project 08**:
  - *Name*: `DataPrivacyAuditor.Tool`
  - *Path*: `src/Module08/DataPrivacyAuditor/`
  - *Scope*: Compliance auditing tool intercepting AI calls via HTTP middleware, inspecting response headers (verifying non-retention compliance flags), validating that sensitive data routes only through authorized endpoints and regions, and publishing an Azure Trust Center compliance report.

---

### Module 09: Cross-Cutting Observability, Token FinOps, and Dashboards with OpenTelemetry
- **Context & Goals**: End-to-end telemetry with OpenTelemetry (Metrics, Structured Logs, Distributed GenAI Traces); tracking token consumption (prompt, completion), per-model latency, session counts, and user volume; exporting telemetry to .NET Aspire Dashboard, Prometheus/Grafana, or Azure Monitor.
- **Abstractions & Frameworks**: `OpenTelemetry`, `Microsoft.Extensions.AI` (`UseOpenTelemetry`), .NET Aspire, `System.Diagnostics.Activity`.
- **Principles & Patterns**: Observability as Code, Twelve-Factor: Factor XI (Logs as Streams), Twelve-Factor Agent: Factor 8 (Comprehensive Observability & Traceability).
- **Microsoft Learn References**:
  - [Telemetry and Metrics in IChatClient](https://learn.microsoft.com/dotnet/ai/ichatclient#telemetry)
  - [Telemetry in .NET Aspire](https://learn.microsoft.com/dotnet/aspire/fundamentals/telemetry)
- **Capstone Project 09**:
  - *Name*: `GenAiObservability.Dashboard`
  - *Path*: `src/Module09/GenAiObservability/`
  - *Scope*: Distributed system orchestrated with .NET Aspire using `client.AsBuilder().UseOpenTelemetry().Build()`. Displays all LLM interactions in the Aspire Dashboard, cumulative tenant/user costs, error rates, cascading tool invocation traces, and automated FinOps budget alerts when daily spending limits are reached.

---

### Module 10: Offensive and Defensive Security: OWASP Top 10 for LLMs, Prompt Injection, and Guardrails
- **Context & Goals**: Classical threats (Direct/Indirect Prompt Injection, Jailbreaking, System Prompt Extraction, Insecure Output Handling); sanitization and strict context isolation of system prompts vs untrusted user inputs; content moderation and programmatic guardrails.
- **Abstractions & Frameworks**: Azure AI Content Safety SDK, C# Guardrail engines, Regex/Semantic Pre-validators.
- **Principles & Patterns**: Defense in Depth, Fail-Safe Defaults, Twelve-Factor Agent: Factor 5 (Deterministic Guardrails vs Stochastic LLMs).
- **Microsoft Learn References**:
  - [Azure AI Content Safety Overview](https://learn.microsoft.com/azure/ai-services/content-safety/overview)
  - [Content Filtering in Azure OpenAI](https://learn.microsoft.com/azure/ai-services/openai/concepts/content-filter)
- **Capstone Project 10**:
  - *Name*: `AiFirewall.Middleware`
  - *Path*: `src/Module10/AiFirewall/`
  - *Scope*: Custom `DelegatingChatClient` operating as an AI Web Application Firewall (WAF). Scans incoming messages against known prompt injection signatures, validates payloads against the Azure Content Safety API, and intercepts outgoing responses to redact leaked API keys or personally identifiable information (PII).

---

### Module 11: Classical Machine Learning with ML.NET: Predictive Models and GenAI Hybridization
- **Context & Goals**: Data ingestion and pipelines (`IDataView`), training, validation, and inference entirely in C# without Python runtimes; regression, multiclass classification, and anomaly detection algorithms on logs and documents; deploying ML.NET models as fast, cost-effective intent routers prior to invoking LLMs.
- **Abstractions & Frameworks**: `Microsoft.ML`, `MLContext`, `ITransformer`, `PredictionEnginePool`.
- **Principles & Patterns**: Data Pipeline Pattern, Pre-filtering Pattern (Early Termination Pattern).
- **Microsoft Learn References**:
  - [Official ML.NET Documentation](https://learn.microsoft.com/dotnet/machine-learning/)
  - [Predictive Model Tutorials with ML.NET](https://learn.microsoft.com/dotnet/machine-learning/tutorials/predict-prices)
- **Capstone Project 11**:
  - *Name*: `SmartSupportTriage.Hybrid`
  - *Path*: `src/Module11/SmartSupportTriage/`
  - *Scope*: Customer support ticket triage pipeline: an on-premises model trained with ML.NET predicts ticket category and severity in microseconds. Standard issues receive pre-canned deterministic responses; complex or anomalous tickets are context-enriched and escalated to the LLM pipeline for deep resolution.

---

### Module 12: Extensibility with Model Context Protocol (MCP) in .NET
- **Context & Goals**: The Model Context Protocol (MCP) as an open interoperability standard to connect LLMs to enterprise databases, tools, and internal APIs; building native C# MCP servers and clients; dynamic runtime tool injection into chat pipelines.
- **Abstractions & Frameworks**: MCP Protocol (JSON-RPC / SSE / stdio), `Microsoft.Extensions.AI` Tool Calling / Function Invocation, C# MCP client/server SDKs.
- **Principles & Patterns**: Adapter Pattern, Inversion of Control, Twelve-Factor Agent: Factor 6 (Tool Abstraction & MCP).
- **Microsoft Learn References**:
  - [Tools and Integrations in Agent Framework](https://learn.microsoft.com/agent-framework/agents/tools/)
  - [Function and Tool Calling with IChatClient](https://learn.microsoft.com/dotnet/ai/ichatclient#tool-calling)
- **Capstone Project 12**:
  - *Name*: `EnterpriseMcpServer.Connector`
  - *Path*: `src/Module12/EnterpriseMcpServer/`
  - *Scope*: Enterprise .NET MCP server exposing SQL database resources and REST API operations via standardized schemas. Followed by a C# client using `IChatClient` consuming tools over SSE, enabling the LLM to run secure, audited analytical queries safely.

---

### Module 13: AI Agents with Microsoft Agent Framework (MAF), Semantic Kernel, and Declarative Agents
- **Context & Goals**:
  - Formal definition of an Agent (Persona, Tools, Memory, Autonomous Decision-Making);
  - The **Microsoft Agent Framework** (`Microsoft.Agents.AI`) ecosystem and interoperability with `Microsoft.SemanticKernel.Agents`;
  - **Declarative Agents**: Modeling agents following Microsoft's **Declarative Agent Manifest (JSON Schema / YAML / TON specs)**, cleanly decoupling behavioral specifications from C# execution logic;
  - Agent lifecycles, thread isolation, and **Harness Agent** patterns (autonomous planning, task backlogs, context compaction, and tool authorization);
  - Enforcing corporate governance with **Human-in-the-Loop (HITL)** for sensitive operations.
- **Abstractions & Frameworks**: `Microsoft.Agents.AI` (`AIAgent`, `AIProjectClient`), `Microsoft.SemanticKernel.Agents`, Declarative Manifest Schemas (JSON/YAML/TON).
- **Principles & Patterns**: Autonomous Agent Pattern, Human-in-the-Loop (HITL), Declarative Specification Pattern, Twelve-Factor Agent: Factor 11 (Human-in-the-Loop & Approval Gates) and Factor 12 (Declarative Specifications).
- **Microsoft Learn References**:
  - [Overview of Microsoft Agent Framework](https://learn.microsoft.com/agent-framework/overview/)
  - [Harness Agent Concepts](https://learn.microsoft.com/agent-framework/concepts/harness)
  - [Microsoft Declarative Agent Manifest Schema](https://learn.microsoft.com/microsoft-365/copilot/extensibility/declarative-agent-manifest-1.8)
- **Capstone Project 13**:
  - *Name*: `AutonomousDevAssistant.Agent`
  - *Path*: `src/Module13/AutonomousDevAssistant/`
  - *Scope*: Intelligent developer agent whose persona, tools, and governance policies are dynamically loaded from a declarative manifest (JSON/YAML/TON). The agent receives natural language engineering requests, builds in-memory execution plans, inspects repositories and edits code files via MCP, and pauses the execution runtime to demand explicit developer confirmation before high-risk operations (e.g., git commit, deployment).

---

### Module 14: Advanced Multi-Agent Orchestration: Graph Workflows and Parallel/Sequential Execution
- **Context & Goals**: Deterministic vs. stochastic multi-agent orchestration; graph-based workflows in Microsoft Agent Framework; sequential, parallel (fan-out/fan-in), and dynamic orchestration inspired by Magentic-One; state isolation across execution nodes via `IResettableExecutor`.
- **Abstractions & Frameworks**: `Microsoft.Agents.AI.Workflows`, Workflow Builders, Graph State Management, `IResettableExecutor`.
- **Principles & Patterns**: Saga Pattern, Directed Acyclic Graph (DAG), Event-Driven Architecture, Twelve-Factor Agent: Factor 3 (Event-Driven & Graph State Transitions).
- **Microsoft Learn References**:
  - [State Isolation in MAF Workflows](https://learn.microsoft.com/agent-framework/concepts/workflows/state)
  - [Magentic Orchestration in Agent Framework](https://learn.microsoft.com/agent-framework/workflows/orchestrations/magentic)
- **Capstone Project 14**:
  - *Name*: `EnterpriseAudit.MultiAgentWorkflow`
  - *Path*: `src/Module14/EnterpriseAudit/`
  - *Scope*: Orchestrated workflow comprising 3 specialized agents (Regulations Researcher, Financial Data Analyst, and Compliance Auditor) coordinated by a Manager Agent. The workflow executes parallel data analysis, reconciles discrepancies across conditional graph branches, and compiles an audited final report with isolated state across concurrent runs.

---

### Module 15: Design by Contract (DbC) for Autonomous Agents in C#
- **Context & Goals**:
  - Adapting Bertrand Meyer's classical **Design by Contract (DbC)** principles to AI Agent architectures;
  - Formal instruction decomposition: **Preconditions** (input validation and initial state), **Invariants** (strict business rules the agent must never violate), and **Postconditions / Acceptance Criteria** (deterministic *Definition of Done*);
  - Parsing and compiling declarative contracts from Markdown files with Frontmatter (`*.agent.md`) using `Markdig` and `YamlDotNet` into strongly-typed C# contract records;
  - Deterministic postcondition validation engine and autonomous self-correction loops (*Self-Correction / Retry Loops*) whenever the LLM outputs violate acceptance criteria.
- **Abstractions & Frameworks**: `Markdig` (with `UseYamlFrontMatter`), `YamlDotNet`, `System.Text.Json.Schema`, `Microsoft.Extensions.AI`.
- **Principles & Patterns**: Design by Contract (DbC), Liskov Substitution Principle (LSP), Open/Closed Principle (OCP), Twelve-Factor Agent: Factor 1 (Deterministic Logic vs Stochastic Model) and Factor 5 (Deterministic Guardrails).
- **Microsoft Learn References**:
  - [Extensibility and Dependency Injection in .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection)
  - [Harness Agent and Validation Concepts](https://learn.microsoft.com/agent-framework/concepts/harness)
- **Capstone Projects 15**:
  - *Project 15.1*: `DbcContractParser.Core` — Library and CLI parsing `*.agent.md` files, validating the syntax of preconditions, invariants, and acceptance criteria into strongly-typed `record AgentContract(...)` objects.
  - *Project 15.2*: `ContractEnforcer.Runner` — Legal and tax analysis agent whose execution is bounded by deterministic postcondition validation. If the LLM hallucinates or omits an obligatory clause required by the Markdown contract, the runtime rejects completion and re-executes the step, injecting the contractual violation as corrective feedback.

---

### Module 16: Grand Master Capstone — The Enterprise Agentic Platform
- **Context & Goals**:
  - Definitive convergence of **ALL curriculum disciplines (Modules 01 to 15)** into a production-grade enterprise platform (*Production-Ready Enterprise Platform*);
  - Architecture and implementation of an **Autonomous Agent Platform** in C# .NET serving as an agnostic AI microservice host across the enterprise;
  - Hot-reload and Multi-tenancy support: publish and update agents in production simply by adding new `*.agent.md` contract files or `*.workflow.md` graph manifests to watched Git repositories, without recompiling C# code;
  - Full end-to-end infrastructure integration:
    - Unified gateway with Polly resilience (Modules 1 and 2);
    - Comparative framework analysis and performance benchmarks (Module 3);
    - Resilient user sessions and memory management in Redis (Module 4);
    - Enterprise Hybrid RAG with GraphRAG and Groundedness Evals (Module 5);
    - Hybrid local Ollama + cloud Azure/OpenRouter deployment (Module 6);
    - Token optimization with TON notation and Structured Outputs (Module 7);
    - Data privacy auditing and Zero Data Retention compliance (Module 8);
    - Comprehensive OpenTelemetry observability in .NET Aspire Dashboard (Module 9);
    - AI WAF and Prompt Injection defense (Module 10);
    - ML.NET predictive pre-filtering (Module 11);
    - Dynamically distributed tools via Model Context Protocol (Module 12);
    - MAF Agent orchestration and graph workflows (Modules 13 and 14);
    - Strict contractual governance via Design by Contract (Module 15).
- **Abstractions & Frameworks**: Complete platform stack (`Microsoft.Agents.AI`, `Microsoft.Extensions.AI`, `Microsoft.Extensions.AI.Evaluation`, `OpenTelemetry`, `Polly`, `.NET Aspire`, `ML.NET`, `MCP`, `StackExchange.Redis`, `Markdig`).
- **Principles & Patterns**: The Twelve-Factor App & The Twelve-Factor Agent, Clean Architecture / DDD, Enterprise Integration Patterns, Zero Trust Architecture.
- **Capstone Project 16 (Grand Finale Capstone)**:
  - *Name*: `EnterpriseAgenticPlatform.Host`
  - *Path*: `src/Module16/EnterpriseAgenticPlatform/`
  - *Scope*: The definitive enterprise AI platform in .NET. A distributed solution orchestrated with .NET Aspire that provisions an agent execution cluster, delivers a web portal for FinOps metrics and contract audits, hosts an enterprise MCP tool catalog, and executes complex business workflows (e.g., *Automated Supplier Onboarding* encompassing legal, tax, and security audits) driven 100% by declarative Markdown contracts with mathematical, deterministic compliance guarantees.

---

## 2. How to Use this Repository

Each module above includes clearly defined boundaries and metadata so you can follow it as an iterative, self-paced learning path:

1. **Study Guide and Specification Generation**: Use the **Meta-Prompt in Section 3** to generate a dedicated Markdown specification at `docs/modules/module-XX-<name>/STUDY_GUIDE.md` for your chosen module. This guide provides in-depth theory, cognitive checkpoints, and the full architectural specification for the capstone project.
2. **Review and Checkpoint Validation**: Read the generated study guide, explore the referenced Microsoft Learn links, and validate each conceptual checkpoint before writing code.
3. **Build the Capstone Project**: Follow the architectural specification in the guide to implement the project in C# (.NET 10 LTS) inside the corresponding modular folder in `src/ModuleXX/<ProjectName>` (for example: `src/Module01/AiProviderBenchmarker/`, `src/Module02/SmartRouter/`). Any supplementary projects or quick didactic demos should also be located inside their module folder (e.g., `src/Module01/SimpleChatDemo/`).
4. **Continuous Improvement**: Each capstone project independently implements automated unit/integration tests, observability instrumentation, and software engineering best practices incrementally.

---

## 3. Meta-Prompt: Generator of Study Guides and Architectural Specifications

Copy and send the prompt below to your LLM (or execute it within this same AI session), specifying your desired module (for example: `"Execute the Meta-Prompt for Module 01"`).

The LLM will generate a comprehensive Markdown guide in `docs/modules/module-XX-<name>/STUDY_GUIDE.md`, serving as your textbook and engineering specification for that module.

````markdown
# SYSTEM PROMPT: SENIOR SOFTWARE ARCHITECT & AI INSTRUCTOR (.NET / C#)

## ROLE AND OBJECTIVE
You are a Senior Software Architect and Artificial Intelligence Specialist within the .NET ecosystem.
Your mission is to read the syllabus of a specific module from the **MafMastery (README.md)** repository and generate a comprehensive, exhaustive, and technically rigorous Markdown document at:
`docs/modules/module-{{MODULE_NUMBER}}-{{MODULE_NAME}}/STUDY_GUIDE.md`

This document must NOT contain the final code of the implemented project, but rather:
1. Thorough theoretical study material;
2. Self-assessment cognitive checkpoints;
3. The complete technical and architectural specification to guide the development of the module's Capstone Project.

---

## DESIGN AND ENGINEERING GUIDELINES
All content must reflect the four pillars of MafMastery:
- **Clean Architecture & DDD**: Strict separation of concerns (Domain, Application, Infrastructure, Presentation).
- **SOLID Principles**: Concrete demonstrations of how SRP, OCP, LSP, ISP, and DIP are applied to AI components in this module.
- **Classical Design Patterns**: Identification and specification of applied patterns (e.g., Decorator via DelegatingChatClient, Strategy, Factory, Adapter via MCP, Saga/DAG).
- **The Twelve-Factor App & The Twelve-Factor Agent**: Practical adoption of relevant factors (Stateless Runtime, Structured I/O with TON/JSON, Backing Services, Observability as Code, Deterministic Guardrails, Design by Contract).
- **Target Technology**: .NET 10 (LTS) and modern C#, prioritizing official Microsoft abstractions (`Microsoft.Extensions.AI`, `Microsoft.Agents.AI`, `Microsoft.Extensions.AI.Evaluation`, `OpenTelemetry`, etc.).

---

## MANDATORY STRUCTURE OF `STUDY_GUIDE.md`

The generated document must strictly adhere to the following structured outline:

### # Module {{NUMBER}}: {{MODULE NAME}} — Study Guide and Engineering Specification

#### 1. Overview and Learning Goals
- Executive summary of the real-world market challenge solved by this module.
- Practical engineering skills acquired upon completion.

#### 2. In-Depth Theoretical Foundations
- Exhaustive conceptual explanation of each syllabus topic.
- Deep-dive into .NET runtime internals and AI invocation mechanics (memory allocation, asynchronous concurrency with `Task`/`IAsyncEnumerable`, token streaming pipelines).
- Technical trade-offs, advantages, disadvantages, and common production pitfalls (*Gotchas & Anti-patterns*).

#### 3. Architectural Principles and Patterns Mapping
- **SOLID in this Module**: Table or itemized list detailing where each of the 5 principles operates.
- **Applied Design Patterns**: Selected patterns and their architectural rationale.
- **Twelve-Factor App & Agent**: Specific factors fulfilled and implementation guidance.
- **Architectural Diagram**: Conceptual Mermaid diagram (`graph TD` or `sequenceDiagram`) illustrating interactions between the domain, AI pipeline, and external services.

#### 4. Cognitive Learning Checkpoints
- List of 5 to 8 conceptual questions and architectural scenarios, complete with answer keys and rationale for pre-code self-assessment.

#### 5. Recommended Reading & Microsoft Learn Grounding
- Curated list of official Microsoft Learn links (API documentation, architectural guides, and official NuGet packages).

#### 6. Capstone Project Technical Specification
- **Project Name**: Standardized project name as defined in README.
- **Solution Path**: Standardized path under the module directory: `src/Module{{NUMBER}}/{{PROJECT_NAME}}/`.
- **Scope & Business Problem**: Simulated enterprise use case.
- **Functional Requirements (FRs)**: Numbered list (FR-01, FR-02...) defining required capabilities.
- **Non-Functional Requirements (NFRs)**: Latency, resilience, observability, decoupling, and memory allocation constraints.
- **Project and Directory Structure (Clean Architecture)**:
  - Example: `Domain`, `Application`, `Infrastructure`, `Presentation` under `src/Module{{NUMBER}}/{{PROJECT_NAME}}/`.
- **Core Interface Contracts and Entities**:
  - C# code showcasing only foundational contracts (interfaces, records, DTOs, and method signatures) without concrete business logic implementations.
- **Acceptance Criteria & Definition of Done (DoD)**:
  - Formal checklist to consider the project complete.
- **Minimum Testing Plan**:
  - Required Unit Test scenarios (mocking with xUnit/Moq/NSubstitute) and Integration Test requirements.

#### 7. Step-by-Step Build Roadmap
- Ordered logical sequence (Step 1 to Step N) guiding the developer to build the software incrementally and testably, including explicit terminal commands for project initialization, running (`dotnet run --project ...`), and testing (`dotnet test ...`).
````