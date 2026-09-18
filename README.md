# MafMastery: Trilha de Engenharia de IA com .NET e C#

> **Construindo Sistemas de Inteligência Artificial Corporativos, Resilientes e Escaláveis sob a Ótica da Engenharia de Software.**

---

## 0. Introdução: O Papel da Engenharia de Software na IA com .NET

### 0.1 Contexto e Justificativa do Roteiro
O ecossistema contemporâneo de Inteligência Artificial Generativa é amplamente dominado por materiais, bibliotecas e tutoriais voltados para Python e JavaScript/TypeScript. Embora essas linguagens sejam excelentes para prototipagem rápida, experimentação científica e scripts em notebooks, o cenário da **engenharia de produção em larga escala** impõe desafios substancialmente diferentes:

- **Acoplamento excessivo**: scripts que misturam chamadas diretas a APIs de IA com regras de negócio essenciais.
- **Falta de Type Safety**: fragilidade na validação de esquemas e respostas geradas por LLMs sem tipagem estrita em tempo de compilação.
- **Fragilidade operacional**: ausência de políticas robustas de resiliência (retries, fallbacks, circuit breakers) e injeção de dependência desacoplada.
- **RAG ingênuo (Naive RAG)**: implementações simplistas baseadas em chunks cegos e busca vetorial por cosseno que falham em cenários corporativos complexos.
- **Instruções em prosa desestruturada**: prompts gigantescos em texto livre que geram alta taxa de desobediência a restrições (*Rule Violation*) e consumo descontrolado de tokens.
- **Ausência de Contratos de Execução**: falta de uma separação formal entre a infraestrutura de execução (runtime) e os critérios de aceite e invariantes de negócio dos agentes.
- **Déficit de governança e segurança**: carência de esteiras com auditoria transparente, telemetria padronizada e proteção contra vulnerabilidades específicas de IA (OWASP Top 10 for LLM).

O ecossistema **.NET (C#)** é historicamente reconhecido por sua excelência em sistemas corporativos de missão crítica, alto desempenho de concorrência assíncrona, forte tipagem e suporte nativo a padrões arquiteturais consagrados. Com as evoluções recentes da Microsoft — em especial o **`Microsoft.Extensions.AI` (MEAI)**, o **`Microsoft.Extensions.AI.Evaluation`**, o **Microsoft Agent Framework (MAF)**, o **Semantic Kernel** e o **ML.NET** —, a plataforma .NET oferece um ferramental unificado de nível enterprise.

Este repositório documenta uma jornada progressiva e completa em **15 Módulos** para capacitar desenvolvedores a atuarem como **Engenheiros de IA com .NET**, cobrindo o ciclo de desenvolvimento de ponta a ponta sem atalhos superficiais.

---

### 0.2 Pilares Arquiteturais da Trilha

Todos os módulos e projetos integradores deste roteiro são projetados com base em quatro pilares inegociáveis de engenharia:

```
+---------------------------------------------------------------------------------------+
|                                ARQUITETURA LIMPA / DDD                                 |
+--------------------------+---------------------------+--------------------------------+
|     PRINCÍPIOS SOLID     |      DESIGN PATTERNS      |       TWELVE-FACTOR APP        |
|  - Single Responsibility |  - Decorator / Pipeline   |  - Stateless Processes (VI)    |
|  - Open/Closed           |  - Strategy / Factory     |  - Backing Services (IV)       |
|  - Liskov Substitution   |  - Adapter (MCP)          |  - Telemetria em Stream (XI)   |
|  - Interface Segregation |  - Circuit Breaker (Polly)|  - Config via Ambiente (III)   |
|  - Dependency Inversion  |  - Saga / DAGs (Workflows)|  - Concorrência (VIII)         |
+--------------------------+---------------------------+--------------------------------+
|                              THE TWELVE-FACTOR AGENT                                  |
|  1. Separação Modelo vs Lógica Determinística    7. Isolamento de Credenciais/Tenants |
|  2. Runtime de Agente 100% Stateless             8. Observabilidade & Rastreabilidade |
|  3. Transições de Estado em Grafo / Eventos      9. Evals Contínuos & Suíte de Testes |
|  4. I/O Estruturado & Formatos TON/JSON         10. Idempotência e Tolerância a Falhas |
|  5. Guardrails Determinísticos contra Injeção   11. Human-in-the-Loop & Aprovação     |
|  6. Abstração de Ferramentas via Protocolo MCP  12. Especificação Declarativa (DbC)   |
+---------------------------------------------------------------------------------------+
```

1. **Princípios SOLID Aplicados à IA**:
   - **SRP (Single Responsibility)**: Separação rigorosa entre os modelos de inferência, orquestração de prompts estruturados, guardrails defensivos e persistência de histórico.
   - **OCP (Open/Closed)**: Extensibilidade do pipeline de chat via delegating handlers (middlewares) sem alterar os clientes subjacentes.
   - **LSP (Liskov Substitution)**: Troca transparente entre provedores de modelos (Ollama local, OpenAI, Azure OpenAI, Anthropic) preservando os contratos do domínio.
   - **ISP (Interface Segregation)**: Interfaces focadas e de responsabilidade única (`IChatClient`, `IEmbeddingGenerator`, `IToolRegistry`, `IEvaluator`).
   - **DIP (Dependency Inversion)**: Camadas de negócio dependentes exclusivamente de abstrações do .NET, nunca de SDKs proprietários específicos.

2. **Design Patterns Estruturais e Comportamentais**:
   - **Decorator / Chain of Responsibility**: Implementado pelo pipeline de `DelegatingChatClient` para auditoria, telemetria, caching, rate limiting e moderação.
   - **Strategy & Factory**: Seleção e instanciação dinâmica do modelo ou provedor ideal baseado em métricas de custo, latência ou tamanho de contexto.
   - **Adapter**: Integração de sistemas e APIs legadas ao formato universal do Model Context Protocol (MCP).
   - **Directed Acyclic Graph (DAG) / Saga**: Coordenação determinística de múltiplos agentes em fluxos de trabalho com branchings e compensações.
   - **Design by Contract (DbC)**: Separação formal de Pré-condições, Invariantes e Critérios de Aceite para execução confiável de agentes.

3. **The Twelve-Factor App & The Twelve-Factor Agent**:
   - Adoção de processos sem estado (*stateless*), desacoplamento de serviços de apoio (*backing services* para bancos vetoriais e caches), configuração isolada em variáveis de ambiente e telemetria como fluxo de eventos.
   - Adoção das 12 diretrizes para agentes autônomos: controle determinístico sobre saídas estocásticas, isolamento rigoroso de credenciais e tenants, formatos compactos e esquemas fortemente validados (JSON/TON), idempotência em chamadas de ferramentas e portões explícitos de intervenção humana (*Human-in-the-Loop*).

---

## 1. Grade de Módulos e Projetos Integradores

---

### Módulo 01: Fundamentos de Conectividade e Abstração Unificada com `Microsoft.Extensions.AI`
- **Contexto & Metas**: Consumir múltiplos provedores (OpenAI, Azure OpenAI, Anthropic, Gemini) sem acoplamento; dominar parâmetros de inferência (`Temperature`, `TopK`, `TopP`, `MaxTokens`, `StopSequences`); medir e comparar latência, custo (preço por 1k tokens) e throughput (tokens por segundo).
- **Abstrações & Frameworks**: `Microsoft.Extensions.AI`, `IChatClient`, `ChatOptions`, `ChatResponse`, `OpenAI` client, `Azure.AI.OpenAI`.
- **Princípios & Padrões**: Dependency Injection (.NET ServiceCollection), Strategy Pattern, Factory Pattern, Single Responsibility Principle (SRP).
- **Referências Microsoft Learn**:
  - [Conceitos de IChatClient no .NET](https://learn.microsoft.com/dotnet/ai/ichatclient)
  - [API Reference: IChatClient](https://learn.microsoft.com/dotnet/api/microsoft.extensions.ai.ichatclient)
- **Projeto Integrador 01**: 
  - *Nome*: `AiProviderBenchmarker.Cli`
  - *Escopo*: Console App em C# construído com Clean Architecture que injeta uma `IChatClientFactory`. O app envia um lote idêntico de prompts para múltiplos provedores simultaneamente, exibindo no console streaming token-a-token e métricas lado a lado (TTFT - Time To First Token, total de tokens gerados, latência total e custo financeiro estimado).

---

### Módulo 02: Gateway Unificado de Modelos, Roteamento Dinâmico e OpenRouter
- **Contexto & Metas**: Implementar um gateway unificado utilizando agregadores como OpenRouter e roteamento inteligente interno; alternar modelos em tempo de execução com base em custo, latência ou fallback em caso de indisponibilidade (circuit breaker e retries).
- **Abstrações & Frameworks**: `Microsoft.Extensions.AI`, `DelegatingChatClient`, `Polly` (resiliência no .NET), `System.Net.Http`.
- **Princípios & Padrões**: Decorator Pattern (Middleware de ChatClient), Circuit Breaker, Fallback Strategy, Twelve-Factor: Factor IV (Backing Services tratados como recursos anexados).
- **Referências Microsoft Learn**:
  - [Custom IChatClient Middleware](https://learn.microsoft.com/dotnet/ai/ichatclient#custom-ichatclient-middleware)
  - [Resiliência HTTP no .NET com Polly](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- **Projeto Integrador 02**:
  - *Nome*: `SmartRouter.Gateway`
  - *Escopo*: API WebMinimal em C# que encapsula um `SmartRoutingChatClient` herdando de `DelegatingChatClient`. O cliente avalia dinamicamente a requisição: consultas simples vão para modelos de baixo custo via OpenRouter; se houver instabilidade ou latência acima do limiar, a resiliência aciona fallback transparente para o Azure OpenAI.

---

### Módulo 03: Sessões de Usuário, Histórico Resiliente e Gerenciamento de Memória de Agente
- **Contexto & Metas**: Arquitetura de gerenciamento de estado para agentes e chats conversacionais; separação estrita entre memória de trabalho de curto prazo (*short-term working memory*) e memória episódica de longo prazo (*long-term episodic memory*); estratégias de compactação de contexto, janela deslizante com contagem exata de tokens e sumarização periódica em background; garantia de agentes 100% stateless persistidos em serviços externos.
- **Abstrações & Frameworks**: `Microsoft.Extensions.Caching.Distributed`, `StackExchange.Redis`, `System.Text.Json`, `Microsoft.Extensions.AI` (`ChatMessage`, `ChatRole`).
- **Princípios & Padrões**: Repository Pattern, Outbox/Event-Driven Pattern para consolidação de memória, Twelve-Factor: Factor VI (Processos Stateless), Twelve-Factor Agent: Factor 2 (Stateless Agent Runtime).
- **Referências Microsoft Learn**:
  - [Gerenciamento de Sessão e Conversação no Agent Framework](https://learn.microsoft.com/agent-framework/concepts/agents/conversations/session)
  - [Cache Distribuído em .NET](https://learn.microsoft.com/aspnet/core/performance/caching/distributed)
- **Projeto Integrador 03**:
  - *Nome*: `ResilientMemory.Core`
  - *Escopo*: Serviço de gerenciamento de sessão conversacional com suporte multi-tenant. Implementa pipeline em background que monitora a contagem de tokens da sessão no Redis: ao atingir 75% da janela do modelo, aciona um processo assíncrono para sumarizar e consolidar mensagens anteriores, mantendo a conversa fluida, preservando a identidade do usuário e garantindo custo controlado de prompt.

---

### Módulo 04: Enterprise RAG: Arquitetura Avançada, Hybrid Search, GraphRAG e Evals
- **Contexto & Metas**: Superar as limitações e falhas do *Naive RAG*; técnicas de parsing de documentos complexos (PDFs com tabelas, Markdown hierárquico); estratégias de chunking avançadas (Semantic Chunking, Parent-Document Retriever, Sentence-Window); **Hybrid Search** combinando busca vetorial densa com busca esparsa (BM25/Full-Text) via Reciprocal Rank Fusion (RRF); **Re-Ranking** com Cross-Encoders para eliminação de ruído e corte drástico de tokens; conceitos de **GraphRAG** (Microsoft Research) com extração de entidades e sumarização de comunidades em grafos; métricas da Tríade do RAG (*Context Relevance*, *Groundedness / Faithfulness*, *Answer Relevance*) com `Microsoft.Extensions.AI.Evaluation`.
- **Abstrações & Frameworks**: `IEmbeddingGenerator<TInput, TEmbedding>`, `Microsoft.Extensions.AI.Evaluation` (`GroundednessEvaluator`, `RelevanceEvaluator`, `RetrievalEvaluator`), `Microsoft.SemanticKernel.Connectors.Qdrant` / `AzureAISearch`, `Testcontainers`.
- **Princípios & Padrões**: Pipeline Pattern, Strategy Pattern para busca híbrida, Continuous Evaluation (CI/CD para IA), Twelve-Factor Agent: Factor 9 (Testability & Evaluation Suites).
- **Referências Microsoft Learn**:
  - [The Microsoft.Extensions.AI.Evaluation libraries](https://learn.microsoft.com/dotnet/ai/evaluation/libraries)
  - [Avaliação de Groundedness e Segurança no .NET](https://learn.microsoft.com/dotnet/ai/evaluation/evaluate-safety)
  - [Vector Store Connectors no Semantic Kernel](https://learn.microsoft.com/semantic-kernel/concepts/vector-store-connectors/)
  - [Microsoft Research GraphRAG Overview](https://learn.microsoft.com/azure/developer/ai/intro-graphrag)
- **Projeto Integrador 04**:
  - *Nome*: `EnterpriseKnowledge.AdvancedRag`
  - *Escopo*: Mecanismo completo de RAG corporativo sobre documentações técnicas complexas. O sistema realiza ingestão hierárquica (Parent-Child), executa busca híbrida (vetorial + lexical) com mesclagem RRF, aplica um modelo Cross-Encoder para reordenar os top 5 resultados mais precisos e entrega a resposta citando fontes verificadas. O projeto acompanha uma suíte de testes automatizados com `Microsoft.Extensions.AI.Evaluation` que calcula a nota de *Groundedness* e reprova o build se houver alucinação.

---

### Módulo 05: IA Local Soberana com Ollama vs. Nuvem: Análise de Trade-offs e Estratégia Híbrida
- **Contexto & Metas**: Executar LLMs locais (Phi-3/4, Llama 3, DeepSeek) via Ollama integrados nativamente ao .NET; benchmarking comparativo: privacidade total e latência local vs. poder computacional e concorrência na nuvem; arquitetura híbrida (local-first com transição transparente para nuvem).
- **Abstrações & Frameworks**: `Microsoft.Extensions.AI.Ollama`, Ollama API, Docker.
- **Princípios & Padrões**: Liskov Substitution Principle (LSP - alternar de Ollama para Azure sem quebrar as regras de domínio), Interface Segregation.
- **Referências Microsoft Learn**:
  - [Uso do Ollama Chat Client no .NET](https://learn.microsoft.com/dotnet/ai/ichatclient#use-an-ollama-chat-client)
- **Projeto Integrador 05**:
  - *Nome*: `HybridLocalCloud.Engine`
  - *Escopo*: Sistema de processamento documental que roda localmente via Ollama (`phi-4` ou `llama3.2`). Quando uma demanda exige raciocínio complexo que excede os recursos locais, a aplicação solicita confirmação e despacha os dados higienizados para o provedor em nuvem, emitindo relatório comparativo de VRAM/RAM vs tokens tarifados.

---

### Módulo 06: Engenharia de Prompts Avançada, Notações Estruturadas (JSON/YAML/TON) e Multimodalidade
- **Contexto & Metas**: 
  - Técnicas de metaprompting, Few-Shot, Chain-of-Thought (CoT) e separação estrita de mensagens de sistema e usuário;
  - **Arquitetura de Instruções Estruturadas**: Comparativo rigoroso entre Prosa Livre vs Markdown vs Tags XML vs Schemas Estruturados (JSON e YAML);
  - **Token Object Notation (TON)**: Fundamentos do TON como padrão compacto desenhado para maximizar a densidade de informação para LLMs, eliminando delimiters e caracteres redundantes de JSON, reduzindo o custo de tokens em até 30% a 40% e preservando tipagem e hierarquia;
  - **Benchmarking de Aderência a Restrições (*Negative Constraint Adherence*)**: Como formatos estruturados (JSON/YAML/TON) mitigam a atenção difusa (*attention drift*) e garantem obediência estrita a regras de negócio e limites de segurança;
  - **Structured Outputs no .NET**: Geração determinística de saídas com `ChatResponseFormat.ForJsonSchema<T>`, validação estrita com `System.Text.Json.Schema` e DTOs fortemente tipados em C#;
  - **Multimodalidade**: Geração e análise multimodal integrando texto, imagem, áudio/transcrição (Whisper) e visão computacional.
- **Abstrações & Frameworks**: `ChatResponseFormat.ForJsonSchema<T>`, `System.Text.Json.Schema`, Azure AI Speech SDK, Serializadores customizados de TON/YAML para C#.
- **Princípios & Padrões**: Type Safety, Data Transfer Objects (DTOs) fortemente tipados, Twelve-Factor Agent: Factor 4 (Structured I/O & Formatos TON/JSON).
- **Referências Microsoft Learn**:
  - [Structured Output com IChatClient](https://learn.microsoft.com/dotnet/ai/ichatclient#structured-output)
  - [Saídas Estruturadas com Azure OpenAI](https://learn.microsoft.com/azure/ai-services/openai/how-to/structured-outputs)
- **Projeto Integrador 06**:
  - *Nome*: `PromptArchitectAndTriage.Service`
  - *Escopo*: Sistema de triagem corporativa que ingere múltiplos formatos de entrada (áudio com depoimento do cliente via Whisper, imagem de dano via modelo de visão) e gera um laudo pericial validado contra um C# `record SinistroReport(...)`. O projeto inclui um benchmark automatizado que compara a execução do System Prompt nos formatos Prosa, JSON, YAML e TON, gerando um relatório em tabela com: tokens consumidos no prompt, tempo de resposta e taxa de conformidade com 10 restrições de negócio complexas.

---

### Módulo 07: Governança, Contratos de Uso, Privacidade e Zero Data Retention
- **Contexto & Metas**: Análise profunda dos termos de serviço dos provedores (OpenAI Enterprise, Azure OpenAI Data & Privacy, Anthropic Commercial Terms); auditoria de tráfego de dados, políticas de retenção temporária e desativação de treinamento em dados de clientes (Zero Data Retention / HIPAA / GDPR).
- **Abstrações & Frameworks**: Azure OpenAI Customer Managed Keys (CMK), Virtual Network (VNet) endpoints, DLP (Data Loss Prevention) patterns.
- **Princípios & Padrões**: Privacy by Design, Zero Trust Architecture, Twelve-Factor: Factor X (Dev/Prod Parity de segurança).
- **Referências Microsoft Learn**:
  - [Dados, Privacidade e Segurança no Azure OpenAI](https://learn.microsoft.com/legal/cognitive-services/openai/data-privacy)
  - [Linha de Base de Segurança do Azure para Serviços de IA](https://learn.microsoft.com/azure/ai-services/openai/concepts/security-baseline)
- **Projeto Integrador 07**:
  - *Nome*: `DataPrivacyAuditor.Tool`
  - *Escopo*: Ferramenta de auditoria de conformidade que intercepta chamadas de IA via middleware HTTP, inspeciona headers (verificando se o endpoint respeita flags de não retenção de dados), valida se dados sensíveis estão sendo enviados para regiões e endpoints homologados e emite relatório alinhado ao Azure Trust Center.

---

### Módulo 08: Observabilidade Transversal, FinOps de Tokens e Dashboards com OpenTelemetry
- **Contexto & Metas**: Telemetria ponta a ponta com OpenTelemetry (Métricas, Logs Estruturados, Traces distribuídos de GenAI); rastreamento de consumo de tokens (prompt, completion), latência por modelo, contagem de sessões e usuários; exportação para painéis (.NET Aspire Dashboard, Prometheus/Grafana ou Azure Monitor).
- **Abstrações & Frameworks**: `OpenTelemetry`, `Microsoft.Extensions.AI` (`UseOpenTelemetry`), .NET Aspire, `System.Diagnostics.Activity`.
- **Princípios & Padrões**: Observability as Code, Twelve-Factor: Factor XI (Logs como Streams), Twelve-Factor Agent: Factor 8 (Comprehensive Observability & Traceability).
- **Referências Microsoft Learn**:
  - [Telemetria e Métricas em IChatClient](https://learn.microsoft.com/dotnet/ai/ichatclient#telemetry)
  - [Telemetria no .NET Aspire](https://learn.microsoft.com/dotnet/aspire/fundamentals/telemetry)
- **Projeto Integrador 08**:
  - *Nome*: `GenAiObservability.Dashboard`
  - *Escopo*: Aplicação orquestrada com .NET Aspire utilizando `client.AsBuilder().UseOpenTelemetry().Build()`. Apresenta no painel do Aspire todas as chamadas de LLM, custo acumulado por tenant/usuário, taxas de erro, rastreabilidade em cascata das chamadas de ferramentas e gatilhos de FinOps quando o orçamento diário for atingido.

---

### Módulo 09: Segurança Ofensiva e Defensiva: OWASP Top 10 for LLM, Prompt Injection e Guardrails
- **Contexto & Metas**: Ameaças clássicas (Direct/Indirect Prompt Injection, Jailbreaking, System Prompt Extraction, Insecure Output Handling); higienização e isolamento rigoroso de contexto de sistema vs dados não confiáveis; moderação de conteúdo e guardrails programáticos.
- **Abstrações & Frameworks**: Azure AI Content Safety SDK, bibliotecas de Guardrails em C#, Regex/Semantic Pre-validators.
- **Princípios & Padrões**: Defense in Depth, Fail-Safe Defaults, Twelve-Factor Agent: Factor 5 (Deterministic Guardrails vs Stochastic LLM).
- **Referências Microsoft Learn**:
  - [Visão Geral do Azure AI Content Safety](https://learn.microsoft.com/azure/ai-services/content-safety/overview)
  - [Filtragem de Conteúdo no Azure OpenAI](https://learn.microsoft.com/azure/ai-services/openai/concepts/content-filter)
- **Projeto Integrador 09**:
  - *Nome*: `AiFirewall.Middleware`
  - *Escopo*: Um `DelegatingChatClient` que atua como Web Application Firewall (WAF) para chamadas de IA. Analisa mensagens de entrada contra padrões de injeção direta e testa a carga contra a API do Azure Content Safety. No retorno da resposta, intercepta e mascara chaves de API ou PII (dados pessoais) vazados acidentalmente.

---

### Módulo 10: Machine Learning Clássico com ML.NET: Modelos Preditivos e Hibridismo com GenAI
- **Contexto & Metas**: Ingestão e pipeline de dados (`IDataView`), treinamento, validação e predição em C# sem dependência de Python; algoritmos de regressão, classificação multiclasse e detecção de anomalias em logs e documentos; uso de modelos ML.NET como roteadores de intenção ultrarrápidos e econômicos antes de invocar LLMs.
- **Abstrações & Frameworks**: `Microsoft.ML`, `MLContext`, `ITransformer`, `PredictionEnginePool`.
- **Princípios & Padrões**: Data Pipeline Pattern, Pre-filtering Pattern (Early Termination Pattern).
- **Referências Microsoft Learn**:
  - [Documentação Oficial do ML.NET](https://learn.microsoft.com/dotnet/machine-learning/)
  - [Tutorial de Regressão e Classificação com ML.NET](https://learn.microsoft.com/dotnet/machine-learning/tutorials/predict-prices)
- **Projeto Integrador 10**:
  - *Nome*: `SmartSupportTriage.Hybrid`
  - *Escopo*: Pipeline de triagem de tickets: um modelo treinado com ML.NET analisa o ticket recebido e prediz a severidade e categoria em microssegundos (on-premises). Se o problema for comum, despacha uma resposta padrão; se for anômalo ou complexo, enriquece o contexto e aciona o pipeline de LLM para resolução detalhada.

---

### Módulo 11: Extensibilidade com Model Context Protocol (MCP) em .NET
- **Contexto & Metas**: O protocolo MCP como padrão aberto de interoperabilidade para conectar LLMs a ferramentas, bases de dados e APIs corporativas; criação de servidores MCP e clientes MCP nativos em C#; injeção dinâmica de ferramentas em pipelines de chat.
- **Abstrações & Frameworks**: Protocolo MCP (JSON-RPC / SSE / stdio), `Microsoft.Extensions.AI` com Tool Calling / Function Invocation, SDKs comunitários e oficiais de MCP em C#.
- **Princípios & Padrões**: Adapter Pattern, Inversion of Control, Twelve-Factor Agent: Factor 6 (Tool Abstraction & MCP).
- **Referências Microsoft Learn**:
  - [Tools e Integrações no Agent Framework](https://learn.microsoft.com/agent-framework/agents/tools/)
  - [Invocação de Funções e Ferramentas com IChatClient](https://learn.microsoft.com/dotnet/ai/ichatclient#tool-calling)
- **Projeto Integrador 11**:
  - *Nome*: `EnterpriseMcpServer.Connector`
  - *Escopo*: Criação de um servidor MCP em .NET que disponibiliza recursos de banco de dados SQL e endpoints REST corporativos com esquemas padronizados. Em seguida, criação de um cliente em C# com `IChatClient` que consome as ferramentas desse servidor MCP via SSE, permitindo que a LLM execute consultas analíticas seguras e auditadas.

---

### Módulo 12: Agentes de IA com Microsoft Agent Framework (MAF), Semantic Kernel e Declarative Agents
- **Contexto & Metas**: 
  - Conceito formal de Agente (Persona, Ferramentas, Memória, Decisão Autônoma);
  - O ecossistema do **Microsoft Agent Framework** (`Microsoft.Agents.AI`) e interoperabilidade com `Microsoft.SemanticKernel.Agents`;
  - **Agentes Declarativos (Declarative Agents)**: Modelagem de agentes baseados no padrão **Declarative Agent Manifest (JSON Schema / YAML / TON specs)** da Microsoft, separando a especificação do comportamento da implementação de execução em C#;
  - Ciclo de vida do agente, isolamento de threads e padrões de **Harness Agent** (planejamento autônomo, rastreamento de tarefas pendentes, compactação de contexto e aprovação de ferramentas);
  - Garantia de governança corporativa com **Human-in-the-Loop (HITL)** para ações sensíveis.
- **Abstrações & Frameworks**: `Microsoft.Agents.AI` (`AIAgent`, `AIProjectClient`), `Microsoft.SemanticKernel.Agents`, Schemas Declarativos (JSON/YAML/TON).
- **Princípios & Padrões**: Autonomous Agent Pattern, Human-in-the-loop (HITL), Declarative Specification Pattern, Twelve-Factor Agent: Factor 11 (Human-in-the-Loop & Approval Gates) e Factor 12 (Declarative Specifications).
- **Referências Microsoft Learn**:
  - [Visão Geral do Microsoft Agent Framework](https://learn.microsoft.com/agent-framework/overview/)
  - [Conceitos de Harness Agent](https://learn.microsoft.com/agent-framework/concepts/harness)
  - [Esquema do Declarative Agent Manifest da Microsoft](https://learn.microsoft.com/microsoft-365/copilot/extensibility/declarative-agent-manifest-1.8)
- **Projeto Integrador 12**:
  - *Nome*: `AutonomousDevAssistant.Agent`
  - *Escopo*: Agente inteligente cuja persona, ferramentas e regras de governança são carregadas dinamicamente a partir de um manifesto declarativo estruturado (JSON/YAML/TON). O agente recebe uma demanda de desenvolvimento em linguagem natural, gera plano de trabalho em memória, executa inspeção de repositório e alteração de arquivos de código via MCP, e antes de qualquer comando de alto risco (ex: commit ou deploy), pausa o runtime e solicita confirmação explícita do desenvolvedor (Human-in-the-Loop).

---

### Módulo 13: Orquestração Multi-Agente Avançada: Workflows em Grafo e Execução Paralela/Sequencial
- **Contexto & Metas**: Orquestração multi-agente determinística vs estocástica; Workflows baseados em Grafos do Microsoft Agent Framework; padrões sequenciais, paralelos (fan-out/fan-in) e orquestração dinâmica inspirada no Magentic-One; garantia de isolamento de estado com executores (`IResettableExecutor`).
- **Abstrações & Frameworks**: `Microsoft.Agents.AI.Workflows`, Workflow Builders, Graph State Management, `IResettableExecutor`.
- **Princípios & Padrões**: Saga Pattern, Directed Acyclic Graph (DAG), Event-Driven Architecture, Twelve-Factor Agent: Factor 3 (Event-Driven & Graph State Transitions).
- **Referências Microsoft Learn**:
  - [Isolamento de Estado em Workflows do MAF](https://learn.microsoft.com/agent-framework/concepts/workflows/state)
  - [Orquestração Magentic no Agent Framework](https://learn.microsoft.com/agent-framework/workflows/orchestrations/magentic)
- **Projeto Integrador 13**:
  - *Nome*: `EnterpriseAudit.MultiAgentWorkflow`
  - *Escopo*: Workflow orquestrado composto por 3 agentes especialistas (Pesquisador de Regulamentações, Analista de Dados Financeiros e Auditor de Conformidade) coordenados por um Agente Gerente. O fluxo executa análises de dados em paralelo, consolida as inconsistências através de um grafo com branches condicionais e produz um relatório final consolidado com isolamento total de estado entre execuções concorrentes.

---

### Módulo 14: Design by Contract (DbC) para Agentes Autônomos em C#
- **Contexto & Metas**:
  - Transposição do princípio clássico de **Design by Contract (DbC)** de Bertrand Meyer para a arquitetura de Agentes de IA;
  - Decomposição formal de instruções: **Pré-condições** (validação de entradas e estado inicial), **Invariantes** (regras de negócio estritas que o agente jamais pode violar durante o ciclo) e **Critérios de Aceite / Pós-condições** (*Definition of Done* determinística);
  - Parsing e compilação de contratos declarativos em arquivos Markdown com Frontmatter (`*.agent.md`) usando `Markdig` e `YamlDotNet` em objetos C# fortemente tipados;
  - Motor de validação determinística de pós-condições e ciclos de auto-correção (*Self-Correction / Retry Loops*) quando a LLM gera saídas que violam os critérios de aceite.
- **Abstrações & Frameworks**: `Markdig` (com extensão `UseYamlFrontMatter`), `YamlDotNet`, `System.Text.Json.Schema`, `Microsoft.Extensions.AI`.
- **Princípios & Padrões**: Design by Contract (DbC), Liskov Substitution Principle (LSP), Open/Closed Principle (OCP), Twelve-Factor Agent: Factor 1 (Lógica Determinística vs Modelo Probabilístico) e Factor 5 (Guardrails Determinísticos).
- **Referências Microsoft Learn**:
  - [Arquitetura de Extensibilidade e Injeção de Dependências no .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection)
  - [Conceitos de Harness Agent e Validação](https://learn.microsoft.com/agent-framework/concepts/harness)
- **Projetos Integradores 14**:
  - *Projeto 14.1*: `DbcContractParser.Core` — Biblioteca e CLI que realiza o parsing de arquivos `*.agent.md`, validando a sintaxe das pré-condições, invariantes e critérios de aceite em objetos tipados `record AgentContract(...)`.
  - *Projeto 14.2*: `ContractEnforcer.Runner` — Agente de análise de contratos jurídicos e tributários cuja execução é submetida a um validador determinístico de pós-condição. Se a LLM alucinar ou omitir uma cláusula exigida no contrato Markdown, o runtime rejeita a finalização e reexecuta a etapa injetando o erro contratual como feedback.

---

### Módulo 15: Grand Master Capstone — The Enterprise Agentic Platform
- **Contexto & Metas**:
  - Unificação definitiva de **TODAS as disciplinas da trilha (Módulos 01 a 14)** em uma solução enterprise de nível de produção (*Production-Ready Enterprise Platform*);
  - Construção de uma **Plataforma Autônoma de Agentes** em C# .NET que atua como host agnóstico de microsserviços de IA para toda a organização;
  - Suporte a *Hot-Reload* e Multi-Tenancy: publicação e atualização de agentes em produção simplesmente adicionando novos arquivos de contrato `*.agent.md` ou grafos `*.workflow.md` em repositórios Git monitorados, sem necessidade de recompilar o código C#;
  - Integração de ponta a ponta da infraestrutura:
    - Gateway unificado com resiliência Polly (Módulos 1 e 2);
    - Gestão de sessões persistentes no Redis (Módulo 3);
    - Mecanismo RAG Híbrido com GraphRAG e validação de Groundedness (Módulo 4);
    - Execução híbrida Ollama local + Nuvem Azure/OpenRouter (Módulo 5);
    - Otimização de tokens com notações TON e Structured Outputs (Módulo 6);
    - Auditoria de dados e Zero Data Retention (Módulo 7);
    - Observabilidade total com OpenTelemetry no .NET Aspire Dashboard (Módulo 8);
    - WAF de IA e proteção contra Prompt Injection (Módulo 9);
    - Pré-filtro preditivo com ML.NET (Módulo 10);
    - Ferramentas distribuídas dinamicamente via Model Context Protocol (Módulo 11);
    - Orquestração de Agentes do MAF e Workflows em Grafo (Módulos 12 e 13);
    - Governança estrita orientada a contratos com Design by Contract (Módulo 14).
- **Abstrações & Frameworks**: Consolidação do stack completo (`Microsoft.Agents.AI`, `Microsoft.Extensions.AI`, `Microsoft.Extensions.AI.Evaluation`, `OpenTelemetry`, `Polly`, `.NET Aspire`, `ML.NET`, `MCP`, `StackExchange.Redis`, `Markdig`).
- **Princípios & Padrões**: The Twelve-Factor App & The Twelve-Factor Agent completos, Clean Architecture / DDD, Enterprise Integration Patterns, Zero Trust Architecture.
- **Projeto Integrador 15 (Grand Finale Capstone)**:
  - *Nome*: `EnterpriseAgenticPlatform.Host`
  - *Escopo*: A plataforma corporativa definitiva em .NET. Uma solução distribuída orquestrada com .NET Aspire que sobe um cluster de execução de agentes, provê um portal web para visualização de métricas de FinOps e auditoria de contratos, expõe um catálogo de ferramentas MCP corporativas e executa fluxos complexos de negócios (ex: *Onboarding Automatizado de Fornecedores* envolvendo auditoria jurídica, fiscal e de segurança) guiados 100% por contratos declarativos em Markdown com garantia matemática e determinística de cumprimento de regras.

---

## 2. Como Utilizar este Repositório

Cada módulo acima foi redigido com metadados e fronteiras bem definidas para que você possa utilizá-lo como um roteiro iterativo de estudos:

1. **Geração de Guias de Estudo e Especificação**: Utilize o **Meta-Prompt da Seção 3** para gerar um documento Markdown dedicado em `docs/modulos/modulo-XX-<nome>/GUIA_DE_ESTUDO.md` para o módulo escolhido. Esse guia conterá toda a teoria aprofundada, checkpoints de aprendizado e a especificação arquitetural do projeto integrador.
2. **Estudo e Validação dos Checkpoints**: Leia o guia gerado, consulte os links do Microsoft Learn e valide cada checkpoint conceitual antes de escrever código.
3. **Construção do Projeto Integrador**: Utilize a especificação técnica do guia para implementar o software em C# (.NET 10 LTS) na pasta correspondente em `src/ModuloXX_<NomeDoProjeto>`.
4. **Evolução Contínua**: Cada projeto integrador implementa testes unitários/integração, observabilidade e boas práticas de engenharia de software de forma independente e incremental.

---

## 3. Meta-Prompt: Gerador de Guias de Estudo e Especificação Arquitetural

Copie e envie o prompt abaixo para a sua LLM (ou execute nesta mesma sessão), informando qual o módulo desejado (por exemplo: `"Execute o Meta-Prompt para o Módulo 01"`).

A LLM irá gerar um documento Markdown completo e aprofundado na pasta `docs/modulos/modulo-XX-<nome>/GUIA_DE_ESTUDO.md`, servindo como a sua apostila técnica e especificação de engenharia para o módulo em questão.

````markdown
# SYSTEM PROMPT: ENGENHEIRO DE SOFTWARE & INSTRUTOR DE IA (.NET / C#)

## PAPEL E OBJETIVO
Você é um Arquiteto de Software Sênior e Especialista em Inteligência Artificial no ecossistema .NET.
Sua missão é ler a ementa de um módulo específico do repositório **MafMastery (README.md)** e gerar um documento Markdown completo, exaustivo e rigoroso em:
`docs/modulos/modulo-{{NUMERO_DO_MODULO}}-{{NOME_DO_MODULO}}/GUIA_DE_ESTUDO.md`

Este documento NÃO deve conter o código final do projeto implementado, mas sim:
1. O material teórico de estudo aprofundado;
2. Os checkpoints cognitivos de autoavaliação;
3. A especificação arquitetural completa e técnica para guiar o desenvolvimento do Projeto Integrador do módulo.

---

## DIRETRIZES DE DESIGN E ENGENHARIA
Todo o conteúdo deve refletir os quatro pilares do MafMastery:
- **Clean Architecture & DDD**: Separação clara de responsabilidades (Domain, Application, Infrastructure, Presentation).
- **Princípios SOLID**: Demonstração explícita de como SRP, OCP, LSP, ISP e DIP são aplicados aos componentes de IA deste módulo.
- **Design Patterns Clássicos**: Identificação e especificação dos padrões aplicados (ex: Decorator via DelegatingChatClient, Strategy, Factory, Adapter via MCP, Saga/DAG).
- **The Twelve-Factor App & The Twelve-Factor Agent**: Aplicação prática dos fatores relevantes (Stateless Runtime, Structured I/O com TON/JSON, Backing Services, Observability as Code, Guardrails Determinísticos, Design by Contract).
- **Tecnologia Alvo**: .NET 10 (LTS) e C# moderno, utilizando preferencialmente abstrações oficiais da Microsoft (`Microsoft.Extensions.AI`, `Microsoft.Agents.AI`, `Microsoft.Extensions.AI.Evaluation`, `OpenTelemetry`, etc.).

---

## ESTRUTURA OBRIGATÓRIA DO ARQUIVO `GUIA_DE_ESTUDO.md`

O documento gerado deve conter estritamente as seguintes seções estruturadas:

### # Módulo {{NUMERO}}: {{NOME DO MÓDULO}} — Guia de Estudo e Especificação de Engenharia

#### 1. Visão Geral e Metas de Aprendizado
- Resumo executivo do problema real de mercado que este módulo resolve.
- Habilidades práticas que o desenvolvedor terá ao concluir o estudo e o projeto.

#### 2. Fundamentação Teórica Aprofundada
- Detalhamento conceitual exaustivo de cada tópico da ementa.
- Análise de funcionamento interno no runtime .NET e nas chamadas de IA (alocação de memória, concorrência assíncrona com `Task`/`IAsyncEnumerable`, fluxos de streaming).
- Trade-offs técnicos, vantagens, desvantagens e armadilhas comuns em produção (*Gotchas & Anti-patterns*).

#### 3. Mapeamento de Princípios e Padrões Arquiteturais
- **SOLID no Módulo**: Tabela ou tópicos detalhando onde cada um dos 5 princípios atua neste cenário.
- **Design Patterns Aplicados**: Padrões adotados e justificativa arquitetural.
- **Twelve-Factor App & Agent**: Quais fatores específicos são atendidos e como garanti-los.
- **Diagrama Arquitetural**: Diagrama conceitual em Mermaid (`graph TD` ou `sequenceDiagram`) ilustrando a interação entre o domínio, o pipeline de IA e serviços externos.

#### 4. Checkpoints Cognitivos de Aprendizagem
- Lista de 5 a 8 perguntas conceituais e desafios de reflexão técnica com as respectivas respostas e explicações de gabarito para autoavaliação antes de iniciar o código.

#### 5. Leituras Recomendadas & Grounding no Microsoft Learn
- Lista curada com links oficiais da Microsoft (documentações de APIs, tutoriais de arquitetura e pacotes NuGet oficiais).

#### 6. Especificação Técnica do Projeto Integrador
- **Nome do Projeto**: Nome padronizado conforme o README.
- **Escopo e Problema de Negócio**: Cenário corporativo simulado.
- **Requisitos Funcionais (RFs)**: Lista numerada (RF-01, RF-02...) com as capacidades exigidas.
- **Requisitos Não-Funcionais (RNFs)**: Latência, resiliência, observabilidade, desacoplamento e limites de alocação de memória.
- **Estrutura de Projetos e Pastas (Clean Architecture)**:
  - Exemplo: `Domain`, `Application`, `Infrastructure`, `Presentation`.
- **Contratos de Interfaces e Entidades Principais**:
  - Código C# demonstrando apenas os contratos essenciais (interfaces, records, DTOs e assinaturas de métodos), sem implementação concreta de regras de negócio.
- **Critérios de Aceite & Definição de Pronto (Definition of Done - DoD)**:
  - Checklist formal para considerar o projeto concluído com sucesso.
- **Plano Mínimo de Testes**:
  - Cenários de Testes Unitários (mocks com xUnit/Moq/NSubstitute) e Testes de Integração requeridos.

#### 7. Roteiro Passo a Passo de Construção (Build Roadmap)
- Sequência lógica e ordenada (Etapa 1 a Etapa N) recomendada para o desenvolvedor sentar no teclado e construir o software de forma incremental e testável.
````