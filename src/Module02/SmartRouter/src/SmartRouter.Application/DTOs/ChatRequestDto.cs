namespace SmartRouter.Application.DTOs;

/// <summary>
/// DTO da requisição de chat recebida pelos endpoints da Minimal API.
/// </summary>
public record ChatRequestDto
{
    /// <summary>
    /// Histórico de mensagens da conversa.
    /// </summary>
    public List<ChatMessageDto> Messages { get; init; } = [];

    /// <summary>
    /// Modelo explicitamente solicitado pelo consumidor (opcional; se omitido, o roteador determina automaticamente).
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Hiperparâmetro de temperatura para amostragem estocástica (opcional).
    /// </summary>
    public float? Temperature { get; init; }

    /// <summary>
    /// Quantidade máxima de tokens permitida na resposta gerada (opcional).
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Força uma categoria de rota: "Economy" ou "Premium" (opcional).
    /// </summary>
    public string? RouteTierPreference { get; init; }
}
