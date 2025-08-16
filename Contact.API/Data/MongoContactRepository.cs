using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Contact.API.Dtos;
using Contact.API.Models;

namespace Contact.API.Data;

public class MongoContactRepository : IContactRepository
{
    //private readonly IMongoCollection<BaseUserInfo> _contacts;

    private readonly ContactContext _contactContext;

    public MongoContactRepository(ContactContext contactContext)
    {
        _contactContext = contactContext;
        //_contacts = contactContext.GetCollection<BaseUserInfo>("Contacts");
    }

    public async Task<bool> UpdateContactInfoAsync(BaseUserInfo userInfo, CancellationToken cancellationToken = default)
    {
        var contactBook = await _contactContext.ContactBooks
                                                .Find(cb => cb.UserId == userInfo.UserId)
                                                .FirstOrDefaultAsync(cancellationToken);
        if (contactBook == null)
        {
            throw new Exception($"Wrong user id for update contact user info userid: {userInfo.UserId}."); // 联系人信息不存在
        }

        var contactIds = contactBook.Contacts.Select(c => c.UserId);
        var filter = Builders<ContactBook>.Filter.And
        (
            Builders<ContactBook>.Filter.In(c => c.UserId, contactIds),
            Builders<ContactBook>.Filter.ElemMatch(c => c.Contacts, c => c.UserId == userInfo.UserId)
        );

        var update = Builders<ContactBook>.Update
            .Set("Contacts.$.Name", userInfo.Name)
            .Set("Contacts.$.Avatar", userInfo.Avatar)
            .Set("Contacts.$.Company", userInfo.Company)
            .Set("Contacts.$.Title", userInfo.Title);

        var result = await _contactContext.ContactBooks.UpdateManyAsync(filter, update, null, cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> AddContactAsync(int userId, BaseUserInfo contact, CancellationToken cancellationToken = default)
    {
        if (_contactContext.ContactBooks.CountDocuments(cb => cb.UserId == userId) == 0)
        {
            // 如果联系人书不存在，则创建一个新的
            var newContactBook = new ContactBook
            {
                UserId = userId,
                Contacts = new List<Contact.API.Models.Contact>
                {
                    new Contact.API.Models.Contact
                    {
                        UserId = contact.UserId,
                        Name = contact.Name,
                        Avatar = contact.Avatar,
                        Company = contact.Company,
                        Title = contact.Title
                    }
                }
            };
            await _contactContext.ContactBooks.InsertOneAsync(newContactBook, null, cancellationToken);
            return true;
        }

        var filter = Builders<ContactBook>.Filter.Eq(cb => cb.UserId, userId);
        var update = Builders<ContactBook>.Update.AddToSet(cb => cb.Contacts, new Contact.API.Models.Contact
        {
            UserId = contact.UserId,
            Name = contact.Name,
            Avatar = contact.Avatar,
            Company = contact.Company,
            Title = contact.Title
        });

        //var options = new UpdateOptions { IsUpsert = true };
        var result = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);
        return result.MatchedCount == 1 && (result.ModifiedCount == 1 || result.UpsertedId != null);
    }

    public async Task<List<Contact.API.Models.Contact>> GetContactsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var contactBook = await _contactContext.ContactBooks
                                                .Find(cb => cb.UserId == userId)
                                                .FirstOrDefaultAsync(cancellationToken);
        if (contactBook == null)
        {
            // log
            return null;
        }
        return contactBook.Contacts;
    }

    public async Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ContactBook>.Filter.And
        (
            Builders<ContactBook>.Filter.Eq(cb => cb.UserId, userId),
            Builders<ContactBook>.Filter.ElemMatch(cb => cb.Contacts, c => c.UserId == contactId)
        );

        var update = Builders<ContactBook>.Update.Set("Contacts.$.Tags", tags);

        var result = await _contactContext.ContactBooks.UpdateOneAsync(filter, update);
        return result.MatchedCount == 1 && result.ModifiedCount == 1;
    }
}