using System.Buffers;
using System.Net;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
namespace codecrafters_http_server;

public sealed class ResponseWriter
{
    private readonly Socket _socket;

    public ResponseWriter(Socket socket)
    {
        _socket = socket;
    }

    public async Task WriteResponseAsync(Response response, CancellationToken cancellationToken)
    {
        var buffer = new ArrayBufferWriter<byte>();
        buffer.Write("HTTP/1.1 "u8.ToArray());
        buffer.Write(GetPhraseForStatusCode(response.StatusCode));
        buffer.Write("\r\n"u8.ToArray());
        if (response.Headers.TryGetValue("Content-Encoding", out var value))
        {
            if (Enum.TryParse<CompressionType>(value, ignoreCase: true,
                    out var compression))
            {
                response.Content = CompressContent(response.Content, compression);
            }
            else
            {
                response.Headers.Remove("Content-Encoding");
            }
        }

        if (response.Content.Length > 0)
        {
            response.Headers["Content-Length"] = response.Content.Length.ToString();
        }

        foreach (var header in response.Headers)
            buffer.Write(Encoding.ASCII.GetBytes($"{header.Key}: {header.Value}\r\n"));

        buffer.Write("\r\n"u8.ToArray());
        buffer.Write(response.Content);
        await _socket.SendAsync(buffer.WrittenMemory, cancellationToken);
    }

    private static byte[] GetPhraseForStatusCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.OK => "200 OK"u8.ToArray(), HttpStatusCode.Created => "201 Created"u8.ToArray(),
            HttpStatusCode.NotFound => "404 Not Found"u8.ToArray()
        };
    }

    private static byte[] CompressContent(byte[] content, CompressionType compressionType)
    {
        if (compressionType == CompressionType.Gzip)
        {
            var compressedStream = new MemoryStream();
            using (var compressor =
                   new GZipStream(compressedStream, CompressionMode.Compress))
            {
                var stream = new MemoryStream(content);
                stream.CopyTo(compressor);
            }

            return compressedStream.ToArray();
        }

        return content;
    }
}

public enum CompressionType
{
    Gzip
}
