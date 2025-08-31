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
        Console.WriteLine($"[{DateTime.Now}] MongoContactRepository 构造函数执行");
        _contactContext = contactContext;
        //_contacts = contactContext.GetCollection<BaseUserInfo>("Contacts");
    }

    /// <summary>
    /// 
    /// 
    /// 
    /// 业务逻辑理解
    /// 这实际上是一个"联系人信息同步"功能：

    /// 当用户更新自己的资料信息（如姓名、头像等）

    /// 系统会自动更新所有其他用户通讯录中关于该用户的记录

    /// 确保所有将该用户存入通讯录的人都能看到最新信息
    /// 
    /// 
    /// 查找源通讯录：首先找到要更新的用户（userInfo.UserId）自己的通讯录

    /// 批量更新引用：然后在该用户所有联系人的通讯录中更新该用户的信息
    /// 
    /// 1. var contactBook = await _contactContext.ContactBooks.Find(...)

    /// 查找用户自己的通讯录（通过UserId匹配）

    /// 这是验证用户存在的"源"通讯录

    /// 2. var contactIds = contactBook.Contacts.Select(c => c.UserId)

    /// 获取该用户通讯录中所有联系人的UserId

    /// 这些就是所有在通讯录中包含该用户的联系人

    /// 3. 筛选条件（filter）做了两件事：


    /// Filter.In：找出属于该用户联系人的所有通讯录

    /// ElemMatch：在这些通讯录中定位到具体该用户的联系人条目

    ///Contacts.$ 表示更新在 ElemMatch 中匹配到的那个特定联系人条目
    /// 
    /// 那就是一步一步实现 数据范围的缩小 去更新的数据呗，这个跟新很有趣呀，sqlserver的使用中不曾用到过
    /// </summary>
    /// <param name="userInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<bool> UpdateContactInfoAsync(UserIdentity userInfo, CancellationToken cancellationToken = default)
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

    public async Task<bool> AddContactAsync(int userId, UserIdentity contact, CancellationToken cancellationToken = default)
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
            return new List<Contact.API.Models.Contact>();
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

        var result = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);
        return result.MatchedCount == 1 && result.ModifiedCount == 1;
    }
}