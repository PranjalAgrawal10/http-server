namespace codecrafters_http_server;

public sealed class HttpContext
{
    public required Request Request { get; init; }
    public required Response Response { get; init; }
}