namespace MediConnect.Application.Interfaces;

public class DatabaseVectorizationResult
{
    public int TotalNormalizedDocuments { get; set; }
    public int TotalIndexedDocuments { get; set; }
    public int FailedDocuments { get; set; }
    public Dictionary<string, int> IndexedByType { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class NormalizedVectorPreviewItem
{
    public string Type { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public interface IDatabaseVectorizationService
{
    Task<DatabaseVectorizationResult> NormalizeAndIndexAsync(CancellationToken cancellationToken = default);
    Task<List<NormalizedVectorPreviewItem>> PreviewNormalizedDocumentsAsync(int limit = 20, CancellationToken cancellationToken = default);
}
