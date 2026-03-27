using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MediConnect.Application.Configurations;
using MediConnect.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediConnect.Infrastructure.Services;

public class GroqLlmService : ILlmService
{
    private readonly GroqSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GroqLlmService> _logger;

    public GroqLlmService(
        IOptions<RagSettings> settings,
        HttpClient httpClient,
        ILogger<GroqLlmService> logger)
    {
        _settings = settings.Value.Groq;
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
                temperature = 0.7,
                max_tokens = 1024
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.Endpoint);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            httpRequest.Content = JsonContent.Create(request);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GroqResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "Không thể tạo phản hồi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate response from Groq");
            return "Xin lỗi, hệ thống AI đang gặp sự cố. Vui lòng thử lại sau.";
        }
    }

    private class GroqResponse
    {
        public List<GroqChoice>? Choices { get; set; }
    }

    private class GroqChoice
    {
        public GroqMessage? Message { get; set; }
    }

    private class GroqMessage
    {
        public string? Content { get; set; }
    }
}
