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

public interface IRagService
{
    Task<RagSymptomAnalysisResult> AnalyzeSymptomsAsync(string symptoms);
    Task IndexMedicalKnowledgeAsync(string id, string content, string specialty);
}
