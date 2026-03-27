using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class VectorDataModel : PageModel
{
    private readonly IRagService _ragService;

    public VectorDataModel(IRagService ragService)
    {
        _ragService = ragService;
    }

    [BindProperty]
    public int PreviewLimit { get; set; } = 20;

    [BindProperty]
    public string SearchQuery { get; set; } = string.Empty;

    [BindProperty]
    public int SearchTopK { get; set; } = 5;

    [BindProperty]
    public float SearchMinScore { get; set; } = 0.5f;

    public List<NormalizedVectorPreviewItem> PreviewItems { get; set; } = new();

    public DatabaseVectorizationResult? IndexResult { get; set; }

    public List<VectorSearchResult> SearchResults { get; set; } = new();
    public RagSystemStatus SystemStatus { get; set; } = new();

    public async Task OnGetAsync()
    {
        SystemStatus = await _ragService.GetSystemStatusAsync();
        PreviewItems = await _ragService.PreviewNormalizedDatabaseDocumentsAsync(PreviewLimit);
    }

    public async Task<IActionResult> OnPostPreviewAsync()
    {
        PreviewLimit = Math.Clamp(PreviewLimit, 1, 200);
        SystemStatus = await _ragService.GetSystemStatusAsync();
        PreviewItems = await _ragService.PreviewNormalizedDatabaseDocumentsAsync(PreviewLimit);
        return Page();
    }

    public async Task<IActionResult> OnPostIndexAsync()
    {
        PreviewLimit = Math.Clamp(PreviewLimit, 1, 200);
        SystemStatus = await _ragService.GetSystemStatusAsync();
        IndexResult = await _ragService.IndexDatabaseKnowledgeAsync();
        PreviewItems = await _ragService.PreviewNormalizedDatabaseDocumentsAsync(PreviewLimit);
        return Page();
    }

    public async Task<IActionResult> OnPostSearchAsync()
    {
        SearchTopK = Math.Clamp(SearchTopK, 1, 20);
        SearchMinScore = Math.Clamp(SearchMinScore, 0f, 1f);

        PreviewLimit = Math.Clamp(PreviewLimit, 1, 200);
        SystemStatus = await _ragService.GetSystemStatusAsync();
        PreviewItems = await _ragService.PreviewNormalizedDatabaseDocumentsAsync(PreviewLimit);

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchResults = await _ragService.SemanticSearchAsync(SearchQuery, SearchTopK, SearchMinScore);
        }

        return Page();
    }
}
