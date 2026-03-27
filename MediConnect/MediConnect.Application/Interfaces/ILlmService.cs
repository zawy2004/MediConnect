namespace MediConnect.Application.Interfaces;

public class LlmMessage
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}

public interface ILlmService
{
    Task<string> GenerateResponseAsync(List<LlmMessage> messages, string? systemPrompt = null);
}
