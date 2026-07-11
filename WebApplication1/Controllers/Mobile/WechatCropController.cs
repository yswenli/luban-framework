
using LuBan.Wechat.Models;

using WebApplication1.Services.ApiServices;

namespace WebApplication1.Controllers.Mobile
{
    /// <summary>
    /// 企业微信消息处理
    /// </summary>
    [AllowAnonymous]
    public class WechatCorpController : BaseMobileController
    {

        /// <summary>
        /// 测试接收企业微信消息处理
        /// </summary>
        /// <param name="input">验证接收企业微信消息处理参数</param>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("/api/[controller]/[action]")]
        [DisplayName("测试接收企业微信消息处理")]
        public async Task<IActionResult> Receive([Required, FromQuery] TestWorkReceiveInput input)
        {
            return await ContentAsync(SysWxCropService.Instance.Receive(input));
        }

        /// <summary>
        /// 接收企业微信消息处理
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous, HttpPost("/api/[controller]/[action]")]
        [DisplayName("接收企业微信消息处理")]
        public async Task<IActionResult> Receive([FromQuery] BaseWorkReceiveInput input)
        {
            return await ContentAsync(await SysWxCropService.Instance.Receive(input));
        }
    }
}
