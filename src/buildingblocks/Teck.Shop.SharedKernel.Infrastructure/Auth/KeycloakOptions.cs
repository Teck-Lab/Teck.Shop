using Keycloak.AuthServices.Authentication;
using Teck.Shop.SharedKernel.Core.Options;

namespace Teck.Shop.SharedKernel.Infrastructure.Auth
{
    /// <summary>
    /// The keycloak options.
    /// </summary>
    public class KeycloakOptions : KeycloakAuthenticationOptions, IOptionsRoot
    { }
}
