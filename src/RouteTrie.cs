using System.Diagnostics.CodeAnalysis;
namespace codecrafters_http_server;
public sealed class RouteTrie {
    private readonly Node _root = new(null!, null);
    public bool Insert(Route route, RequestHandler handler) {
        var current = _root;
        var pathIndex = 0;
        while (pathIndex < route.Length) {
            var next = current.Children.Find(x => x.Segment.Equals(route[pathIndex]));
            if (next is null)
                break;
            current = next;
            pathIndex++;
        }
        if (pathIndex >= route.Length) {
            return false;
        }
        while (pathIndex < route.Length) {
            var newNode = new Node(route[pathIndex], default);
            current.Children.Add(newNode);
            current = newNode;
            pathIndex++;
        }
        current.Handler = handler;
        return true;
    }
    public bool TryGetValue(Route route, out RequestHandler? handler) {
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
        handler = current.Handler;
        return true;
    }
    private sealed class Node {
        [SetsRequiredMembers]
        public Node(RouteSegment segment, RequestHandler? handler) {
            Segment = segment;
            Handler = handler;
            Children = [];
        }
        public required RouteSegment Segment { get; init; }
        public RequestHandler? Handler { get; set; }
        public required List<Node> Children { get; init; }
    }
}