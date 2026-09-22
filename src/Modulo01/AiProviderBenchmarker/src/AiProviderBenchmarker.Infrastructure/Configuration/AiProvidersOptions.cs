namespace AiProviderBenchmarker.Infrastructure.Configuration;

/// <summary>
/// Opções raiz para configuração dos provedores de IA.
/// Utiliza uma lista flexível de provedores onde cada um define sua propriedade AiProviderName.
/// </summary>
public class AiProvidersOptions
{
    public const string SectionName = "AiProviders";

    /// <summary>
    /// Lista de provedores configurados no sistema.
    /// Permite cadastrar múltiplos modelos/endpoints do mesmo tipo (ex: dois modelos de OpenAI ou Ollama).
    /// </summary>
    public List<AiProviderConfig> Providers { get; set; } = [];

    /// <summary>
    /// Obtém a configuração correspondente à chave composta (Provider:Model), modelo/implantação ou nome do provedor informado.
    /// </summary>
    public AiProviderConfig? GetProvider(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        // 1. Busca por correspondência exata de Key (ex: "OpenAi:gpt-4o-mini", "AzureOpenAi:gpt-4o-mini")
        var byKey = Providers.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (byKey != null) return byKey;

        // 2. Se a chave estiver em formato composto com separador (ex: "OpenAi:gpt-4o-mini" ou "OpenAi (gpt-4o-mini)")
        if (key.Contains(':') || key.Contains('/') || key.Contains('('))
        {
            var parts = key.Split([':', '/', '(', ')'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length >= 2)
            {
                var providerPart = parts[0];
                var modelPart = parts[1];
                var byBoth = Providers.FirstOrDefault(p =>
                    p.AiProviderName.Equals(providerPart, StringComparison.OrdinalIgnoreCase) &&
                    p.DeploymentName.Equals(modelPart, StringComparison.OrdinalIgnoreCase));
                if (byBoth != null) return byBoth;
            }
        }

        // 3. Busca por DeploymentName (apenas se for único entre os provedores)
        var matchingDeployments = Providers.Where(p => p.DeploymentName.Equals(key, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matchingDeployments.Count == 1) return matchingDeployments[0];

        // 4. Busca por AiProviderName
        var byProviderName = Providers.FirstOrDefault(p => p.AiProviderName.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (byProviderName != null) return byProviderName;

        return matchingDeployments.FirstOrDefault();
    }

    /// <summary>
    /// Obtém a configuração buscando explicitamente por Nome de Provedor E Nome de Implantação/Modelo.
    /// </summary>
    public AiProviderConfig? GetProvider(string providerName, string modelName)
    {
        if (string.IsNullOrWhiteSpace(providerName) && string.IsNullOrWhiteSpace(modelName))
            return null;

        if (!string.IsNullOrWhiteSpace(providerName) && !string.IsNullOrWhiteSpace(modelName))
        {
            var exactMatch = Providers.FirstOrDefault(p =>
                p.AiProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase) &&
                p.DeploymentName.Equals(modelName, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null) return exactMatch;
        }

        return GetProvider(modelName) ?? GetProvider(providerName);
    }

    /// <summary>
    /// Resolve uma lista de nomes/chaves de modelos para as respectivas configurações de provedores.
    /// </summary>
    public List<AiProviderConfig> ResolveProviders(IEnumerable<string> keysOrNames)
    {
        var resolved = new List<AiProviderConfig>();
        foreach (var item in keysOrNames)
        {
            if (string.IsNullOrWhiteSpace(item)) continue;

            var config = GetProvider(item);
            if (config != null)
            {
                // Se item é exatamente o nome de um provedor genérico (ex: "Gemini" ou "OpenAi"), inclui todas as configurações ativas daquele provedor
                if (item.Equals(config.AiProviderName, StringComparison.OrdinalIgnoreCase))
                {
                    var matchingProviders = Providers.Where(p => p.AiProviderName.Equals(item, StringComparison.OrdinalIgnoreCase) && p.IsConfigured).ToList();
                    foreach (var p in matchingProviders)
                    {
                        if (!resolved.Contains(p)) resolved.Add(p);
                    }
                    continue;
                }

                if (!resolved.Contains(config)) resolved.Add(config);
                continue;
            }
        }
        return resolved;
    }
}
