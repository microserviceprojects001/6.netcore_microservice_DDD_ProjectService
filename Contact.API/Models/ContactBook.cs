using MongoDB.Bson.Serialization.Attributes;

namespace Contact.API.Models;

/// <summary>
/// DB 的_id没有定义对应的属性，会报错，素有加这个标签
/// </summary>
[BsonIgnoreExtraElements]

public class ContactBook
{
    public ContactBook()
    {
        Contacts = new List<Contact>();
    }
    public int UserId { get; set; }
    /// <summary>
    /// 联系人列表
    /// </summary>
    public List<Contact> Contacts { get; set; }

}