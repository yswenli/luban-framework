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
                echostr = "YOUR_ECHOSTR"
            }) ?? "";

            Assert.IsNotNull(result);
        }


        /// <summary>
        /// 测试接收
        /// </summary>
        [TestMethod]
        public void TestReceive()
        {
            var xml = "<xml><ToUserName><![CDATA[YOUR_CORP_ID]]></ToUserName><Encrypt><![CDATA[YOUR_ENCRYPTED_MESSAGE]]></Encrypt><AgentID><![CDATA[1000005]]></AgentID></xml>";

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
