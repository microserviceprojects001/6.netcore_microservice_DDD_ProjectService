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
