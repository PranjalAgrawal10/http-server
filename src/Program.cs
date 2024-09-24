using System.Net;
using System.Text;
using codecrafters_http_server;

string? directory = null;

if (args.Length > 0 && args[0] == "--directory") {
    directory = args[1];
}

var server = new Server();

server.RegisterEndpoint("/", HttpMethod.Get, context =>
{
    context.Response.StatusCode = HttpStatusCode.OK;
    return Task.CompletedTask;
});

server.RegisterEndpoint("/echo/{s}", HttpMethod.Get, context => {
    context.Response.StatusCode = HttpStatusCode.OK;
    var s = context.Request.UrlParams["s"];
    context.Response.Headers.Add("Content-Type", "text/plain");
    context.Response.Headers.Add("Content-Length", s.Length.ToString());
    context.Response.Content = Encoding.UTF8.GetBytes(s);
    return Task.CompletedTask;
});

server.RegisterEndpoint("/user-agent", HttpMethod.Get, context => {
    context.Response.StatusCode = HttpStatusCode.OK;
    var userAgent = context.Request.Headers["User-Agent"];
    context.Response.Headers.Add("Content-Type", "text/plain");
    context.Response.Headers.Add("Content-Length", userAgent.Length.ToString());
    context.Response.Content = Encoding.UTF8.GetBytes(userAgent);
    return Task.CompletedTask;
});


server.RegisterEndpoint("/files/{file-name}", HttpMethod.Get, async context => {

    if (directory is null)
    {
        context.Response.StatusCode = HttpStatusCode.NotFound;
        return;
    }

    var fileName = context.Request.UrlParams["file-name"];
    var path = Path.Combine(directory, fileName);
    if (!File.Exists(path))
    {
        context.Response.StatusCode = HttpStatusCode.NotFound;
        return;
    }

    var content = await File.ReadAllTextAsync(path);
    context.Response.Headers.Add("Content-Type", "application/octet-stream");
    context.Response.Headers.Add("Content-Length", content.Length.ToString());
    context.Response.Content = Encoding.UTF8.GetBytes(content);
    context.Response.StatusCode = HttpStatusCode.OK;
});

server.RegisterEndpoint("/files/{file-name}", HttpMethod.Post, async context => {
        if (directory is null) {
            context.Response.StatusCode = HttpStatusCode.NotFound;
            return;
        }
        var fileName = context.Request.UrlParams["file-name"];
        var path = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(path, context.Request.Content);
        context.Response.StatusCode = HttpStatusCode.Created;
    });

await server.RunAsync(CancellationToken.None);