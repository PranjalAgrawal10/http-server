namespace codecrafters_http_server;
public sealed class Request {
    public required HttpMethod Method { get; init; }
    public required string Url { get; init; }
    public required Dictionary<string, string> Headers { get; init; }
    public required byte[] Content { get; init; }
}