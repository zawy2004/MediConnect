using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class SystemMonitoringModel : PageModel
{
    private readonly IAdminPortalService _adminPortalService;
    private readonly IRagService _ragService;
    private readonly ILlmService _llmService;

    public SystemMonitoringModel(
        IAdminPortalService adminPortalService,
        IRagService ragService,
        ILlmService llmService)
    {
        _adminPortalService = adminPortalService;
        _ragService = ragService;
        _llmService = llmService;
    }

    public AdminMonitoringDto Data { get; set; } = new();
    public AdminAiSelfCheckResultDto? SelfCheckResult { get; set; }

    public async Task OnGetAsync()
    {
        Data = await _adminPortalService.GetMonitoringAsync();
    }

    public async Task<IActionResult> OnPostAiSelfCheckAsync()
    {
        Data = await _adminPortalService.GetMonitoringAsync();

        var result = new AdminAiSelfCheckResultDto
        {
            CheckedAt = DateTime.Now
        };

        var status = await _ragService.GetSystemStatusAsync();
        result.Items.Add(new AdminAiSelfCheckItemDto
        {
            Name = "Qdrant Connectivity",
            Passed = status.QdrantAvailable,
            Detail = status.QdrantAvailable
                ? $"Online: {status.QdrantEndpoint}"
                : $"Offline: {status.QdrantEndpoint}"
        });

        try
        {
            var llmResponse = await _llmService.GenerateResponseAsync(
                new List<LlmMessage> { new() { Role = "user", Content = "Trả lời đúng chữ: OK" } },
                "Bạn là hệ thống self-check. Chỉ trả lời đúng chữ OK.");

            var llmPassed = llmResponse.Trim().Contains("OK", StringComparison.OrdinalIgnoreCase);
            result.Items.Add(new AdminAiSelfCheckItemDto
            {
                Name = "LLM Response",
                Passed = llmPassed,
                Detail = llmPassed ? llmResponse.Trim() : $"Unexpected response: {llmResponse.Trim()}"
            });
        }
        catch (Exception ex)
        {
            result.Items.Add(new AdminAiSelfCheckItemDto
            {
                Name = "LLM Response",
                Passed = false,
                Detail = ex.Message
            });
        }

        var previewItems = await _ragService.PreviewNormalizedDatabaseDocumentsAsync(1);
        result.Items.Add(new AdminAiSelfCheckItemDto
        {
            Name = "Vector Preview Read",
            Passed = previewItems.Count > 0,
            Detail = previewItems.Count > 0
                ? $"Loaded {previewItems.Count} normalized preview item(s)."
                : "No normalized preview data found."
        });

        try
        {
            var vectorResults = await _ragService.SemanticSearchAsync("đau đầu", 1, 0f);
            result.Items.Add(new AdminAiSelfCheckItemDto
            {
                Name = "Vector Search Path",
                Passed = status.QdrantAvailable,
                Detail = status.QdrantAvailable
                    ? $"Search executed successfully. Returned {vectorResults.Count} item(s)."
                    : "Skipped because Qdrant is offline."
            });
        }
        catch (Exception ex)
        {
            result.Items.Add(new AdminAiSelfCheckItemDto
            {
                Name = "Vector Search Path",
                Passed = false,
                Detail = ex.Message
            });
        }

        result.IsOverallPassed = result.Items.All(x => x.Passed);
        SelfCheckResult = result;

        return Page();
    }
}
