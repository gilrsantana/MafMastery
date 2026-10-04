namespace SmartRouter.Application.DTOs;

/// <summary>
/// DTO representando uma mensagem conversacional trocada com o modelo.
/// </summary>
/// <param name="Role">Papel do autor da mensagem: "user", "assistant", "system" ou "tool".</param>
/// <param name="Content">Conteúdo textual da mensagem.</param>
public record ChatMessageDto(string Role, string Content);
