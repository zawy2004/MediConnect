using MediConnect.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MediConnect.Application.Services;

public class RagService : IRagService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStoreService;
    private readonly ILlmService _llmService;
    private readonly ILogger<RagService> _logger;

    private const string SystemPrompt = @"Bạn là trợ lý y tế AI của MediConnect. Nhiệm vụ của bạn là phân tích triệu chứng bệnh nhân và đưa ra khuyến nghị chuyên khoa phù hợp.

Dựa trên triệu chứng và kiến thức y khoa được cung cấp, hãy:
1. Đánh giá mức độ nghiêm trọng (thấp/trung bình/cao/khẩn cấp)
2. Gợi ý chuyên khoa phù hợp nhất
3. Đưa ra lời khuyên ngắn gọn cho bệnh nhân

Trả lời bằng tiếng Việt, ngắn gọn và chuyên nghiệp.
Format trả lời:
SPECIALTY: [tên chuyên khoa]
RISK: [số từ 0-100]
REPLY: [lời khuyên cho bệnh nhân]";

    public RagService(
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStoreService,
        ILlmService llmService,
        ILogger<RagService> logger)
    {
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
        _llmService = llmService;
        _logger = logger;
    }

    public async Task<RagSymptomAnalysisResult> AnalyzeSymptomsAsync(string symptoms)
    {
        try
        {
            // Step 1: Generate embedding for symptoms
            var embedding = await _embeddingService.GenerateEmbeddingAsync(symptoms);

            // Step 2: Search for relevant medical knowledge
            var searchResults = await _vectorStoreService.SearchAsync(embedding, topK: 5);

            var relevantKnowledge = searchResults
                .Where(r => r.Score > 0.5f)
                .Select(r => r.Content)
                .ToList();

            // Step 3: Build context for LLM
            var context = relevantKnowledge.Count > 0
                ? $"Kiến thức y khoa liên quan:\n{string.Join("\n---\n", relevantKnowledge)}"
                : "Không tìm thấy kiến thức y khoa cụ thể trong cơ sở dữ liệu.";

            var userMessage = $"Triệu chứng bệnh nhân: {symptoms}\n\n{context}";

            // Step 4: Generate response using LLM
            var messages = new List<LlmMessage>
            {
                new() { Role = "user", Content = userMessage }
            };

            var response = await _llmService.GenerateResponseAsync(messages, SystemPrompt);

            // Step 5: Parse LLM response
            return ParseLlmResponse(symptoms, response, relevantKnowledge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze symptoms with RAG");

            // Fallback to basic analysis
            return FallbackAnalysis(symptoms);
        }
    }

    public async Task IndexMedicalKnowledgeAsync(string id, string content, string specialty)
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(content);

        await _vectorStoreService.UpsertAsync(id, embedding, content, new Dictionary<string, string>
        {
            { "specialty", specialty },
            { "indexed_at", DateTime.UtcNow.ToString("O") }
        });

        _logger.LogInformation("Indexed medical knowledge: {Id}, Specialty: {Specialty}", id, specialty);
    }

    private RagSymptomAnalysisResult ParseLlmResponse(string symptoms, string response, List<string> relevantKnowledge)
    {
        var result = new RagSymptomAnalysisResult
        {
            SymptomText = symptoms,
            RelevantKnowledge = relevantKnowledge
        };

        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("SPECIALTY:", StringComparison.OrdinalIgnoreCase))
            {
                result.SuggestedSpecialty = line.Substring("SPECIALTY:".Length).Trim();
            }
            else if (line.StartsWith("RISK:", StringComparison.OrdinalIgnoreCase))
            {
                var riskStr = line.Substring("RISK:".Length).Trim();
                if (int.TryParse(riskStr, out var risk))
                {
                    result.RiskScore = Math.Clamp(risk, 0, 100);
                }
            }
            else if (line.StartsWith("REPLY:", StringComparison.OrdinalIgnoreCase))
            {
                result.AssistantReply = line.Substring("REPLY:".Length).Trim();
            }
        }

        // If parsing failed, use the full response
        if (string.IsNullOrEmpty(result.AssistantReply))
        {
            result.AssistantReply = response;
        }

        return result;
    }

    private RagSymptomAnalysisResult FallbackAnalysis(string symptoms)
    {
        var lowered = symptoms.ToLowerInvariant();
        var suggestedSpecialty = "Tổng quát";
        var riskScore = 30;
        var reply = "Mức độ thấp. Bạn nên đặt lịch tư vấn tổng quát để bác sĩ đánh giá trực tiếp.";

        if (lowered.Contains("tức ngực") || lowered.Contains("khó thở") || lowered.Contains("đau tim"))
        {
            suggestedSpecialty = "Nội tim mạch";
            riskScore = 85;
            reply = "Triệu chứng có nguy cơ cao. Bạn nên khám Nội tim mạch trong ngày và theo dõi dấu hiệu bất thường.";
        }
        else if (lowered.Contains("ho") || lowered.Contains("sốt") || lowered.Contains("viêm") || lowered.Contains("mệt"))
        {
            suggestedSpecialty = "Nội tổng quát";
            riskScore = 55;
            reply = "Mức độ trung bình. Bạn nên đặt lịch khám trong 24 giờ để được kiểm tra chuyên sâu.";
        }

        return new RagSymptomAnalysisResult
        {
            SymptomText = symptoms,
            SuggestedSpecialty = suggestedSpecialty,
            RiskScore = riskScore,
            AssistantReply = reply,
            RelevantKnowledge = new List<string>()
        };
    }
}
