using System.Net;
using System.Text;
using codecrafters_http_server;

var server = new Server();
server.RegisterEndpoint("/echo/{s}", context => {
    context.Response.StatusCode = HttpStatusCode.OK;
    var s = context.Request.UrlParams["s"];
    context.Response.Headers.Add("Content-Type", "text/plain");
    context.Response.Headers.Add("Content-Length", s.Length.ToString());
    context.Response.Content = Encoding.UTF8.GetBytes(s);
    return Task.CompletedTask;
});

server.RegisterEndpoint("/user-agent", context => {
    context.Response.StatusCode = HttpStatusCode.OK;
    var userAgent = context.Request.Headers["User-Agent"];
    context.Response.Headers.Add("Content-Type", "text/plain");
    context.Response.Headers.Add("Content-Length", userAgent.Length.ToString());
    context.Response.Content = Encoding.UTF8.GetBytes(userAgent);
    return Task.CompletedTask;
});


server.RegisterEndpoint("/", context => {
    context.Response.StatusCode = HttpStatusCode.OK;
    return Task.CompletedTask;
});
await server.RunAsync(CancellationToken.None);