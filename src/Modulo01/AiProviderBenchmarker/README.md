# Módulo 01: AiProviderBenchmarker — Comparador de Provedores de IA

> **Projeto Integrador do Módulo 01** do programa **MafMastery**.  
> Implementado em **.NET 10 (LTS)** e **C# 14**, baseado na biblioteca oficial **`Microsoft.Extensions.AI` (MEAI)**, **Clean Architecture**, princípios **Twelve-Factor App & Agent** e interface rica com **Spectre.Console**.

---

## 🎯 Objetivo do Projeto

O **AiProviderBenchmarker** é uma ferramenta de linha de comando (CLI) enterprise projetada para submeter simultaneamente o mesmo prompt a múltiplos provedores de Modelos de Linguagem (LLMs) — tanto em nuvem comercial (OpenAI, Azure OpenAI) quanto locais (Ollama) e simulados offline —, mensurando e comparando com rigor:

1. **TTFT (Time To First Token)**: Tempo decorrido até a chegada do primeiríssimo token via streaming (medida fundamental de percepção de velocidade de resposta).
2. **Latência Total**: Duração total da geração de ponta a ponta.
3. **Throughput de Geração (TPS)**: Taxa real de tokens de saída por segundo gerados pelo modelo, expurgando o TTFT da equação para isolar a capacidade do motor de inferência.
4. **Estimativa FinOps ($ USD)**: Cálculo do custo financeiro da inferência baseado no consumo de tokens de entrada/saída contra tabelas públicas de precificação por milhão de tokens.

---

## 🏛️ Arquitetura da Solução

A solução segue rigorosamente os princípios de **Clean Architecture**, com separação clara de responsabilidades:

```
src/Modulo01/AiProviderBenchmarker/
├── AiProviderBenchmarker.slnx              # Arquivo de solução moderno do .NET 10
├── src/
│   ├── AiProviderBenchmarker.Domain/        # Camada de Domínio: regras puras, sem I/O ou frameworks
│   │   ├── Model/
│   │   │   ├── ProviderType.cs              # Enum: OpenAi, AzureOpenAi, Ollama, Simulated
│   │   │   ├── ModelPricing.cs              # Value Object de FinOps (custo por 1M tokens)
│   │   │   └── ProviderMetric.cs            # Entidade/DTO de resultado com cálculo de TPS
│   │   └── Services/
│   │       └── ICostEstimator.cs            # Contrato para estimativa de custos de inferência
│   │
│   ├── AiProviderBenchmarker.Application/   # Casos de Uso e Orquestração
│   │   ├── Common/
│   │   │   └── IChatClientFactory.cs        # Contrato de fábrica para abstração de IChatClient
│   │   └── UseCases/
│   │       ├── RunBenchmarkCommand.cs       # DTO de entrada do benchmark
│   │       ├── IRunBenchmarkUseCase.cs      # Contrato do caso de uso
│   │       └── RunBenchmarkHandler.cs       # Disparo paralelo (Task.WhenAll), TTFT & resiliência
│   │
│   ├── AiProviderBenchmarker.Infrastructure/ # Adaptadores externos de IA e Precificação
│   │   ├── Configuration/
│   │   │   ├── AiProviderConfig.cs      # Modelo base extensível com AiProviderName
│   │   │   └── AiProvidersOptions.cs    # Coleção de provedores (List<AiProviderConfig>)
│   │   ├── Mock/
│   │   │   └── SimulatedChatClient.cs       # Implementação IChatClient offline e determinística
│   │   ├── Factories/
│   │   │   ├── Strategies/                  # Padrão Strategy (SOLID/OCP) para instanciação de clientes
│   │   │   │   ├── IChatClientStrategy.cs   # Contrato da estratégia
│   │   │   │   ├── SimulatedClientStrategy.cs # Estratégia para motor simulado e fallback
│   │   │   │   ├── AzureOpenAiClientStrategy.cs # Estratégia para Azure OpenAI
│   │   │   │   └── OpenAiCompatibleClientStrategy.cs # Estratégia para OpenAI / Ollama / Gemini / Grok
│   │   │   └── ChatClientFactory.cs         # Fábrica desacoplada orquestradora (Strategy Pattern)
│   │   └── Pricing/
│   │       └── CostEstimator.cs             # Estimativa dinâmica de custos baseada em appsettings.json
│   │
│   └── AiProviderBenchmarker.Cli/           # Interface de Apresentação (Console)
│       ├── appsettings.json                 # Configurações de modelos, endpoints e credenciais
│       ├── Configuration/
│       │   └── ServiceCollectionExtensions.cs # Injeção de Dependência (IoC) e Configuration
│       ├── Arguments/
│       │   ├── CliParsedOptions.cs          # Modelo com opções parseadas da linha de comando
│       │   └── CliArgumentParser.cs         # Parser isolado de flags (-p, --providers, -m, -h)
│       ├── Runners/
│       │   ├── IBenchmarkRunner.cs          # Contrato de executor
│       │   ├── InteractiveBenchmarkRunner.cs # Fluxo interativo guiado (Spectre.Console)
│       │   ├── NonInteractiveBenchmarkRunner.cs # Fluxo autônomo para CI/CD
│       │   └── CliApp.cs                    # Orquestrador de alto nível da aplicação
│       ├── UI/
│       │   ├── TableRenderer.cs             # Renderização de tabelas, árvore, pódio e status
│       │   └── CliHelpRenderer.cs           # Renderização da tela de ajuda (--help)
│       └── Program.cs                       # Entrypoint minimalista (< 35 linhas)
│
└── tests/
    └── AiProviderBenchmarker.Tests/         # Testes automatizados (xUnit, FluentAssertions, NSubstitute)
        ├── Domain/                          # Testes unitários das fórmulas de métricas (TPS)
        ├── Infrastructure/                  # Testes do catálogo de custos e precificação
        └── Application/                     # Testes de concorrência e tolerância a falhas
```

---

## ⚡ Como Executar

### ⚠️ Regra Importante sobre Argumentos no `dotnet run`

Ao executar uma aplicação de console com o `dotnet run`, **é obrigatório utilizar o separador `--` antes das opções da sua aplicação**.  
Caso contrário, o utilitário `dotnet run` interceptará argumentos como `--help`, `-p` ou `--providers` como se fossem opções do próprio SDK do .NET!

```bash
# ❌ INCORRETO (o dotnet run intercepta a flag e exibe o help do próprio .NET):
dotnet run --project src/Modulo01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli --help

# ✔️ CORRETO (os argumentos após '--' são entregues diretamente à aplicação AiProviderBenchmarker):
dotnet run --project src/Modulo01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- --help
```

---

### 1. Modo Interativo (com Menus e Seletores)

Se nenhum argumento for passado e o terminal for interativo (TTY), a aplicação inicia um assistente interativo guiado pelo `Spectre.Console`:

```bash
dotnet run --project src/Modulo01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli
```

No modo interativo você pode:
- Escolher entre prompts de teste pré-definidos (Programação C# .NET 10, Análise Arquitetural, Tradução Técnica) ou digitar livremente um prompt customizado.
- Selecionar com a barra de espaço quais provedores deseja incluir na rodada.
- Definir o `max_tokens` de saída desejado.
- Repetir novas rodadas consecutivas sem sair da aplicação.

---

### 2. Modo Não-Interativo / Linha de Comando (CI/CD ou Automação)

Ideal para scripts, pipelines de CI ou comparações rápidas:

```bash
dotnet run --project src/Modulo01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- [argumentos]
```

#### Tabela de Argumentos da CLI:

| Argumento | Forma Curta | Descrição | Valor Padrão | Exemplo |
| :--- | :--- | :--- | :--- | :--- |
| `--prompt <texto>` | `-p` | Texto do prompt a ser enviado aos modelos | Pergunta pré-definida de C# 14 | `-p "Explique Clean Architecture"` |
| `--providers <lista>` | *(nenhuma)* | Provedores separados por vírgula | `Simulated` (+ provedores ativos) | `--providers Ollama,Simulated` |
| `--max-tokens <num>` | `-m` | Limite máximo de tokens de saída | `300` | `-m 200` |
| `--help` | `-h` | Exibe a tela de ajuda da aplicação | *(nenhum)* | `--help` |

#### Provedores Suportados na flag `--providers`:
- `Simulated`: Motor embutido offline (mock determinístico de alta fidelidade; não requer internet ou chaves de API).
- `Ollama`: Motor local via API compatível OpenAI (ex: `http://localhost:11434/v1` com modelo local, e.g., `llama3.2:3b`, `phi4`).
- `OpenAi`: API da OpenAI (utiliza a chave configurada em `appsettings.json` ou variável de ambiente).
- `AzureOpenAi`: Recurso Azure OpenAI Service (utiliza endpoint e chave de implantação).
- `Gemini`: Google Gemini via endpoint OpenAI-compatible (`https://generativelanguage.googleapis.com/v1beta/openai/`).
- `Grok`: xAI Grok via endpoint OpenAI-compatible (`https://api.x.ai/v1`).
- *Qualquer outro provedor*: Qualquer provedor cadastrado na lista `Providers` do `appsettings.json` com `AiProviderName` pode ser referenciado diretamente pelo seu nome!

---

### Exemplos Práticos de Execução

#### Exemplo A: Testar Provedores Locais e Offline (Ollama e Simulado)
```bash
dotnet run --project src/Modulo01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- --providers Ollama,Simulated --max-tokens 200
```

#### Exemplo B: Executar com Prompt Customizado
```bash
dotnet run --project src/Modulo01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- -p "Escreva um exemplo de padrão Factory em C# 14 com pattern matching." --providers Simulated,OpenAi -m 250
```

#### Exemplo C: Exibir a Ajuda da CLI
```bash
dotnet run --project src/Modulo01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- --help
```

---

## ⚙️ Configuração dos Provedores (`appsettings.json`)

As configurações de conexão e modelos residem em `src/AiProviderBenchmarker.Cli/appsettings.json` e podem ser sobrescritas por Variáveis de Ambiente:

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

> 💡 **Nota sobre Resiliência**: Se a chave do AI Provider não for preenchida, a aplicação aciona de forma transparente o **Fallback Simulado**, permitindo que você execute e demonstre a ferramenta sem erros inesperados.

---

## 📊 Entendendo as Métricas do Relatório

Ao término de cada rodada, a aplicação gera uma tabela consolidada com as seguintes colunas:

| Métrica | Significado e Cálculo |
| :--- | :--- |
| **Status** | Indica `✔ OK` para execuções bem-sucedidas ou `✖ Falha` caso o provedor tenha retornado erro (timeout, credencial ausente, etc.). |
| **TTFT** | **Time To First Token**: Cronometrado desde o envio da requisição até a recepção do primeiro fragmento de streaming (`update.Text`). |
| **Latência** | Duração total de ponta a ponta da execução da chamada. |
| **Tokens E/S** | Tokens estimados de Entrada (Input Prompt) e de Saída (Output Completion). |
| **TPS (Tokens/s)** | Throughput real de geração: $$\text{TPS} = \frac{\text{Tokens de Saída}}{\text{Latência Total} - \text{TTFT}}$$ |
| **Custo ($ USD)** | Estimativa monetária baseada na tabela oficial por 1 milhão de tokens. Provedores locais como **Ollama** sempre reportam `$0.000000`. |

Abaixo da tabela, a CLI exibe:
- **Prévia das Respostas**: Árvore com os primeiros caracteres da resposta sintetizada de cada modelo.
- **Pódio do Benchmark**: Cartões destacando o campeão de menor TTFT (velocidade inicial), o campeão de maior Throughput (taxa de geração) e a opção mais econômica em custos (FinOps).

---

## 🧪 Suíte de Testes Automatizados

A suíte de testes unitários foi desenvolvida com **xUnit**, **FluentAssertions** e **NSubstitute**:

```bash
# Executa todos os testes unitários da solução
dotnet test src/Modulo01/AiProviderBenchmarker/AiProviderBenchmarker.slnx
```

### O que os testes cobrem:
1. **`ProviderMetricTests`**:
   - Valida o cálculo exato de TPS expurgando o TTFT da latência total.
   - Trata cenários de borda como zero tokens ou TTFT superior ao tempo total sem gerar divisão por zero.
2. **`CostEstimatorTests`**:
   - Valida os custos ponderados para modelos OpenAI (`gpt-4o`, `gpt-4o-mini`, etc.).
   - Garante que inferências no **Ollama** sempre retornem custo zero ($0.00).
   - Valida o fallback de custo para modelos customizados não catalogados.
3. **`RunBenchmarkHandlerTests`**:
   - Testa a orquestração simultânea de múltiplos provedores via `Task.WhenAll`.
   - Valida o **isolamento de falhas**: se um provedor lançar `HttpRequestException` ou timeout, ele é marcado como falha individual, enquanto os outros provedores continuam executando e geram métricas de sucesso com integridade.
   - Testa a notificação em tempo real via canal de progresso (`IProgress<T>`).
