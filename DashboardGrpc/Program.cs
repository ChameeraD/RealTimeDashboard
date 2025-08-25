using Microsoft.AspNetCore.Server.Kestrel.Core;
using DashboardGrpc.Services;
using System.IO.Compression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configure Kestrel for HTTP/2 and HTTPS
builder.WebHost.ConfigureKestrel(k =>
{
    // HTTP endpoint for development (supports both HTTP/1.1 and HTTP/2)
    k.ListenAnyIP(5090, opt =>
    {
        opt.Protocols = HttpProtocols.Http1AndHttp2;
    });
    
    // HTTPS endpoint for production
    k.ListenAnyIP(7051, opt =>
    {
        opt.Protocols = HttpProtocols.Http2;
        opt.UseHttps();
    });
});

// Add authentication (JWT Bearer for production, or disable for development)
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

builder.Services.AddAuthorization();

// gRPC framework services with compression and interceptors
builder.Services.AddGrpc(options =>
{
    options.ResponseCompressionAlgorithm = "gzip";
    options.ResponseCompressionLevel = CompressionLevel.Fastest;
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaxReceiveMessageSize = 1024 * 1024; // 1MB
    options.MaxSendMessageSize = 1024 * 1024;   // 1MB
});

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS services
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map health checks
app.MapHealthChecks("/health");

// Map your gRPC service
app.MapGrpcService<DashboardStreamService>();

// Add CORS for development
if (app.Environment.IsDevelopment())
{
    app.UseCors(builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}

app.Run();
