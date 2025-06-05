using System.Text.Json;
using System.Text.Json.Serialization;
using Yarp.ReverseProxy.Configuration;

namespace Yarp.Gateway.Public.Extensions
{
    public class TokenExchangeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenExchangeMiddleware> _logger;
        private readonly RouteAudienceResolver _audienceResolver;

        public TokenExchangeMiddleware(
            RequestDelegate next,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<TokenExchangeMiddleware> logger,
            RouteAudienceResolver audienceResolver)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _audienceResolver = audienceResolver;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();

                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var originalToken = authHeader["Bearer ".Length..].Trim();
                    var audience = GetAudienceFromRoute(context);

                    if (!string.IsNullOrEmpty(audience))
                    {
                        var exchangedToken = await ExchangeTokenAsync(originalToken, audience, context.RequestAborted);

                        if (!string.IsNullOrEmpty(exchangedToken))
                        {
                            // Replace the Authorization header with the exchanged token
                            context.Request.Headers["Authorization"] = $"Bearer {exchangedToken}";
                            _logger.LogInformation($"Token exchanged successfully for audience '{audience}'.");
                        }
                        else
                        {
                            _logger.LogWarning("Token exchange failed or returned empty token.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Audience not found for token exchange.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token exchange middleware.");
            }

            await _next(context);
        }

        private string? GetAudienceFromRoute(HttpContext context)
        {
            return _audienceResolver.ResolveAudience(context.Request.Path);
        }

        private async Task<string?> ExchangeTokenAsync(string subjectToken, string audience, CancellationToken cancellationToken)
        {
            var keycloakUrl = _configuration["Keycloak:auth-server-url"]?.TrimEnd('/');
            var realm = _configuration["Keycloak:realm"];
            var tokenEndpoint = $"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token";

            // Use gateway client credentials here — make sure these are correct in your appsettings.json or environment
            var clientId = _configuration["Keycloak:resource"];
            var clientSecret = _configuration["Keycloak:credentials:secret"];

            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:token-exchange" },
                { "subject_token", subjectToken },
                { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
                { "requested_token_type", "urn:ietf:params:oauth:token-type:access_token" },
                { "audience", audience },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token exchange failed. Response: {Content}", response.Content);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(stream, cancellationToken: cancellationToken);

            return tokenResponse?.AccessToken;
        }

        private class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }
    }
}
