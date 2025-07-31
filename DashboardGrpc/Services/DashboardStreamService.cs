using System;
using System.Threading.Tasks;
using Grpc.Core;
using Dashboard;

namespace DashboardGrpc.Services
{
    public class DashboardStreamService : Dashboard.DashboardService.DashboardServiceBase
    {
        public override async Task Subscribe(
            Subscription request,
            IServerStreamWriter<DataPoint> responseStream,
            ServerCallContext context)
        {
            var random = new Random();

            // Loop until the client disconnects
            while (!context.CancellationToken.IsCancellationRequested)
            {
                // Create a new data point
                var point = new DataPoint
                {
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Value     = random.NextDouble() * 100
                };

                // Send the point to the client
                await responseStream.WriteAsync(point);

                // Wait for the specified interval or cancellation
                await Task.Delay(request.IntervalMs, context.CancellationToken);
            }
        }
    }
}
