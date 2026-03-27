namespace MediConnect.Application.Interfaces;

public class VectorSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float Score { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public interface IVectorStoreService
{
    Task<List<VectorSearchResult>> SearchAsync(float[] vector, int topK = 5);
    Task UpsertAsync(string id, float[] vector, string content, Dictionary<string, string>? metadata = null);
    Task<bool> EnsureCollectionExistsAsync();
}
