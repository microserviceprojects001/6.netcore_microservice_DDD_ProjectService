using Contact.API.Data;
using Microservice.Contracts.Events;
using DotNetCore.CAP;
using System.Threading.Tasks;
using Contact.API.Dtos;

namespace Contact.API.IntegrationEvents.EventHandling
{
    public class UserProfileChangedEventHandler : ICapSubscribe
    {
        private readonly IContactRepository _contactRepository;

        public UserProfileChangedEventHandler(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        // debug代码是，与正常跟代码的方式是一样的，contact.API run debug 起来，user.API dotnetrun起来，
        // post man调用user.API的更新用户接口
        [CapSubscribe("user.profile.changed")]
        public async Task Handle(UserProfileChangedEvent @event)
        {
            var userInfo = new UserIdentity
            {
                UserId = @event.UserId,
                Name = @event.Name,
                Company = @event.Company,
                Title = @event.Title,
                Avatar = @event.Avatar
            };

            await _contactRepository.UpdateContactInfoAsync(userInfo);
        }
    }
}
