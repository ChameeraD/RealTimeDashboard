# Real-Time Dashboard

A production-ready real-time dashboard application built with Blazor Server and gRPC server-streaming for low-latency data updates, featuring Syncfusion Blazor Charts for data visualization, comprehensive error handling, and authentication support.

## ✨ Features

- **Real-time streaming**: Server pushes data updates to browser via gRPC
- **HTTP/2 with TLS**: Secure, always-open connection with compression
- **Interactive UI**: Start/stop streaming with live status updates and error handling
- **Data visualization**: Syncfusion Blazor Charts for real-time line charts
- **Responsive design**: Bootstrap-based layout with live data table
- **Authentication**: JWT-based authentication with development fallback
- **Error handling**: Comprehensive error handling with retry logic and user feedback
- **Health monitoring**: Built-in health checks and connection monitoring
- **Performance optimized**: Data batching, memory management, and load testing support

## 🏗️ Project Structure

```
RealTimeDashboard/
├─ DashboardApp/          # Blazor Server UI
│  ├─ Components/
│  │  ├─ Pages/
│  │  │  └─ LiveData.razor  # Real-time data display with charts
│  │  └─ Layout/
│  ├─ Program.cs
│  └─ appsettings.json
├─ DashboardGrpc/         # gRPC backend service
│  ├─ Services/
│  │  └─ DashboardStreamService.cs  # Server-streaming implementation
│  ├─ Protos/
│  │  └─ dashboard.proto   # Protocol buffer definitions
│  ├─ Program.cs
│  └─ appsettings.json
├─ load-test.js           # k6 load testing script
├─ TROUBLESHOOTING.md     # Comprehensive troubleshooting guide
└─ RealTimeDashboard.sln
```

## 🚀 Quick Start

### Prerequisites

- **.NET 9.0 SDK** or later
- **HTTPS development certificate** (needed for HTTP/2 + TLS in dev)
- **Syncfusion Blazor Charts license** (free community license available)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/ChameeraD/RealTimeDashboard.git
   cd RealTimeDashboard
   ```

2. **Trust the development certificate**:
   ```bash
   dotnet dev-certs https --trust
   ```

3. **Restore packages**:
   ```bash
   dotnet restore
   ```

### Running the Application

1. **Start the gRPC server**:
   ```bash
   cd DashboardGrpc
   dotnet run
   ```
   The gRPC server will start on `https://localhost:5001`

2. **Start the Blazor application** (in a new terminal):
   ```bash
   cd DashboardApp
   dotnet run
   ```
   The Blazor app will start on `http://localhost:5011`

3. **Access the dashboard**:
   - Navigate to the Blazor app URL
   - Click "Live Data" in the navigation menu
   - Click "Start" to begin streaming data
   - View real-time charts and data table

## 🔐 Authentication & Security

### Development Mode
- Uses a simple development authentication handler
- Automatically authenticates as "dev-user"
- No JWT tokens required

### Production Mode
- JWT Bearer token authentication
- Configurable issuer, audience, and signing key
- Secure HTTPS communication with HTTP/2

### Configuration
Update `appsettings.json` in both projects:
```json
{
  "Jwt": {
    "Issuer": "https://your-domain.com",
    "Audience": "https://your-domain.com",
    "Key": "your-super-secret-key-with-at-least-32-characters"
  }
}
```

## 📊 Architecture

### gRPC Service
- **HTTP/2 with TLS** for secure communication
- **Server-streaming pattern** for continuous data flow
- **Authentication middleware** with JWT support
- **Health checks** at `/health` endpoint
- **Compression** enabled for optimal performance
- **Comprehensive logging** and error handling

### Blazor UI with Syncfusion Charts
- **Typed gRPC client** with retry policies
- **Real-time data binding** and UI updates
- **Error handling** with user-friendly messages
- **Connection monitoring** and health checks
- **Performance optimization** with data point limiting

## 🔧 Configuration

### gRPC Server Settings
```json
{
  "Grpc": {
    "MaxReceiveMessageSize": 1048576,
    "MaxSendMessageSize": 1048576,
    "EnableDetailedErrors": true,
    "ResponseCompressionAlgorithm": "gzip",
    "ResponseCompressionLevel": "Fastest"
  }
}
```

### Blazor App Settings
```json
{
  "Grpc": {
    "ServerUrl": "https://localhost:5001"
  }
}
```

## 📈 Performance & Monitoring

### Built-in Metrics
- Connection uptime tracking
- Message count monitoring
- Real-time status updates
- Error rate tracking

### Load Testing
Use the included k6 load test script:
```bash
# Install k6
brew install k6  # macOS
choco install k6  # Windows

# Install gRPC extension
k6 install xk6-grpc

# Run load test
k6 run load-test.js --vus 100 --duration 30s
```

### Performance Monitoring
```bash
# Monitor .NET performance counters
dotnet-counters monitor --process-id <PID>

# Check health endpoint
curl https://localhost:5001/health
```

## 🐛 Troubleshooting

### Common Issues
1. **HTTPS Certificate Errors**: Run `dotnet dev-certs https --trust`
2. **Connection Refused**: Verify gRPC server is running on port 5001
3. **Authentication Errors**: Check JWT configuration in production
4. **Performance Issues**: Monitor data point limits and compression

### Debug Mode
Enable detailed logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Grpc": "Debug",
      "DashboardGrpc": "Debug"
    }
  }
}
```

### Getting Help
- Check the [Troubleshooting Guide](TROUBLESHOOTING.md)
- Review console logs for detailed error messages
- Verify .NET version compatibility
- Check package references and dependencies

## 🚀 Deployment

### Production Considerations
1. **SSL Certificates**: Use valid SSL certificates
2. **Firewall Rules**: Open required ports (5001 for gRPC)
3. **Load Balancer**: Ensure HTTP/2 support
4. **Environment Variables**: Set production JWT keys
5. **Monitoring**: Implement application monitoring and alerting

### Docker Support
The application can be containerized using standard .NET Docker images. Ensure your container environment supports HTTP/2 and TLS.

## 📚 Additional Resources

- [gRPC for .NET Documentation](https://docs.microsoft.com/en-us/aspnet/core/grpc/)
- [Blazor Server Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Syncfusion Blazor Charts](https://blazor.syncfusion.com/documentation/chart/)
- [k6 Load Testing](https://k6.io/docs/)
- [HTTP/2 and TLS with Kestrel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Built with [.NET 9.0](https://dotnet.microsoft.com/)
- UI components from [Syncfusion Blazor](https://blazor.syncfusion.com/)
- Load testing with [k6](https://k6.io/)
- Real-time communication via [gRPC](https://grpc.io/) 