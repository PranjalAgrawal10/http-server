using System.Diagnostics.CodeAnalysis;
namespace codecrafters_http_server;
public sealed class RouteTrie {
  private readonly Node _root = new(null!);
  public bool Insert(Route route, HttpMethod method, RequestHandler handler) {
    var current = _root;
    var pathIndex = 0;
    while (pathIndex < route.Length) {
      var next = current.Children.Find(x => x.Segment.Equals(route[pathIndex]));
      if (next is null)
        break;
      current = next;
      pathIndex++;
    }
    if (pathIndex >= route.Length && current.Handlers.ContainsKey(method)) 
      return false;
    
    while (pathIndex < route.Length) {
      var newNode = new Node(route[pathIndex]);
      current.Children.Add(newNode);
      current = newNode;
      pathIndex++;
    }
    
    current.SetHandler(method, handler);
    return true;
  }
  public bool TryGetValue(Route route, HttpMethod method,
                          out RequestHandler? handler) {
    handler = default;
    var current = _root;
    var pathIndex = 0;
    while (pathIndex < route.Length) {
      var next = current.Children.Find(x => x.Segment.Equals(route[pathIndex]));
      if (next is null)
        return false;
      current = next;
      route[pathIndex].IsParameter = current.Segment.IsParameter;
      route[pathIndex].ParameterName = current.Segment.ParameterName;
      pathIndex++;
    }
    return current.Handlers.TryGetValue(method, out handler);
  }
  private sealed class Node {
    [SetsRequiredMembers]
    public Node(RouteSegment segment) {
      Segment = segment;
      Handlers = [];
      Children = [];
    }
    public required RouteSegment Segment { get; init; }
    public required Dictionary<HttpMethod, RequestHandler> Handlers { get; init; }
    public required List<Node> Children { get; init; }
    public bool SetHandler(HttpMethod method, RequestHandler handler) {
      return Handlers.TryAdd(method, handler);
    }
  }
}