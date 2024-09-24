using System.Net;
namespace codecrafters_http_server;
public sealed class Response {
    public required HttpStatusCode StatusCode { get; set; }
    public required Dictionary<string, string> Headers { get; init; }
    public required byte[] Content { get; set; }
}