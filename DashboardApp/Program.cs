using DashboardApp.Components;
using Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net.Http;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Syncfusion Blazor service
builder.Services.AddSyncfusionBlazor();

// Add authentication for production
if (builder.Environment.IsProduction())
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "your-secret-key"))
            };
        });
}
else
{
    // For development, add basic authentication services
    builder.Services.AddAuthentication();
}

// Add gRPC client with retry policies and error handling
builder.Services.AddGrpcClient<DashboardService.DashboardServiceClient>(options =>
{
    // Use HTTPS for proper HTTP/2 support
    options.Address = new Uri(builder.Configuration["Grpc:ServerUrl"] ?? "https://localhost:7051");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
    {
        // In development, accept self-signed certificates
        return builder.Environment.IsDevelopment() || errors == System.Net.Security.SslPolicyErrors.None;
    }
})
.ConfigureChannel(options =>
{
    // Ensure HTTP/2 is used
    options.HttpHandler = new SocketsHttpHandler
    {
        EnableMultipleHttp2Connections = true,
        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(30)
    };
});

// Add HTTP client for health checks
builder.Services.AddHttpClient("HealthCheck", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Grpc:ServerUrl"] ?? "https://localhost:7051");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
