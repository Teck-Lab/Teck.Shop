using System.Text.RegularExpressions;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Teck.Shop.SharedKernel.Infrastructure;
using Teck.Shop.SharedKernel.Infrastructure.Auth;
using Teck.Shop.SharedKernel.Infrastructure.Options;
using Yarp.Gateway.Public.Extensions;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Swagger;
using Yarp.ReverseProxy.Swagger.Extensions;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var appOptions = new AppOptions();
builder.Configuration.GetSection(AppOptions.Section).Bind(appOptions);

// Add shared base infrastructure (adds logging, cors, forwarded headers, correlation id, etc)
builder.AddBaseInfrastructure(appOptions);

// Add keycloak Authentication.
KeycloakAuthenticationOptions keycloakOptions = builder.Configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>()!;
builder.Services.AddKeycloak(builder.Configuration, builder.Environment, keycloakOptions);

// REVERSE PROXY
var configuration = builder.Configuration.GetSection("ReverseProxy");
builder.Services.AddReverseProxy()
    .LoadFromConfig(configuration)
    .AddSwagger(configuration)
    .AddServiceDiscoveryDestinationResolver()
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            var httpContext = transformContext.HttpContext;

            // 🔁 Forward Correlation ID if present
            if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId.ToString());
            }

            // 🔐 Forward access token if not anonymous
            var endpoint = httpContext.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null;

            if (!allowAnonymous)
            {
                var token = await httpContext.GetTokenAsync("access_token");

                if (!string.IsNullOrEmpty(token))
                {
                    transformContext.ProxyRequest.Headers.Authorization = new("Bearer", token);
                }
            }
        });
    });

// Build app
var app = builder.Build();

// Use shared base infrastructure middleware (cors, forwarded headers, auth, correlation id, etc)
app.UseBaseInfrastructure();

app.UseAuthentication();
app.UseAuthorization();

// REVERSE PROXY
app.MapReverseProxy();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapScalarApiReference("/", options =>
{
    var config = app.Services.GetRequiredService<IOptionsMonitor<ReverseProxyDocumentFilterConfig>>().CurrentValue;

    foreach (var cluster in config.Clusters)
    {
        foreach (var destination in cluster.Value.Destinations)
        {
            var destinationName = destination.Key;

            var swaggers = destination.Value.Swaggers;
            if (swaggers == null)
                continue;
            foreach (var swagger in swaggers)
            {
                var service = swagger.PrefixPath.Trim('/');
                foreach (var path in swagger.Paths)
                {
                    var versionMatch = Regex.Match(path, @"v\d+");
                    var version = versionMatch.Success ? versionMatch.Value : "v1";
                    var docName = $"{service}-{version}".ToLowerInvariant();

                    options.AddDocument(
                        docName,
                        $"{cluster.Key[0].ToString().ToUpperInvariant()}{cluster.Key.Substring(1).ToLowerInvariant()} {version}",
                        $"/{cluster.Key.ToLowerInvariant()}/openapi/{{documentName}}/openapi.json");
                }
            }
        }

    }
    options
    .AddPreferredSecuritySchemes("oAuth2")
    .AddAuthorizationCodeFlow("oAuth2", flow =>
    {
        flow.ClientId = "scalar-ui";
        flow.Pkce = Pkce.Sha256; // Enable PKCE
    });
});

await app.RunAsync();