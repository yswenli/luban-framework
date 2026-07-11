/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives.YiBai
*文件名： YBLiveClient
*版本号： V1.0.0.0
*唯一标识：8205dade-4385-4199-b97b-d476fb2b345c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/18 18:12:51
*描述：100直播
*
*=================================================
*修改标记
*修改时间：2024/9/18 18:12:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：100直播
*
*****************************************************************************/
using LuBan.Lives.YiBai.Models;

namespace LuBan.Lives.YiBai
{
    /// <summary>
    /// 100直播
    /// </summary>
    public class YBLiveClient : BaseLiveClient, ILiveClient
    {
        /// <summary>
        /// 会议客户端
        /// </summary>
        /// <param name="liveConfig"></param>
        public YBLiveClient(LiveOption? liveConfig) : base(liveConfig)
        {

        }

        /// <summary>
        /// 会议客户端
        /// </summary>
        public YBLiveClient() : this(NacosConfigUtil.Read<LiveOption>("LiveOption"))
        {

        }

        protected override Dictionary<string, string> GetBaseHeaders()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取前台登录Token
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        string GetToken(string phone)
        {
            try
            {
                var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
                var timeStamp = Convert.ToInt64(ts.TotalMilliseconds);
                var str = string.Format("phone={0}&postTime={1}&key={2}", phone, timeStamp, _liveOption.AppSecret);
                var signature = str.GetMD5Str().ToLower();

                var json = @"  {
                " + "\n" +
                               @"    ""phone"":""" + phone + @""",
                " + "\n" +
                               @"    ""businessId"":" + _liveOption.AppId + @",
                " + "\n" +
                               @"    ""sign"":""" + signature + @""",
                " + "\n" +
                               @"    ""postTime"":""" + timeStamp + @"""
                " + "\n" +
                   @"    }";

                var headers = new Dictionary<string, string>();
                headers.Add("remote-host", "sinqi.100doc.com.cn");

                var data = _httpClient.Post("/djb/yb-user-api-ntk/user/third/login", json, headers);

                return SerializeUtil.Deserialize<YBDatainfo>(data)?.data ?? "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        /// <summary>
        /// 获取直播间链接
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="secret"></param>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="avatar"></param>
        /// <returns></returns>
        public string GetLiveUrl(string channelId, string secret, string userId, string name, string avatar)
        {
            var token = GetToken(userId);
            var url = string.Format("{0}/#/details/{1}/{2}?token={3}", _liveOption.Url, secret, channelId, token);
            return url;
        }


        /// <summary>
        /// 获取后台登录Token
        /// </summary>
        /// <returns></returns>
        string GetApiToken()
        {
            try
            {
                var json = @"{
                " + "\n" +
                                @"    ""username"":""" + _liveOption.UserName + @""",
                " + "\n" +
                                @"    ""password"":""" + _liveOption.Password + @""",
                " + "\n" +
                                @"    ""passwordSha"":""" + _liveOption.Salt + @"""}";

                var headers = new Dictionary<string, string>();
                headers.Add("remote-host", "sinqi.100doc.com.cn");

                var data = _httpClient.Post("/yb-company/company/user/login", json, headers);

                return SerializeUtil.Deserialize<YBDatainfo>(data)?.data ?? "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
