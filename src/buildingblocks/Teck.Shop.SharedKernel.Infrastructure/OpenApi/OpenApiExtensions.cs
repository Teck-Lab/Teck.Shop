using System.Runtime.InteropServices;
using FastEndpoints;
using FastEndpoints.Swagger;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;
using Microsoft.AspNetCore.Builder;
using NSwag;
using NSwag.Generation.Processors.Security;
using Scalar.AspNetCore;
using Teck.Shop.SharedKernel.Infrastructure.Options;

namespace Teck.Shop.SharedKernel.Infrastructure.OpenApi
{
    /// <summary>
    /// The open api extensions.
    /// </summary>
    public static class OpenApiExtensions
    {
        /// <summary>
        /// Add open api infrastructure.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="openApiOptions"></param>
        /// <param name="appOptions">The app options.</param>
        public static void AddOpenApiInfrastructure(
            this WebApplicationBuilder builder,
            OpenApiOptions openApiOptions,
            AppOptions appOptions)
        {
            var keycloakOptions = builder.Configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>();

            List<Action<DocumentOptions>> documentOptions = new List<Action<DocumentOptions>>();

            foreach (int apiVersion in openApiOptions.ApiVersions)
            {
                Action<DocumentOptions> document = new(options =>
                {
                    options.EnableJWTBearerAuth = false;
                    options.MaxEndpointVersion = apiVersion;
                    options.DocumentSettings = setting =>
                    {
                        setting.Version = $"v{apiVersion}";
                        setting.Title = $"{appOptions.Name} API";
                        setting.DocumentName = $"{openApiOptions.UrlPath.ToLowerInvariant()}-v{apiVersion}";
                        setting.Description = $"Open API Documentation of {appOptions.Name} API.";

                        if (keycloakOptions != null)
                        {
                            setting.AddSecurity("oAuth2", AddOAuthScheme(keycloakOptions.KeycloakTokenEndpoint, keycloakOptions.KeycloakUrlRealm + "protocol/openid-connect/auth", keycloakOptions.KeycloakTokenEndpoint, openApiOptions.UrlPath.ToLowerInvariant()));
                        }

                        setting.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("oAuth2"));
                    };
                });


                documentOptions.Add(document);
            }

            foreach (ref Action<DocumentOptions> option in CollectionsMarshal.AsSpan(documentOptions))
            {
                builder.Services.SwaggerDocument(option);
            }
        }

        /// <summary>
        /// Use open api infrastructure.
        /// </summary>
        /// <param name="app">The app.</param>
        /// <param name="openApiOptions"></param>
        /// <param name="appOptions"></param>
        public static void UseOpenApiInfrastructure(this WebApplication app, OpenApiOptions openApiOptions, AppOptions appOptions)
        {
            app.UseSwaggerGen(document =>
            {
                document.Path = "/openapi/{documentName}/openapi.json";
                document.PostProcess = (doc, httpReq) =>
                {
                    doc.Servers.Clear();
                    doc.Servers.Add(new NSwag.OpenApiServer { Url = $"/{openApiOptions.UrlPath.ToLowerInvariant()}" });
                };
            });
            app.MapScalarApiReference("docs", options =>
            {
                options.OpenApiRoutePattern = "/openapi/{documentName}/openapi.json";
                foreach (var apiVersion in openApiOptions.ApiVersions)
                {
                    options.AddDocument($"{openApiOptions.UrlPath.ToLowerInvariant()}-v{apiVersion}", $"{appOptions.Name} API v{apiVersion}");
                }

                options
                    .AddPreferredSecuritySchemes("oAuth2")
                    .AddAuthorizationCodeFlow("oAuth2", flow =>
                {
                    flow.ClientId = "scalar-ui";
                    flow.Pkce = Pkce.Sha256; // Enable PKCE
                });
            });
        }

        private static OpenApiSecurityScheme AddOAuthScheme(string tokenUrl, string authorizationUrl, string refreshUrl, string audience)
        {
            return new OpenApiSecurityScheme()
            {
                In = OpenApiSecurityApiKeyLocation.Header,
                Type = OpenApiSecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {

                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = authorizationUrl,
                        TokenUrl = tokenUrl,
                        RefreshUrl = refreshUrl,

                        // Should look into fetching this from keycloak.
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID Connect scope" },
                            { "profile", "User profile scope" },
                            { audience, $"Access to {audience}" }
                        }
                    }
                }
            };
        }
    }
}
