/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Mobile
*文件名： WechatCropController.cs
*版本号： V1.0.0.0
*唯一标识：51deb06b-05dc-43d8-8d2d-1a0441e3b51e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：WechatCropController 控制器
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：WechatCropController 控制器
*
*****************************************************************************/


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
