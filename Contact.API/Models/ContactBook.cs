namespace Contact.API.Models;

public class ContactBook
{
    public int UserId { get; set; }
    /// <summary>
    /// 联系人列表
    /// </summary>
    public List<Contact> Contacts { get; set; } = new List<Contact>();

}