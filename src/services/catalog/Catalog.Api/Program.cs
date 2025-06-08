using System.Reflection;
using Catalog.Api.Extensions;
using Catalog.Application;
using Catalog.Infrastructure;
using Teck.Shop.SharedKernel.Infrastructure;
using Teck.Shop.SharedKernel.Infrastructure.Caching;
using Teck.Shop.SharedKernel.Infrastructure.Endpoints;
using Teck.Shop.SharedKernel.Infrastructure.Idempotency;
using Teck.Shop.SharedKernel.Infrastructure.OpenApi;
using Teck.Shop.SharedKernel.Infrastructure.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

Assembly applicationAssembly = typeof(ICatalogApplication).Assembly;

var appOptions = new AppOptions();
var openApiOptions = new OpenApiOptions();

builder.Configuration.GetSection(AppOptions.Section).Bind(appOptions);
builder.Configuration.GetSection(OpenApiOptions.Section).Bind(openApiOptions);

builder.AddBaseInfrastructure(appOptions);
builder.AddCachingInfrastructure();
builder.AddCatalogInfrastructure(applicationAssembly);

builder.Services.AddFastEndpointsInfrastructure(applicationAssembly);
builder.AddMediatorInfrastructure(applicationAssembly);
builder.Services.AddIdempotencySupport();
builder.AddOpenApiInfrastructure(openApiOptions, appOptions);

builder.Services.AddRequestTimeouts();
builder.Services.AddOutputCache();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.UseBaseInfrastructure();
app.UseCatalogInfrastructure();
app.UseRequestTimeouts();
app.UseFastEndpointsInfrastructure();
app.UseOpenApiInfrastructure(openApiOptions, appOptions);

await app.RunAsync();

// Test
