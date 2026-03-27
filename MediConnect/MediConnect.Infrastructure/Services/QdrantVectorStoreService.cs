using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediConnect.Application.Configurations;
using MediConnect.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediConnect.Infrastructure.Services;

public class QdrantVectorStoreService : IVectorStoreService
{
    private readonly QdrantSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<QdrantVectorStoreService> _logger;
    private readonly string _baseUrl;

    public QdrantVectorStoreService(
        IOptions<RagSettings> settings,
        HttpClient httpClient,
        ILogger<QdrantVectorStoreService> logger)
    {
        _settings = settings.Value.Qdrant;
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = BuildQdrantBaseUrl(_settings);

        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Remove("api-key");
            _httpClient.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);
        }
    }

    public async Task<bool> EnsureCollectionExistsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/collections/{_settings.CollectionName}");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            // Create collection
            var createRequest = new
            {
                vectors = new
                {
                    size = _settings.VectorSize,
                    distance = "Cosine"
                }
            };

            var createResponse = await _httpClient.PutAsJsonAsync(
                $"{_baseUrl}/collections/{_settings.CollectionName}",
                createRequest);

            return createResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure Qdrant collection exists");
            return false;
        }
    }

    public async Task<List<VectorSearchResult>> SearchAsync(float[] vector, int topK = 5)
    {
        try
        {
            var searchRequest = new
            {
                vector,
                limit = topK,
                with_payload = true
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/collections/{_settings.CollectionName}/points/search",
                searchRequest);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Qdrant search failed with status {StatusCode}", response.StatusCode);
                return new List<VectorSearchResult>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<QdrantSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Result?.Select(r => new VectorSearchResult
            {
                // Qdrant returns dynamic `id`; when deserializing it can be null.
                Id = r.Id?.ToString() ?? string.Empty,
                Score = r.Score,
                Content = r.Payload?.TryGetValue("content", out var contentVal) == true
                    ? contentVal.GetString() ?? string.Empty
                    : string.Empty,
                Metadata = r.Payload?.Where(p => p.Key != "content")
                    .ToDictionary(p => p.Key, p => p.Value.GetString() ?? string.Empty) ?? new()
            }).ToList() ?? new List<VectorSearchResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Qdrant search failed");
            return new List<VectorSearchResult>();
        }
    }

    public async Task UpsertAsync(string id, float[] vector, string content, Dictionary<string, string>? metadata = null)
    {
        try
        {
            var collectionReady = await EnsureCollectionExistsAsync();
            if (!collectionReady)
            {
                throw new InvalidOperationException("Qdrant collection is not available.");
            }

            var payload = new Dictionary<string, object>
            {
                { "content", content }
            };

            if (metadata != null)
            {
                foreach (var kv in metadata)
                {
                    payload[kv.Key] = kv.Value;
                }
            }

            var upsertRequest = new
            {
                points = new[]
                {
                    new
                    {
                        id = GenerateStablePointId(id),
                        vector,
                        payload
                    }
                }
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"{_baseUrl}/collections/{_settings.CollectionName}/points",
                upsertRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Qdrant upsert failed ({response.StatusCode}): {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Qdrant upsert failed");
            throw;
        }
    }

    private static ulong GenerateStablePointId(string sourceId)
    {
        // Use deterministic hash so the same source document always maps to the same Qdrant point ID.
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sourceId));
        return BitConverter.ToUInt64(bytes, 0);
    }

    private static string BuildQdrantBaseUrl(QdrantSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Url))
        {
            var trimmed = settings.Url.Trim().TrimEnd('/');
            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = $"https://{trimmed}";
            }

            return trimmed;
        }

        return $"http://{settings.Host}:{settings.Port}";
    }

    private class QdrantSearchResponse
    {
        public List<QdrantSearchResult>? Result { get; set; }
    }

    private class QdrantSearchResult
    {
        public object Id { get; set; } = new();
        public float Score { get; set; }
        public Dictionary<string, JsonElement>? Payload { get; set; }
    }
}
