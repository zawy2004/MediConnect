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
        try
        {
            // Use Ollama for embeddings
            var endpoint = $"{_settings.Ollama.Endpoint}/api/embeddings";

            var request = new
            {
                model = "nomic-embed-text",
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

            return result.Embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding via Ollama, using fallback");
            return GenerateFallbackEmbedding(text);
        }
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
