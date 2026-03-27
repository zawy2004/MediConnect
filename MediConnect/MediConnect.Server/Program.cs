using MediConnect.Application;
using MediConnect.Application.Configurations;
using MediConnect.Application.Interfaces;
using MediConnect.Infrastructure;
using MediConnect.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

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
