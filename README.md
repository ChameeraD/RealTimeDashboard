# Real-Time Dashboard

A real-time dashboard application built with Blazor Server and gRPC server-streaming for low-latency data updates, featuring Syncfusion Blazor Charts for data visualization.

## Project Structure

```
RealTimeDashboard/
├─ DashboardApp/          # Blazor Server UI
│  ├─ Components/
│  │  ├─ Pages/
│  │  │  └─ LiveData.razor  # Real-time data display with charts
│  │  └─ Layout/
│  └─ Program.cs
├─ DashboardGrpc/         # gRPC backend service
│  ├─ Services/
│  │  └─ DashboardStreamService.cs  # Server-streaming implementation
│  ├─ Protos/
│  │  └─ dashboard.proto   # Protocol buffer definitions
│  └─ Program.cs
└─ RealTimeDashboard.sln
```

## Features

- **Real-time streaming**: Server pushes data updates to browser via gRPC
- **HTTP/2 with TLS**: Secure, always-open connection
- **Interactive UI**: Start/stop streaming with live status updates
- **Data visualization**: Syncfusion Blazor Charts for real-time line charts
- **Responsive design**: Bootstrap-based layout with live data table

## Prerequisites

- .NET 9.0 SDK
- HTTPS development certificate
- Syncfusion Blazor Charts license (free community license available)

## Running the Application

1. **Start the gRPC server**:
   ```bash
   cd DashboardGrpc
   dotnet run
   ```
   The gRPC server will start on `https://localhost:5001`

2. **Start the Blazor application**:
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

## Architecture

### gRPC Service
- Uses HTTP/2 with TLS for secure communication
- Implements server-streaming pattern
- Generates random data points every 500ms (configurable)
- Handles client disconnection gracefully

### Blazor UI with Syncfusion Charts
- Connects to gRPC service via typed client
- Displays real-time data in interactive line charts
- Shows latest values in a scrollable table
- Provides start/stop controls with status indicators
- Updates UI automatically with each new data point

## Protocol Buffer Definition

```protobuf
service DashboardService {
  rpc Subscribe (Subscription) returns (stream DataPoint);
}

message Subscription {
  string source_id  = 1;
  int32  interval_ms = 2;
}

message DataPoint {
  int64  timestamp = 1;
  double value     = 2;
}
```

## Chart Features

- **Real-time line chart**: Updates automatically as new data arrives
- **Time-based X-axis**: Shows timestamps in HH:mm:ss format
- **Responsive design**: Adapts to different screen sizes
- **Data markers**: Visual indicators for each data point
- **Performance optimized**: Limits to 50 data points for smooth rendering

## Development Notes

- The gRPC service generates random data for demonstration
- Data points are limited to 50 items for optimal chart performance
- The connection uses self-signed certificates for local development
- Both projects target .NET 9.0 for latest features
- Syncfusion Blazor Charts provides professional-grade charting capabilities 