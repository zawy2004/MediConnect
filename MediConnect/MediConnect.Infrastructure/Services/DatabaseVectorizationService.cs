using System.Text;
using System.Text.RegularExpressions;
using MediConnect.Application.Interfaces;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediConnect.Infrastructure.Services;

public class DatabaseVectorizationService : IDatabaseVectorizationService
{
    private static readonly Regex MultiWhitespaceRegex = new("\\s+", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new(@"\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}\b", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"\b(?:\+?84|0)(?:\d[ .-]?){8,10}\b", RegexOptions.Compiled);
    private static readonly Regex LongNumberRegex = new(@"\b\d{9,16}\b", RegexOptions.Compiled);
    private static readonly Regex UrlRegex = new(@"https?://\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SensitiveIdKeywordRegex = new(
        @"(?i)\b(cmnd|cccd|bhyt|so the|so ho chieu|passport|ma benh nhan|patient id|patient code)\b\s*[:#-]?\s*[A-Z0-9-]{4,20}",
        RegexOptions.Compiled);
    private static readonly Regex SensitiveAddressKeywordRegex = new(
        @"(?i)\b(dia chi|address)\b\s*[:#-]?\s*[^.;\n]{6,120}",
        RegexOptions.Compiled);

    private readonly MediconnectContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStoreService;
    private readonly ILogger<DatabaseVectorizationService> _logger;

    public DatabaseVectorizationService(
        MediconnectContext dbContext,
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStoreService,
        ILogger<DatabaseVectorizationService> logger)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
        _logger = logger;
    }

    public async Task<DatabaseVectorizationResult> NormalizeAndIndexAsync(CancellationToken cancellationToken = default)
    {
        var result = new DatabaseVectorizationResult();

        var collectionReady = await _vectorStoreService.EnsureCollectionExistsAsync();
        if (!collectionReady)
        {
            result.Errors.Add("Vector collection is not available.");
            return result;
        }

        var documents = await BuildNormalizedDocumentsAsync(cancellationToken);
        result.TotalNormalizedDocuments = documents.Count;

        foreach (var doc in documents)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(doc.Content);

                var metadata = new Dictionary<string, string>(doc.Metadata)
                {
                    ["type"] = doc.Type,
                    ["source_id"] = doc.SourceId,
                    ["normalized_at"] = DateTime.UtcNow.ToString("O")
                };

                await _vectorStoreService.UpsertAsync(doc.VectorId, embedding, doc.Content, metadata);

                result.TotalIndexedDocuments++;
                if (!result.IndexedByType.ContainsKey(doc.Type))
                {
                    result.IndexedByType[doc.Type] = 0;
                }

                result.IndexedByType[doc.Type]++;
            }
            catch (Exception ex)
            {
                result.FailedDocuments++;
                var error = $"{doc.Type}:{doc.SourceId} - {ex.Message}";
                result.Errors.Add(error);
                _logger.LogWarning(ex, "Failed to index normalized document {Type}:{SourceId}", doc.Type, doc.SourceId);
            }
        }

        _logger.LogInformation(
            "Database vectorization completed. Normalized={Normalized}, Indexed={Indexed}, Failed={Failed}",
            result.TotalNormalizedDocuments,
            result.TotalIndexedDocuments,
            result.FailedDocuments);

        return result;
    }

    public async Task<List<NormalizedVectorPreviewItem>> PreviewNormalizedDocumentsAsync(int limit = 20, CancellationToken cancellationToken = default)
    {
        var safeLimit = Math.Clamp(limit, 1, 200);
        var documents = await BuildNormalizedDocumentsAsync(cancellationToken);

        return documents
            .Take(safeLimit)
            .Select(d => new NormalizedVectorPreviewItem
            {
                Type = d.Type,
                SourceId = d.SourceId,
                Content = d.Content,
                Metadata = new Dictionary<string, string>(d.Metadata)
            })
            .ToList();
    }

    private async Task<List<NormalizedVectorDocument>> BuildNormalizedDocumentsAsync(CancellationToken cancellationToken)
    {
        var documents = new List<NormalizedVectorDocument>();

        var specialties = await _dbContext.Specialties
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        documents.AddRange(specialties.Select(s => new NormalizedVectorDocument
        {
            Type = "specialty",
            SourceId = s.SpecialtyId.ToString(),
            VectorId = $"specialty:{s.SpecialtyId}",
            Content = NormalizeText($"Chuyen khoa: {s.SpecialtyName}. Mo ta: {s.Description}."),
            Metadata = new Dictionary<string, string>
            {
                ["specialty_name"] = s.SpecialtyName
            }
        }));

        var departments = await _dbContext.Departments
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        documents.AddRange(departments.Select(d => new NormalizedVectorDocument
        {
            Type = "department",
            SourceId = d.DepartmentId.ToString(),
            VectorId = $"department:{d.DepartmentId}",
            Content = NormalizeText($"Khoa phong: {d.DepartmentName}. Vi tri: {d.Location}. Mo ta: {d.Description}."),
            Metadata = new Dictionary<string, string>
            {
                ["department_name"] = d.DepartmentName
            }
        }));

        var doctorProfiles = await _dbContext.DoctorProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Department)
            .Where(x => x.ApprovalStatus == "APPROVED")
            .ToListAsync(cancellationToken);

        var doctorIds = doctorProfiles.Select(x => x.UserId).Distinct().ToList();
        var doctorSpecialties = await _dbContext.DoctorSpecialties
            .AsNoTracking()
            .Include(x => x.Specialty)
            .Where(x => doctorIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);

        var specialtiesByDoctor = doctorSpecialties
            .GroupBy(x => x.UserId)
            .ToDictionary(
                x => x.Key,
                x => string.Join(", ", x.Select(i => i.Specialty.SpecialtyName).Distinct()));

        documents.AddRange(doctorProfiles.Select(dp =>
        {
            specialtiesByDoctor.TryGetValue(dp.UserId, out var doctorSpecialtyText);

            var builder = new StringBuilder();
            builder.Append($"Bac si: {dp.User.FullName}. ");
            builder.Append($"Chuyen khoa: {doctorSpecialtyText}. ");
            builder.Append($"Khoa phong: {dp.Department?.DepartmentName}. ");
            builder.Append($"Kinh nghiem: {dp.YearsOfExperience} nam. ");
            builder.Append($"Hoc van: {dp.Education}. ");
            builder.Append($"Mo ta: {dp.Bio}. ");
            builder.Append($"Phi tu van: {dp.ConsultationFee}. ");
            builder.Append($"Dia diem: {dp.Location}. ");
            builder.Append($"Danh gia trung binh: {dp.AverageRating}. ");

            return new NormalizedVectorDocument
            {
                Type = "doctor_profile",
                SourceId = dp.DoctorProfileId.ToString(),
                VectorId = $"doctor_profile:{dp.DoctorProfileId}",
                Content = NormalizeText(builder.ToString()),
                Metadata = new Dictionary<string, string>
                {
                    ["doctor_user_id"] = dp.UserId.ToString(),
                    ["doctor_name"] = dp.User.FullName,
                    ["specialties"] = doctorSpecialtyText ?? string.Empty
                }
            };
        }));

        var medicalRecords = await _dbContext.MedicalRecords
            .AsNoTracking()
            .Include(x => x.Doctor)
            .OrderByDescending(x => x.RecordDate)
            .Take(1000)
            .ToListAsync(cancellationToken);

        documents.AddRange(medicalRecords.Select(mr =>
        {
            var builder = new StringBuilder();
            builder.Append($"Ho so benh an ngay {mr.RecordDate:yyyy-MM-dd}. ");
            builder.Append($"Bac si phu trach: {mr.Doctor.FullName}. ");
            builder.Append($"Trieu chung: {mr.Symptoms}. ");
            builder.Append($"Chan doan: {mr.Diagnosis}. ");
            builder.Append($"Huong dieu tri: {mr.TreatmentPlan}. ");
            builder.Append($"Don thuoc: {mr.Prescription}. ");

            return new NormalizedVectorDocument
            {
                Type = "medical_record",
                SourceId = mr.RecordId.ToString(),
                VectorId = $"medical_record:{mr.RecordId}",
                Content = NormalizeText(builder.ToString()),
                Metadata = new Dictionary<string, string>
                {
                    ["record_date"] = mr.RecordDate.ToString("yyyy-MM-dd"),
                    ["doctor_id"] = mr.DoctorId.ToString()
                }
            };
        }));

        var reviews = await _dbContext.Reviews
            .AsNoTracking()
            .Include(x => x.Doctor)
            .Where(x => x.IsVisible)
            .OrderByDescending(x => x.CreatedAt)
            .Take(1000)
            .ToListAsync(cancellationToken);

        documents.AddRange(reviews.Select(rv =>
        {
            var reviewText = $"Danh gia bac si {rv.Doctor.FullName}. Diem: {rv.Rating}/5. Nhan xet: {rv.Comment}. Sentiment: {rv.SentimentLabel}.";

            return new NormalizedVectorDocument
            {
                Type = "review",
                SourceId = rv.ReviewId.ToString(),
                VectorId = $"review:{rv.ReviewId}",
                Content = NormalizeText(reviewText),
                Metadata = new Dictionary<string, string>
                {
                    ["doctor_id"] = rv.DoctorId.ToString(),
                    ["rating"] = rv.Rating.ToString()
                }
            };
        }));

        return documents
            .Where(x => !string.IsNullOrWhiteSpace(x.Content))
            .ToList();
    }

    private static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormKC).Trim();
        normalized = RedactSensitiveData(normalized);
        normalized = MultiWhitespaceRegex.Replace(normalized, " ");
        return normalized;
    }

    private static string RedactSensitiveData(string input)
    {
        var redacted = SensitiveIdKeywordRegex.Replace(input, "[REDACTED_SENSITIVE_ID]");
        redacted = SensitiveAddressKeywordRegex.Replace(redacted, "[REDACTED_ADDRESS]");
        redacted = EmailRegex.Replace(redacted, "[REDACTED_EMAIL]");
        redacted = PhoneRegex.Replace(redacted, "[REDACTED_PHONE]");
        redacted = LongNumberRegex.Replace(redacted, "[REDACTED_NUMBER]");
        redacted = UrlRegex.Replace(redacted, "[REDACTED_URL]");
        return redacted;
    }

    private class NormalizedVectorDocument
    {
        public string Type { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public string VectorId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
