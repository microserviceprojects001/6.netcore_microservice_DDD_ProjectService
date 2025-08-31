using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
public class Config
{
    // 定义 API 资源
    public static IEnumerable<ApiResource> GetApiResource()
    {
        return new List<ApiResource>
        {
            new ApiResource("gateway_api", "user api service")
            {
                Scopes = { "gateway_api" }  // 关联到 ApiScope
            },
            new ApiResource("contact_api", "contact service")
            {
                Scopes = { "contact_api" }
            },
             new ApiResource("user_api", "user api") {
                Scopes = { "user_api" }
             }
        };
    }

    // 新增：定义 API Scope
    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            new ApiScope("gateway_api", "My API Scope"),  // 必须和 AllowedScopes 中的名称一致
            new ApiScope("contact_api", "my contact scope"),
            new ApiScope("user_api", "user_api scope")
        };
    }

    // 客户端配置

    public static IEnumerable<Client> GetClients()
    {
        return new List<Client>
        {
            new Client
            {
                ClientId = "android",
                ClientSecrets = { new Secret("secret".Sha256()) },
                //RefreshTokenExpires = TokenExpiration.Sliding,
                AllowOfflineAccess = true,
                RequireClientSecret = false,
                AllowedGrantTypes =  new List<string> { "sms_code" }, // 使用自定义的授权类型
                AllowedScopes =
                {
                    "gateway_api",
                    "contact_api",
                    "user_api",
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess, // 允许离线访问
                },
            }

        };
    }

    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };
    }

    public static List<TestUser> GetUsers()
    {
        return new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "1",
                Username = "alice",
                Password = "password"
            },
            new TestUser
            {
                SubjectId = "2",
                Username = "bob",
                Password = "password"
            }
        };
    }
}

