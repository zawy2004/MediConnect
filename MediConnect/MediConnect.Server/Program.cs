using MediConnect.Application;
using MediConnect.Application.Configurations;
using MediConnect.Application.Interfaces;
using MediConnect.Infrastructure;
using MediConnect.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// HttpClient for external services
builder.Services.AddHttpClient();

// Configuration - Payment
builder.Services.Configure<VnPaySettings>(
    builder.Configuration.GetSection("VNPay"));
builder.Services.Configure<MomoSettings>(
    builder.Configuration.GetSection("MomoAPI"));

// Configuration - RAG
builder.Services.Configure<RagSettings>(
    builder.Configuration.GetSection("RAG"));

// Configuration - Email
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));

// Register LLM Service based on configuration
var useLlm = builder.Configuration["RAG:UseLLM"] ?? "Groq";
if (useLlm.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<ILlmService, OllamaLlmService>();
}
else
{
    builder.Services.AddScoped<ILlmService, GroqLlmService>();
}

// Authentication
var authenticationBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    // Skip validation for external identities (e.g. Google callback principal).
                    return;
                }

                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var user = await userRepository.GetByIdAsync(userId);

                if (user == null || !user.IsActive)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authenticationBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;

        // Explicitly request email/profile and map claims for reliable retrieval in callback.
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
    });
}

// Register Application Services
builder.Services.AddApplication();

// Register Infrastructure (DbContext, Repositories)
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
