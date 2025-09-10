using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Contact.API.Infrastructure
{
    public static class AuthorizationExtensions
    {
        public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder builder, params string[] scopes)
        {
            return builder.RequireAssertion(context =>
            {
                var scopeClaims = context.User.FindAll(c => c.Type == "scope");
                var userScopes = scopeClaims.SelectMany(c => c.Value.Split(' ')).ToList();

                return userScopes.Any(s => scopes.Contains(s));
            });
        }

        // 修改：使用字符串常量代替 JwtClaimTypes
        public static bool IsInRole(this ClaimsPrincipal user, params string[] roles)
        {
            return user.Claims
                .Where(c => c.Type == "role" || c.Type == ClaimTypes.Role)
                .Any(c => roles.Contains(c.Value));
        }

        public static bool HasScope(this ClaimsPrincipal user, params string[] scopes)
        {
            var scopeClaims = user.FindAll(c => c.Type == "scope");
            var userScopes = scopeClaims.SelectMany(c => c.Value.Split(' ')).ToList();
            return userScopes.Any(s => scopes.Contains(s));
        }
    }
}