using Contact.API.Data;
using Microsoft.Extensions.Options;
using Contact.API;
using Contact.API.Dtos;

namespace Contact.API.Services;

public class UserService : IUserService
{
    private readonly IContactRepository _contactRepository;
    private readonly IOptions<AppSettings> _appSettings;

    public UserService(IContactRepository contactRepository, IOptions<AppSettings> appSettings)
    {
        _contactRepository = contactRepository;
        _appSettings = appSettings;
    }

    // Implement methods to interact with the contact repository
    public async Task<BaseUserInfo> GetBaseUserInfoAsync(int userId, CancellationToken cancellationToken = default)
    {
        return new BaseUserInfo
        {
            UserId = userId,


        };
    }
}