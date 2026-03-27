using System.Net.Http.Json;
using System.Text.Json;
using MediConnect.Application.Configurations;
using MediConnect.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediConnect.Infrastructure.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly RagSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmbeddingService> _logger;
    private DateTime _ollamaRetryAfterUtc = DateTime.MinValue;
    private bool _unavailableLogged;

    public EmbeddingService(
        IOptions<RagSettings> settings,
        HttpClient httpClient,
        ILogger<EmbeddingService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        if (!_settings.Embedding.UseOllama)
        {
            return GenerateFallbackEmbedding(text);
        }

        if (DateTime.UtcNow < _ollamaRetryAfterUtc)
        {
            return GenerateFallbackEmbedding(text);
        }

        try
        {
            // Use Ollama for embeddings
            var endpoint = $"{_settings.Ollama.Endpoint}/api/embeddings";

            var request = new
            {
                model = _settings.Embedding.OllamaModel,
                prompt = text
            };

            var response = await _httpClient.PostAsJsonAsync(endpoint, request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();

            if (result?.Embedding == null || result.Embedding.Length == 0)
            {
                _logger.LogWarning("Empty embedding returned from Ollama");
                return GenerateFallbackEmbedding(text);
            }

            _unavailableLogged = false;
            _ollamaRetryAfterUtc = DateTime.MinValue;

            return result.Embedding;
        }
        catch (HttpRequestException ex)
        {
            SetOllamaCooldown();
            if (!_unavailableLogged)
            {
                _logger.LogWarning(ex,
                    "Ollama endpoint is unavailable ({Endpoint}). Falling back to local embedding for {RetrySeconds}s.",
                    _settings.Ollama.Endpoint,
                    _settings.Embedding.OllamaRetrySeconds);
                _unavailableLogged = true;
            }

            return GenerateFallbackEmbedding(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding via Ollama, using fallback");
            return GenerateFallbackEmbedding(text);
        }
    }

    private void SetOllamaCooldown()
    {
        var retrySeconds = Math.Max(10, _settings.Embedding.OllamaRetrySeconds);
        _ollamaRetryAfterUtc = DateTime.UtcNow.AddSeconds(retrySeconds);
    }

    private float[] GenerateFallbackEmbedding(string text)
    {
        // Simple hash-based embedding as fallback
        var embedding = new float[_settings.Embedding.Dimension];
        var hash = text.GetHashCode();
        var random = new Random(hash);

        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1);
        }

        // Normalize
        var magnitude = (float)Math.Sqrt(embedding.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] /= magnitude;
            }
        }

        return embedding;
    }

    private class OllamaEmbeddingResponse
    {
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
