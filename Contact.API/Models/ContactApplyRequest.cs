using MongoDB.Bson.Serialization.Attributes;
using System;
namespace Contact.API.Models;

/// <summary>
/// 用户发起请求申请好友过来，用户UI看到一个列表 同意或者拒绝，同意之后会把Requestor信息作为contact保存到 ContactBook中
/// </summary>
[BsonIgnoreExtraElements]
public class ContactApplyRequest
{
    /// <summary>
    /// 被申请的用户Id
    /// </summary>
    public int UserId { get; set; }

    /// 以下都是申请人的信息
    /// <summary>
    /// 用户名称
    /// </summary>

    public string Name { get; set; }

    /// <summary>
    /// 公司
    /// </summary>
    public string Company { get; set; }

    /// <summary>
    /// 工作职位
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 用户头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 申请人
    /// </summary>
    public int ApplierId { get; set; }

    /// <summary>
    /// 是否通过， 0 未通过，  1 已通过
    /// </summary>
    public int Approvaled { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    public DateTime HandledTime { get; set; }

    public DateTime ApplyTime { get; set; }
}