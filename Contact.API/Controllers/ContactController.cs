using Microsoft.AspNetCore.Mvc;
using Contact.API.Models;
using Contact.API.Data;
using Contact.API.Services;

namespace Contact.API.Controllers;

[ApiController]
[Route("api/contacts")]
public class ContactController : BaseController
{

    private readonly IContactApplyRequestRepository _contactApplyRequestRepository;
    private readonly IContactRepository _contactRepository;

    private readonly IUserService _userService;
    public ContactController(IContactApplyRequestRepository contactApplyRequestRepository,
                                IUserService userService,
                                IContactRepository contactRepository)
    {
        Console.WriteLine($"[{DateTime.Now}] ContactController 构造函数执行");
        _contactApplyRequestRepository = contactApplyRequestRepository;
        _userService = userService;
        _contactRepository = contactRepository;
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        return Ok(await _contactRepository.GetContactsAsync(UserIdentity.UserId, cancellationToken));
    }

    [HttpPut]
    [Route("tags")]
    public async Task<IActionResult> TagContact([FromBody] TagContactInputViewModel viewModel, CancellationToken cancellationToken)
    {
        var result = await _contactRepository.TagContactAsync(UserIdentity.UserId, viewModel.ContactId, viewModel.Tags, cancellationToken);
        if (result)
        {
            return Ok();
        }
        return StatusCode(500, "更新好友标签失败");
    }

    /// <summary>
    /// 获取好友申请列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("apply-requests")]
    public async Task<IActionResult> GetApplyRequests(CancellationToken cancellationToken = default)
    {
        var requests = await _contactApplyRequestRepository.GetRequestListAsync(UserIdentity.UserId, cancellationToken);
        return Ok(requests);
    }

    /// <summary>
    /// 添加好友请求
    /// </summary>
    /// <returns></returns>

    [HttpPost]
    [Route("apply-requests")]
    public async Task<IActionResult> AddApplyRequest(int userId, CancellationToken cancellationToken = default)
    {
        var userBaseInfo = await _userService.GetBaseUserInfoAsync(userId, cancellationToken);
        if (userBaseInfo == null)
        {
            throw new Exception("获取用户信息失败");
        }

        var request = new ContactApplyRequest
        {
            UserId = userId,  //被申请那个人的Id ,传过来那个人的Id，这块我一直没理解上去，就是页面上我当前人 向另一个人申请好友，那另一个人就是 被申请的人
            ApplierId = UserIdentity.UserId,   //当前登录用户的Id，也就是申请人的Id
            Name = userBaseInfo.Name,
            Company = userBaseInfo.Company,
            ApplyTime = DateTime.Now,
            Title = userBaseInfo.Title,
            Avatar = userBaseInfo.Avatar,
        };

        var result = await _contactApplyRequestRepository.AddRequestAsync(request, cancellationToken);
        if (result)
        {
            return Ok();
        }
        else
        {
            //log
            return StatusCode(500, "添加好友请求失败");
        }
    }

    /// <summary>
    /// 通过好友请求
    /// 此时我作为 被申请人，接受了申请人的好友请求
    /// 也就是我当前登录的用户，接受了另一个用户的好友请求
    /// 审批通过之后，需要互相加上联系人
    /// </summary>
    /// <returns></returns>

    [HttpPut]
    [Route("apply-requests")]
    public async Task<IActionResult> ApprovalRequestAsync(int applierId, CancellationToken cancellationToken = default)
    {
        var result = await _contactApplyRequestRepository.ApprovalAsync(UserIdentity.UserId, applierId, cancellationToken); //就是谁申请人的id


        if (!result)
        {
            return StatusCode(500, "通过好友请求失败");
        }
        var applier = await _userService.GetBaseUserInfoAsync(applierId, cancellationToken);
        var userinfo = await _userService.GetBaseUserInfoAsync(UserIdentity.UserId, cancellationToken);

        _contactRepository.AddContactAsync(UserIdentity.UserId, userinfo, cancellationToken);
        _contactRepository.AddContactAsync(applierId, applier, cancellationToken);

        return Ok();
    }
}
