namespace Yarp.Gateway.Public.Extensions
{
    public class RouteAudienceResolver
    {
        private readonly Dictionary<string, string> _audienceByRoutePrefix = new();

        public RouteAudienceResolver(IConfiguration configuration)
        {
            var proxySection = configuration.GetSection("ReverseProxy");
            var routeSection = proxySection.GetSection("Routes");
            var clusterSection = proxySection.GetSection("Clusters");

            foreach (var route in routeSection.GetChildren())
            {
                var routeId = route.Key;
                var path = route.GetSection("match")["path"];

                if (string.IsNullOrEmpty(path))
                    continue;

                // Extract clusterId, get destination's metadata
                var clusterId = route["clusterId"];
                var audience = clusterSection
                    .GetSection(clusterId)
                    .GetSection("Destinations")
                    .GetChildren()
                    .Select(d => d.GetSection("Metadata")?["Audience"])
                    .FirstOrDefault(a => !string.IsNullOrEmpty(a));

                if (!string.IsNullOrEmpty(audience))
                {
                    // Normalize path (remove catch-all etc)
                    var basePath = path.Split('/')[0];
                    _audienceByRoutePrefix[basePath] = audience!;
                }
            }
        }

        public string? ResolveAudience(PathString path)
        {
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments is { Length: > 0 } && _audienceByRoutePrefix.TryGetValue(segments[0], out var audience))
            {
                return audience;
            }

            return null;
        }
    }
}
