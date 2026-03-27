using System.Net.Http.Json;
using System.Text.Json;
using MediConnect.Application.Configurations;
using MediConnect.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediConnect.Infrastructure.Services;

public class OllamaLlmService : ILlmService
{
    private readonly OllamaSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaLlmService> _logger;

    public OllamaLlmService(
        IOptions<RagSettings> settings,
        HttpClient httpClient,
        ILogger<OllamaLlmService> logger)
    {
        _settings = settings.Value.Ollama;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GenerateResponseAsync(List<LlmMessage> messages, string? systemPrompt = null)
    {
        try
        {
            var allMessages = new List<object>();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                allMessages.Add(new { role = "system", content = systemPrompt });
            }

            allMessages.AddRange(messages.Select(m => new { role = m.Role, content = m.Content }));

            var request = new
            {
                model = _settings.Model,
                messages = allMessages,
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync($"{_settings.Endpoint}/api/chat", request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Message?.Content ?? "Không thể tạo phản hồi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate response from Ollama");
            return "Xin lỗi, hệ thống AI đang gặp sự cố. Vui lòng thử lại sau.";
        }
    }

    private class OllamaResponse
    {
        public OllamaMessage? Message { get; set; }
    }

    private class OllamaMessage
    {
        public string? Content { get; set; }
    }
}
