using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using IdentityModel;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using System.Linq; // 添加这个 using 指令

namespace User.Identity.Authentication;

public class ProfileService : IProfileService
{
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var subject = context.Subject ?? throw new System.Exception("No subject");
        var subjectId = subject.GetSubjectId();
        if (string.IsNullOrEmpty(subjectId))
        {
            throw new System.Exception("No subject ID");
        }

        context.IssuedClaims = context.Subject.Claims.ToList();
        await Task.CompletedTask;
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = false;
        var sub = context.Subject ?? throw new System.Exception("No subject");
        var subjectId = sub.GetSubjectId();
        if (!string.IsNullOrEmpty(subjectId))
        {
            context.IsActive = true;
        }
        await Task.CompletedTask;
    }

}