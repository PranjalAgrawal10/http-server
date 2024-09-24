using System.Buffers;
using System.Net.Sockets;
using System.Text;
namespace codecrafters_http_server;


public sealed class RequestReader {
  private readonly Socket _socket;
  public RequestReader(Socket socket) { _socket = socket; }
  public async Task<Request> ReadAsync(CancellationToken cancellationToken) {
    var buffer = new byte[4096];
    var requestBytesWriter = new ArrayBufferWriter<byte>();
    while (true) {
      var readBytes =
          await _socket.ReceiveAsync(buffer.AsMemory(), cancellationToken);
      if (readBytes == 0)
        break;
      requestBytesWriter.Write(buffer[..readBytes].AsSpan());
      if (readBytes < buffer.Length)
        break;
    }
    var bytes = requestBytesWriter.WrittenMemory;
    var requestLineBytes = ReadUntilNewLine(bytes);
    var requestLine = Encoding.ASCII.GetString(requestLineBytes.Span);
    var parts =
        requestLine.Split(' ', StringSplitOptions.TrimEntries |
                                   StringSplitOptions.RemoveEmptyEntries);
    bytes = bytes[requestLineBytes.Length..];
    var headers = new Dictionary<string, string>();
    while (true) {
      var headersBytes = ReadUntilNewLine(bytes);
      bytes = bytes[headersBytes.Length..];
      var headerString = Encoding.ASCII.GetString(headersBytes.Span);
      if (headerString == "\r\n")
        break;
      var (key, value) = ParseHeader(headerString);
      headers[key] = value;
    }
    var bodyBytes = ReadUntilNewLine(bytes);
    return new Request { Headers = headers, Method = new HttpMethod(parts[0]),
                         Url = parts[1], Content = bodyBytes.ToArray() };
  }
  private static (string Key, string Value) ParseHeader(string s) {
    var separatorIndex = s.IndexOf(':');
    return (s[..separatorIndex],
            s[(separatorIndex + 1)..].Trim('\r', '\n', ' '));
  }
  private ReadOnlyMemory<byte> ReadUntilNewLine(ReadOnlyMemory<byte> buffer) {
    if (buffer.IsEmpty)
      return buffer;
    var i = 0;
    while (i < buffer.Length && buffer.Span[i] != '\r' && buffer.Span[i + 1] != '\n') {
      i++;
    }
    return buffer[..(i + 2)];
  }
}
