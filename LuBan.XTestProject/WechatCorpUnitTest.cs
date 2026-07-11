/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.TestProject1
*文件名： WechatCorpUnitTest
*版本号： V1.0.0.0
*唯一标识：edae907c-e392-45e8-b000-932932cf2901
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/2 11:54:19
*描述：企业微信单元测试
*
*=================================================
*修改标记
*修改时间：2024/8/2 11:54:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：企业微信单元测试
*
*****************************************************************************/
using LuBan.Wechat;
using LuBan.Wechat.Models;

using SKIT.FlurlHttpClient.Wechat.Work.Events;

namespace LuBan.TestProject1
{

    /// <summary>
    /// 企业微信单元测试
    /// </summary>
    [TestClass]
    public class WechatCorpUnitTest
    {
        WechatCorpClient _wechatWorkClient;
        WechatCorpCallBackCall _WechatCorpCallBackCall;

        /// <summary>
        /// 初始化
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            _wechatWorkClient = (WechatCorpClient)WechatClientFactory.Create(EnumWechatType.Corp);
            _WechatCorpCallBackCall = new WechatCorpCallBackCall(_wechatWorkClient);
        }

        /// <summary>
        /// 企业微信单元测试
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            var result = _WechatCorpCallBackCall.AccessValid(new TestWorkReceiveInput()
            {
                msg_signature = "3a149508b3d286594704c539cf7488d43554be6b",
                timestamp = "1731053744",
                nonce = "5w3neo8n12",
                echostr = "ZU6ph8INCDylcH1NVw5AEX5yx6WFaceBh+7jTFQjpvKzDhSEoZDTyZ9VRrnjhzfiMUaucGyd5uNTSs8JlcOohA=="
            }) ?? "";

            Assert.IsNotNull(result);
        }


        /// <summary>
        /// 测试接收
        /// </summary>
        [TestMethod]
        public void TestReceive()
        {
            var xml = "<xml><ToUserName><![CDATA[wwa44b7b4a6582f4b0]]></ToUserName><Encrypt><![CDATA[ug7q5HfC0+fAY8vaOHuyWkI8QRgXFInj7tIvvhFHpIH62PVIRiHNTi6NQqCGpklCV3DafH+lO/BltVr9edTBJ3+dHV792bplcMznXZJbWFkZBezZdFuMG6O9YRIhKNqatfeF8joBzqliapCfeuaLh8Mvs5n8WyChTIdQWMbp2y1dVHJ5+CJhqXDR9+QYQH7qPpc7jrXha1+gke6nUdQubl7u7XuCrMagV87F0ErivVZ86Wdl3KauR3OvA+6hW+THJ3iHRJJ1/1KAfxHshjlluziR6fpRumiziDKTTG8g3bJBHxEKgSw5uSJoYxrbhRA3MDbRROaQXevJuY+PFlPYkkIO6QnQOywmm2YTOnx6+RfAwokAPeH2vikYE5176aXJvrvppzoI2T/TBTJIOifqHrGTylpfxY9z+4KHu+IHCbyPpi0N1BdNucntXKUSG59PJI7+ZqmyemWFvfhJRqZNEQ==]]></Encrypt><AgentID><![CDATA[1000005]]></AgentID></xml>";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            try
            {
                var result = _WechatCorpCallBackCall.Receive<TextMessageEvent, TextMessageReply>(new BaseWorkReceiveInput(), stream, isJson: false,
                    (client, msg, ev) =>
                    {
                        var userId = ev.FromUserName;

                        //todo:处理接收内容业务
                        //OnReceiveKF(ev);

                    },
                    (client, ev) =>
                    {
                        //todo:处理回复业务

                        var replay = new TextMessageReply()
                        {
                            ToUserName = ev.FromUserName,
                            FromUserName = ev.ToUserName,
                            Content = "Hello",
                            MessageType = "text",
                            CreateTimestamp = ev.CreateTimestamp,
                            Event = ev.Event,
                            InfoTimestamp = ev.InfoTimestamp,
                            InfoType = ev.InfoType
                        };

                        return replay;
                    });

                Assert.IsNotNull(result);
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }
    }
}
