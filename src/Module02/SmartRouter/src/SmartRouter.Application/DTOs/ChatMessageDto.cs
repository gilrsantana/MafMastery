namespace SmartRouter.Application.DTOs;

/// <summary>
/// DTO representing a conversational message exchanged with the model.
/// </summary>
/// <param name="Role">Author role: "user", "assistant", "system", or "tool".</param>
/// <param name="Content">Textual content of the message.</param>
public record ChatMessageDto(string Role, string Content);
