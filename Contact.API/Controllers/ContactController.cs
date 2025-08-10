using Microsoft.AspNetCore.Mvc;
using Contact.API.Models;
using Contact.API.Data;
using Contact.API.Services;

namespace Contact.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : BaseController
{

    private readonly IContactApplyRequestRepository _contactApplyRequestRepository;

    private readonly IUserService _userService;
    public ContactController(IContactApplyRequestRepository contactApplyRequestRepository, IUserService userService)
    {
        _contactApplyRequestRepository = contactApplyRequestRepository;
        _userService = userService;
    }

    /// <summary>
    /// 获取好友申请列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("apply-requests")]
    public async Task<IActionResult> GetApplyRequests()
    {
        var requests = await _contactApplyRequestRepository.GetRequestListAsync(UserIdentity.UserId);
        return Ok(requests);
    }

    /// <summary>
    /// 添加好友请求
    /// </summary>
    /// <returns></returns>

    [HttpPost]
    [Route("apply-requests")]
    public async Task<IActionResult> AddApplyRequest(int userId)
    {
        var userBaseInfo = await _userService.GetBaseUserInfoAsync(userId);
        if (userBaseInfo == null)
        {
            throw new Exception("获取用户信息失败");
        }

        var request = new ContactApplyRequest
        {
            UserId = userId,  //被申请那个人的Id ,传过来那个人的Id
            ApplierId = UserIdentity.UserId,   //当前登录用户的Id
            Name = userBaseInfo.Name,
            Company = userBaseInfo.Company,
            CreateTime = DateTime.Now,
            Title = userBaseInfo.Title,
            Avatar = userBaseInfo.Avatar,
        };

        var result = await _contactApplyRequestRepository.AddRequestAsync(request);
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
    /// </summary>
    /// <returns></returns>

    [HttpPut]
    [Route("apply-requests")]
    public async Task<IActionResult> ApprovalRequest(int applierId)
    {
        var result = await _contactApplyRequestRepository.ApprovalAsync(applierId); //就是谁申请人的id
        if (result)
        {
            return Ok();
        }
        else
        {
            //log
            return StatusCode(500, "通过好友请求失败");
        }
    }
}
