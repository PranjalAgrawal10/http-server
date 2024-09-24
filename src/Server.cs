using System.Net;
using System.Net.Sockets;
namespace codecrafters_http_server;
public delegate Task RequestHandler(HttpContext context);

public sealed class Server {
  
  private readonly RouteTrie _handlersWithParameters = new();
  private readonly Dictionary<string, RequestHandler> _handlers = new();
  
  public async Task RunAsync(CancellationToken cancellationToken) {
    using var server = new TcpListener(IPAddress.Any, 4221);
    Console.WriteLine("Start listening on 4221 port");
    server.Start();
    while (true) {
      var socket = await server.AcceptSocketAsync(cancellationToken);
      _ = HandleRequest(socket, cancellationToken);
    }
    
  }
  private async Task HandleRequest(Socket socket, CancellationToken cancellationToken)
  {
    try {
      var request = await ReadRequestAsync(socket, cancellationToken);
      var response = new Response { StatusCode = HttpStatusCode.Accepted,
                                    Headers = [], Content = [] };
      var context = new HttpContext { Request = request, Response = response };
      
      if (_handlers.TryGetValue(request.Url, out var handler)) {
        await handler(context);
      } else if (Route.TryParse(request.Url, out var route) &&
                 _handlersWithParameters.TryGetValue(route, request.Method, out handler))
      {
        foreach (var segment in route.ParametersSegments) {
          request.UrlParams[segment.ParameterName!] = segment.Value!;
        }
        await handler!(context);
        
      } else {
        response.StatusCode = HttpStatusCode.NotFound;
      }
      
      await WriteResponseAsync(socket, response, cancellationToken);
      
    } finally {
      socket.Dispose();
    }
  }
  
  private static async Task<Request>
  ReadRequestAsync(Socket socket, CancellationToken cancellationToken) {
    var reader = new RequestReader(socket);
    return await reader.ReadAsync(cancellationToken);
  }
  private static async
      Task WriteResponseAsync(Socket socket, Response response,
                              CancellationToken cancellationToken) {
    var writer = new ResponseWriter(socket);
    await writer.WriteResponseAsync(response, cancellationToken);
  }
  public void RegisterEndpoint(string route, HttpMethod method, RequestHandler handler) {
    if (!Route.TryParse(route, out var r)) 
      throw new ArgumentException("Invalid route", nameof(route));

    var insertResult = r.ContainsParameters
                           ? _handlersWithParameters.Insert(r, method, handler)
                           : _handlers.TryAdd(route, handler);
    if (!insertResult) {
      throw new ArgumentException("Route already registered", nameof(route));
    }
  }
}