namespace AiProviderBenchmarker.Infrastructure.Configuration;

/// <summary>
/// Root options for AI providers configuration.
/// Uses a flexible list of providers where each defines its own AiProviderName.
/// </summary>
public class AiProvidersOptions
{
    public const string SectionName = "AiProviders";

    /// <summary>
    /// List of providers configured in the system.
    /// Allows registering multiple models/endpoints of the same type (e.g., two OpenAI or Ollama models).
    /// </summary>
    public List<AiProviderConfig> Providers { get; set; } = [];

    /// <summary>
    /// Gets the configuration matching the composite key (Provider:Model), model/deployment name, or provider name.
    /// </summary>
    public AiProviderConfig? GetProvider(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        // 1. Exact match on Key (e.g., "OpenAi:gpt-4o-mini", "AzureOpenAi:gpt-4o-mini")
        var byKey = Providers.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (byKey != null) return byKey;

        // 2. If the key is formatted with delimiters (e.g., "OpenAi:gpt-4o-mini" or "OpenAi (gpt-4o-mini)")
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

        // 3. Search by DeploymentName (only if unique among providers)
        var matchingDeployments = Providers.Where(p => p.DeploymentName.Equals(key, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matchingDeployments.Count == 1) return matchingDeployments[0];

        // 4. Search by AiProviderName
        var byProviderName = Providers.FirstOrDefault(p => p.AiProviderName.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (byProviderName != null) return byProviderName;

        return matchingDeployments.FirstOrDefault();
    }

    /// <summary>
    /// Gets configuration by explicitly searching by Provider Name AND Deployment/Model Name.
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
    /// Resolves a collection of model names or keys to their respective provider configurations.
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
                // If item matches a generic provider name (e.g., "Gemini" or "OpenAi"), include all active configurations for that provider
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
