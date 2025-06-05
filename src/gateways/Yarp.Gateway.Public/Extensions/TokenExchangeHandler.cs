using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Model;

public class TokenExchangeHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TokenExchangeHandler> _logger;

    public TokenExchangeHandler(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<TokenExchangeHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var originalToken = request.Headers.Authorization?.Parameter;
        if (string.IsNullOrEmpty(originalToken))
        {
            _logger.LogDebug("No Authorization header found, forwarding request without token exchange.");
            return await base.SendAsync(request, cancellationToken);
        }

        // Get audience from route metadata
        var audience = GetAudienceFromRouteMetadata();

        if (string.IsNullOrEmpty(audience))
        {
            _logger.LogDebug("No audience metadata found on route, forwarding original token.");
            return await base.SendAsync(request, cancellationToken);
        }

        var exchangedToken = await ExchangeTokenAsync(originalToken, audience, cancellationToken);

        if (!string.IsNullOrEmpty(exchangedToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", exchangedToken);
            _logger.LogInformation("Token exchanged successfully for audience {Audience}.", audience);
        }
        else
        {
            _logger.LogWarning("Token exchange failed. Forwarding original token.");
            // optionally fallback to original token or reject
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private string? GetAudienceFromRouteMetadata()
    {
        var context = _httpContextAccessor.HttpContext;
        var proxyFeature = context?.Features.Get<ReverseProxyFeature>();
        var metadata = proxyFeature?.Route?.Config?.Metadata;
        if (metadata != null && metadata.TryGetValue("Audience", out var audienceObj))
        {
            return audienceObj?.ToString();
        }
        return null;
    }

    private async Task<string?> ExchangeTokenAsync(string subjectToken, string audience, CancellationToken cancellationToken)
    {
        var keycloakUrl = _configuration["Keycloak:auth-server-url"]?.TrimEnd('/');
        var realm = _configuration["Keycloak:realm"];
        var clientId = _configuration["Keycloak:resource"];
        var clientSecret = _configuration["Keycloak:credentials:secret"];

        var tokenEndpoint = $"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token";

        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "urn:ietf:params:oauth:grant-type:token-exchange" },
            { "subject_token", subjectToken },
            { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
            { "audience", audience },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        var client = _httpClientFactory.CreateClient("TokenExchangeClient");
        var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Token exchange failed with status code {StatusCode}", response.StatusCode);
            return null;
        }

        var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(contentStream, cancellationToken: cancellationToken);
        return tokenResponse?.AccessToken;
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}
