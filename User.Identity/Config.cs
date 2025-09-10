using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using IdentityModel;

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
            new ApiResource("contactResource", "Contact Management Service")  // 使用您自定义的名称
            {
                Scopes = {
                    "contact_api",
                    "contact.read",         // 读取联系人权限
                    "contact.write",        // 写入联系人权限
                    "contact.manage",       // 管理联系人权限
                    "contact.admin"         // 管理员权限
                }
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
            new ApiScope("contact_api", "Contact API Scope"),
            new ApiScope("contact.read", "Read contacts"),
            new ApiScope("contact.write", "Write contacts"),
            new ApiScope("contact.manage", "Manage contacts"),
            new ApiScope("contact.admin", "Admin contacts"),
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
                    "user_api",
                    "contact_api",
                    "contact.read",         // 读取权限
                    "contact.write",        // 写入权限
                    "contact.manage",       // 管理权限
                    "contact.admin",        // 管理员权限
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "role", // 直接使用字符串 "role"
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
            new IdentityResources.Profile(),
            new IdentityResource("role", "Your role(s)", new[] { JwtClaimTypes.Role }) // 正确的方式定义角色身份资源
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

