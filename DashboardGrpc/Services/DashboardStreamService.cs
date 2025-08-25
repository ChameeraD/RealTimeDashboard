using System;
using System.Threading.Tasks;
using Grpc.Core;
using Dashboard;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace DashboardGrpc.Services
{
    public class DashboardStreamService : Dashboard.DashboardService.DashboardServiceBase
    {
        private readonly ILogger<DashboardStreamService> _logger;
        private readonly IWebHostEnvironment _environment;

        public DashboardStreamService(ILogger<DashboardStreamService> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public override async Task Subscribe(
            Subscription request,
            IServerStreamWriter<DataPoint> responseStream,
            ServerCallContext context)
        {
            try
            {
                // Authorization check - in production, you'd validate JWT tokens here
                if (_environment.IsProduction())
                {
                    var user = context.GetHttpContext().User;
                    if (!user.Identity?.IsAuthenticated ?? true)
                    {
                        _logger.LogWarning("Unauthorized access attempt from {Peer}", context.Peer);
                        throw new RpcException(new Status(StatusCode.PermissionDenied, "Authentication required"));
                    }
                    _logger.LogInformation("Starting stream for user {User} on source {SourceId} with interval {Interval}ms", 
                        user.Identity?.Name ?? "Unknown", request.SourceId, request.IntervalMs);
                }
                else
                {
                    _logger.LogInformation("Starting stream for development on source {SourceId} with interval {Interval}ms", 
                        request.SourceId, request.IntervalMs);
                }

                // Validate request parameters
                if (string.IsNullOrEmpty(request.SourceId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "SourceId is required"));
                }

                if (request.IntervalMs < 100 || request.IntervalMs > 10000)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Interval must be between 100ms and 10 seconds"));
                }

                var random = new Random();
                var messageCount = 0;

                // Loop until the client disconnects
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Create a new data point
                        var point = new DataPoint
                        {
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            Value = random.NextDouble() * 100
                        };

                        // Send the point to the client
                        await responseStream.WriteAsync(point);
                        messageCount++;

                        // Log progress every 100 messages
                        if (messageCount % 100 == 0)
                        {
                            _logger.LogDebug("Sent {Count} messages to {Peer}", messageCount, context.Peer);
                        }

                        // Wait for the specified interval or cancellation
                        await Task.Delay(request.IntervalMs, context.CancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Client disconnected or cancellation requested
                        _logger.LogInformation("Stream cancelled for {Peer} after {Count} messages", context.Peer, messageCount);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending data point to {Peer}", context.Peer);
                        throw new RpcException(new Status(StatusCode.Internal, "Failed to send data point"));
                    }
                }

                _logger.LogInformation("Stream completed for {Peer} with {Count} messages", context.Peer, messageCount);
            }
            catch (RpcException)
            {
                // Re-throw gRPC exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Subscribe method for {Peer}", context.Peer);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
    }
}
