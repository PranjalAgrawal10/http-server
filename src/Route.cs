namespace codecrafters_http_server;
public sealed class Route {
    private readonly RouteSegment[] _segments;
    private Route(RouteSegment[] segments) { _segments = segments; }
    public RouteSegment this[int index] => _segments[index];
    public int Length => _segments.Length;
    public bool ContainsParameters => _segments.Any(x => x.IsParameter);
    public IEnumerable<RouteSegment> ParametersSegments =>
        _segments.Where(x => x.IsParameter);
    public static bool TryParse(string value, out Route route) {
        if (value == "/") {
            route =
                new Route([new RouteSegment { IsParameter = false, Value = "/" }]);
            return true;
        }
        var segments = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var routeSegments = new RouteSegment[segments.Length];
        for (var i = 0; i < segments.Length; i++) {
            var segment = segments[i];
            var isParameter = segment.StartsWith('{') && segment.EndsWith('}');
            routeSegments[i] = new RouteSegment {
                IsParameter = isParameter, Value = isParameter ? null : segment,
                ParameterName = isParameter ? segment.Trim('{', '}') : null
            };
        }
        route = new Route(routeSegments);
        return true;
    }
}
public sealed class RouteSegment : IEquatable<RouteSegment> {
    public string? Value { get; init; }
    public required bool IsParameter { get; set; }
    public string? ParameterName { get; set; }
    public bool Equals(RouteSegment? other) {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return IsParameter || Value == other.Value;
    }
}