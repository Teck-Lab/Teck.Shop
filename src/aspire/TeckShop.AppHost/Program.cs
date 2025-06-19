using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("redis");

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume(isReadOnly: false);
var catalogdb = postgres.AddDatabase("catalogdb");

// Fix for CS1061: 'ParameterDefault' does not contain a definition for 'Value'.
// The issue arises because the 'ParameterDefault' class does not have a 'Value' property.
// Instead, we need to use the 'GetDefaultValue()' method provided by 'ParameterDefault' to retrieve the default value.

var rabbitmqUserName = builder.CreateResourceBuilder(new ParameterResource(
    "guest",
    defaultValue => defaultValue?.GetDefaultValue() ?? "guest", // Use GetDefaultValue() instead of Value
    false
));
var rabbitmqPassword = builder.CreateResourceBuilder(new ParameterResource(
    "guest",
    defaultValue => defaultValue?.GetDefaultValue() ?? "guest", // Use GetDefaultValue() instead of Value
    false
));

var rabbitmq = builder.AddRabbitMQ("rabbitmq", rabbitmqUserName, rabbitmqPassword).WithManagementPlugin();

var keycloak = builder.AddKeycloakContainer("keycloak", "26.2.5")
    .WithDataVolume("local");

var realm = keycloak.AddRealm("TeckShop");

var catalogMigrationService = builder.AddProject<Projects.Catalog_MigrationService>("catalog-migrationservice")
    .WithReference(catalogdb)
    .WaitFor(catalogdb);

var catalogapi = builder.AddProject<Projects.Catalog_Api>("catalog-api")
    .WithReference(cache)
    .WithReference(catalogdb)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WithReference(realm)
    .WaitForCompletion(catalogMigrationService)
    .WaitFor(cache)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.Yarp_Gateway_Public>("yarp-gateway-public")
    .WaitFor(catalogapi)
    .WithReference(catalogapi);

await builder.Build().RunAsync();
