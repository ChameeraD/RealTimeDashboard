using Microsoft.AspNetCore.Server.Kestrel.Core;
using DashboardGrpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(k =>
{
    k.ListenAnyIP(5001, opt =>
    {
        opt.Protocols = HttpProtocols.Http2; // Force Kestrel to speak HTTP/2 on port 5001
        opt.UseHttps();                      // Turn on HTTPS (self-signed dev cert is fine for local testing)
    });
});

// gRPC framework services
builder.Services.AddGrpc();

var app = builder.Build();

// Map your service class
app.MapGrpcService<DashboardStreamService>();

app.Run();
