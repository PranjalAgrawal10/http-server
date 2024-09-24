using System.Net;
using System.Net.Sockets;
using codecrafters_http_server;

var server = new Server();
server.RegisterEndpoint("/", context => {
    context.Response.StatusCode = HttpStatusCode.OK;
    return Task.CompletedTask;
});
await server.RunAsync(CancellationToken.None);