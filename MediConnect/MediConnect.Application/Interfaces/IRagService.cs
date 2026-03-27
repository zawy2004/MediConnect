using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public class RagSymptomAnalysisResult
{
    public string SymptomText { get; set; } = string.Empty;
    public string AssistantReply { get; set; } = string.Empty;
    public string SuggestedSpecialty { get; set; } = "Tổng quát";
    public int RiskScore { get; set; }
    public List<string> RelevantKnowledge { get; set; } = new();
}

public class RagSystemStatus
{
    public bool QdrantAvailable { get; set; }
    public bool OllamaAvailable { get; set; }
    public string UseLlm { get; set; } = "Groq";
    public bool EmbeddingUseOllama { get; set; }
    public string QdrantEndpoint { get; set; } = string.Empty;
    public string OllamaEndpoint { get; set; } = string.Empty;
}

public interface IRagService
{
    Task<RagSymptomAnalysisResult> AnalyzeSymptomsAsync(string symptoms);
    Task IndexMedicalKnowledgeAsync(string id, string content, string specialty);
    Task<DatabaseVectorizationResult> IndexDatabaseKnowledgeAsync(CancellationToken cancellationToken = default);
    Task<List<NormalizedVectorPreviewItem>> PreviewNormalizedDatabaseDocumentsAsync(int limit = 20, CancellationToken cancellationToken = default);
    Task<List<VectorSearchResult>> SemanticSearchAsync(string query, int topK = 5, float minScore = 0.5f);
    Task<RagSystemStatus> GetSystemStatusAsync(CancellationToken cancellationToken = default);
}
