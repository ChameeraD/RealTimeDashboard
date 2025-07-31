import grpc from "k6/net/grpc";
export default () => {
    const client = new grpc.Client();
    client.load(["./Protos"], "dashboard.proto");
    client.connect("localhost:5001", { plaintext: true });
    const stream = client.invoke("dashboard.DashboardService/Subscribe", {
        source_id: "sensor-1",
        interval_ms: 100,
    });
    stream.on("data", () => { });
    client.close();
}; 