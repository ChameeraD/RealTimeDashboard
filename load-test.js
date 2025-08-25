import grpc from "k6/net/grpc";
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const connectionTime = new Trend('connection_time');
const messageLatency = new Trend('message_latency');

export const options = {
    stages: [
        { duration: '30s', target: 10 },   // Ramp up to 10 users
        { duration: '1m', target: 50 },    // Ramp up to 50 users
        { duration: '2m', target: 100 },   // Ramp up to 100 users
        { duration: '2m', target: 100 },   // Stay at 100 users
        { duration: '1m', target: 0 },     // Ramp down to 0 users
    ],
    thresholds: {
        'errors': ['rate<0.1'],           // Error rate should be less than 10%
        'connection_time': ['p95<1000'],  // 95% of connections should be under 1s
        'message_latency': ['p95<500'],   // 95% of messages should have latency under 500ms
    },
};

export default function () {
    const client = new grpc.Client();

    // Load the proto file
    client.load(["./Protos"], "dashboard.proto");

    // Measure connection time
    const startTime = Date.now();

    try {
        // Connect to the gRPC server
        client.connect("localhost:5001", {
            plaintext: false,  // Use TLS
            timeout: '10s'     // Connection timeout
        });

        const connectTime = Date.now() - startTime;
        connectionTime.add(connectTime);

        // Create subscription request
        const request = {
            source_id: "load-test-sensor",
            interval_ms: 100,
        };

        // Start streaming
        const stream = client.invoke("dashboard.DashboardService/Subscribe", request, {
            timeout: '30s'
        });

        let messageCount = 0;
        const streamStartTime = Date.now();

        // Listen for data
        stream.on("data", (data) => {
            messageCount++;

            // Calculate message latency (simplified - in real scenario you'd track individual messages)
            const latency = Date.now() - streamStartTime;
            messageLatency.add(latency);

            // Limit the number of messages per stream to avoid overwhelming the test
            if (messageCount >= 50) {
                stream.close();
            }
        });

        // Handle stream errors
        stream.on("error", (error) => {
            console.error(`Stream error: ${error.message}`);
            errorRate.add(1);
        });

        // Handle stream end
        stream.on("end", () => {
            console.log(`Stream ended after ${messageCount} messages`);
        });

        // Wait for stream to complete or timeout
        sleep(5);

        // Close the stream
        stream.close();

        // Verify we received some messages
        check(messageCount, {
            'received messages': (count) => count > 0,
            'message count reasonable': (count) => count >= 10 && count <= 100,
        });

    } catch (error) {
        console.error(`Connection error: ${error.message}`);
        errorRate.add(1);
    } finally {
        // Always close the client
        client.close();
    }

    // Add some delay between iterations
    sleep(1);
}

// Setup function to verify the gRPC server is accessible
export function setup() {
    console.log('Setting up load test...');
    console.log('Make sure the gRPC server is running on localhost:5001');
    console.log('The server should be configured with HTTP/2 and TLS');

    // You could add a health check here if needed
    return {};
}

// Teardown function
export function teardown(data) {
    console.log('Load test completed');
    console.log('Check the metrics above for performance analysis');
} 