# Troubleshooting Guide

This guide addresses common issues you might encounter when setting up and running the Real-Time Dashboard with Blazor and gRPC.

## Common Issues and Solutions

### 1. HTTPS Development Certificate Issues

**Problem**: `dotnet run` fails with HTTPS certificate errors.

**Solution**:
```bash
# Trust the development certificate
dotnet dev-certs https --trust

# Verify the certificate is trusted
dotnet dev-certs https --check
```

**Alternative**: If you continue having issues, you can temporarily disable HTTPS in development by modifying `Program.cs`:
```csharp
// Comment out or remove this line in development
// opt.UseHttps();
```

### 2. gRPC Connection Refused

**Problem**: Blazor app can't connect to gRPC server with "Connection refused" error.

**Solutions**:
1. **Verify gRPC server is running**:
   ```bash
   cd DashboardGrpc
   dotnet run
   ```
   Look for: `Now listening on: https://localhost:5001`

2. **Check port conflicts**:
   ```bash
   # On macOS/Linux
   lsof -i :5001
   
   # On Windows
   netstat -an | findstr :5001
   ```

3. **Verify HTTP/2 support**:
   - Ensure you're using .NET 9.0 or later
   - Check that Kestrel is configured for HTTP/2 in `Program.cs`

### 3. Protocol Buffer Generation Errors

**Problem**: Build fails with protobuf-related errors.

**Solutions**:
1. **Clean and rebuild**:
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

2. **Verify protobuf tools**:
   ```bash
   dotnet list package | grep Grpc.Tools
   ```

3. **Check proto file paths** in `.csproj` files:
   ```xml
   <Protobuf Include="..\DashboardGrpc\Protos\dashboard.proto" GrpcServices="Client">
     <Link>Protos\dashboard.proto</Link>
   </Protobuf>
   ```

### 4. Authentication Errors

**Problem**: gRPC calls fail with "Permission denied" or authentication errors.

**Solutions**:
1. **Development mode**: The app includes a development authentication handler that always authenticates as "dev-user"
2. **Production mode**: Ensure JWT configuration is correct in `appsettings.json`
3. **Check authentication middleware order** in `Program.cs`:
   ```csharp
   app.UseAuthentication();  // Must come before UseAuthorization
   app.UseAuthorization();
   ```

### 5. Performance Issues

**Problem**: Dashboard feels sluggish or unresponsive.

**Solutions**:
1. **Limit data points**: The current implementation limits to 50 points for performance
2. **Enable compression**: gRPC compression is enabled by default
3. **Monitor memory usage**: Use `dotnet-counters monitor` to track performance
4. **Batch updates**: Consider batching multiple data points before calling `StateHasChanged()`

### 6. Browser Compatibility Issues

**Problem**: Dashboard doesn't work in certain browsers.

**Solutions**:
1. **HTTP/2 requirement**: gRPC requires HTTP/2, which is supported in all modern browsers
2. **TLS requirement**: HTTPS is required for HTTP/2 in browsers
3. **Fallback**: Consider implementing a SignalR fallback for older browsers if needed

### 7. Load Testing Issues

**Problem**: k6 load tests fail or show poor performance.

**Solutions**:
1. **Verify TLS configuration**: Ensure the load test uses the correct TLS settings
2. **Check server capacity**: Monitor server resources during load testing
3. **Adjust test parameters**: Modify the load test stages in `load-test.js`
4. **Use appropriate VU count**: Start with fewer virtual users and increase gradually

### 8. Deployment Issues

**Problem**: Application works locally but fails in production.

**Solutions**:
1. **Environment variables**: Ensure production environment variables are set
2. **Firewall rules**: Verify ports 5001 (gRPC) and your Blazor app port are open
3. **Load balancer configuration**: If using a load balancer, ensure it supports HTTP/2
4. **SSL certificates**: Use valid SSL certificates in production

## Debugging Tips

### Enable Detailed Logging

Add this to your `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Grpc": "Debug",
      "DashboardGrpc": "Debug",
      "DashboardApp": "Debug"
    }
  }
}
```

### Use Browser Developer Tools

1. **Network tab**: Monitor gRPC calls and their timing
2. **Console**: Check for JavaScript errors
3. **Performance tab**: Monitor rendering performance

### Monitor Server Resources

```bash
# Monitor .NET performance counters
dotnet-counters monitor --process-id <PID>

# Monitor system resources
top -p <PID>  # Linux/macOS
# or use Task Manager on Windows
```

## Performance Optimization

### 1. Data Batching
Instead of updating the UI for every data point, batch updates:
```csharp
private List<DataPoint> pendingUpdates = new();
private Timer? batchTimer;

private void BatchUpdate(DataPoint point)
{
    pendingUpdates.Add(point);
    
    if (batchTimer == null)
    {
        batchTimer = new Timer(_ => FlushBatch(), null, 100, Timeout.Infinite);
    }
}
```

### 2. Virtual Scrolling
For large datasets, implement virtual scrolling to only render visible items.

### 3. Memory Management
Regularly clean up old data and dispose of resources:
```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        cts?.Cancel();
        cts?.Dispose();
        batchTimer?.Dispose();
    }
    base.Dispose(disposing);
}
```

## Security Considerations

### 1. JWT Configuration
- Use strong, unique keys (at least 32 characters)
- Set appropriate expiration times
- Validate issuer and audience

### 2. Network Security
- Always use HTTPS in production
- Consider implementing mutual TLS for gRPC
- Use appropriate firewall rules

### 3. Input Validation
- Validate all gRPC request parameters
- Implement rate limiting if needed
- Log security-related events

## Getting Help

If you continue experiencing issues:

1. **Check the logs**: Look for detailed error messages in the console output
2. **Verify versions**: Ensure you're using compatible versions of .NET and packages
3. **Search issues**: Check the GitHub repository for similar issues
4. **Create issue**: If the problem persists, create a detailed issue with:
   - Error messages
   - Environment details (.NET version, OS)
   - Steps to reproduce
   - Logs and stack traces

## Useful Commands

```bash
# Check .NET version
dotnet --version

# List installed packages
dotnet list package

# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run specific project
dotnet run --project DashboardGrpc
dotnet run --project DashboardApp

# Check HTTPS certificate
dotnet dev-certs https --check

# Trust development certificate
dotnet dev-certs https --trust
``` 