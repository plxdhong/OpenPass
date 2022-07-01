using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;
using System.Collections.Generic;

public class Config
{
    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
    {
        new ApiScope(name: "api_cfew",  displayName: "api cfew manage" ),
        new ApiScope
           {
               Enabled = true,
               Name  = "role",
               DisplayName = "Role(s)",
               Description = "roles of user",
               UserClaims = {
                        JwtClaimTypes.Role // JwtClaimTypes.Role ClaimTypes.Role
                        
               }
           }
    };
    }
    public static IEnumerable<Client> GetClients()
    {
        return new List<Client>
        {
            new Client
            {
                ClientId = "cfewClient",
                ClientName = "cfew Client",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                AllowAccessTokensViaBrowser = true,

                RedirectUris =           { "https://cfew.rainballs.com/" },
                PostLogoutRedirectUris = { "https://cfew.rainballs.com/" },
                AllowedCorsOrigins =     { "https://cfew.rainballs.com" },
                AllowOfflineAccess=true,

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "api_cfew",
                    "role"
                }
            }
        };
    }
    public static IEnumerable<IdentityResource> IdentityResources()
    {
        return new List<IdentityResource>
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
    };
    }
}
