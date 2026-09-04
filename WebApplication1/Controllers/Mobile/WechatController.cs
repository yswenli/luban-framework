/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Mobile
*文件名： WechatController.cs
*版本号： V1.0.0.0
*唯一标识：456ff39b-fb5c-48f2-ac06-1e34a8f74a06
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：WechatController 控制器
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：WechatController 控制器
*
*****************************************************************************/


using LuBan.Wechat;
using LuBan.Wechat.Models;

using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace WebApplication1.Controllers.Mobile
{

    public class WechatController : BaseMobileController
    {
        /// <summary>
        /// 微信回调接收消息接口GET
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpGet("/api/[controller]/[action]")]
        [DisplayName("接收微信服务号消息处理GET")]
        public async Task<IActionResult> Receive([FromQuery, Required] ReceiveInput input)
        {
            Logger.Debug("syswechat.Receive", input);
            return await ContentAsync(input.echostr);
        }

        /// <summary>
        /// 微信回调接收消息接口POST
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous, HttpPost("/api/[controller]/[action]")]
        [DisplayName("接收微信服务号消息处理POST")]
        public async Task<IActionResult> Receive()
        {
            return await ContentAsync("Hello World");
        }

        /// <summary>
        /// 测试微信接口
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpGet]
        [DisplayName("测试微信接口")]
        public async Task<Result> GetAccessToken(string code)
        {
            var wechatApiClient = WechatClientFactory.Create(EnumWechatType.Api) as WechatApiClient; ;
            var reqOAuth2 = new SnsOAuth2AccessTokenRequest()
            {
                Code = code,
            };
            var resOAuth2 = await wechatApiClient!.ExecuteSnsOAuth2AccessTokenAsync(reqOAuth2);
            return SuccessResult(resOAuth2);
        }
    }
}
