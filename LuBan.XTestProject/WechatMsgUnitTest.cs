/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.XTestProject
*文件名： WechatMsgUnitTest
*版本号： V1.0.0.0
*唯一标识：34954c90-66c8-48c3-bcec-2d1788557f3e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/14 13:20:23
*描述：微信消息单元测试
*
*=================================================
*修改标记
*修改时间：2024/11/14 13:20:23
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信消息单元测试
*
*****************************************************************************/
using LuBan.Wechat.Utils;

using System.Xml;

namespace LuBan.XTestProject
{
    /// <summary>
    /// 微信消息单元测试
    /// </summary>
    [TestClass]
    public class WechatMsgUnitTest
    {
        [TestMethod]
        public void Test()
        {
            try
            {
                var xml = "<xml><ToUserName><![CDATA[YOUR_TO_USER_NAME]]></ToUserName><Encrypt><![CDATA[YOUR_ENCRYPTED_MESSAGE]]></Encrypt><AgentID><![CDATA[]]></AgentID></xml>";

                TryParseXml(xml, out var encryptedMsg, out var toUserName, out var agentId);

                var cpid = "YOUR_CORP_ID";
                var key = "YOUR_ENCRYPTION_KEY";

                var text = CryptographyUtil.AES_decrypt(encryptedMsg, key, ref cpid);

                Assert.IsNotNull(text);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }


        public static bool TryParseXml(string xml, out string encryptedMsg, out string toUserName, out string agentId)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            encryptedMsg = string.Empty;
            toUserName = string.Empty;
            agentId = string.Empty;
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.XmlResolver = null;
                xmlDocument.LoadXml(xml);
                XmlNode firstChild = xmlDocument.FirstChild;
                if (firstChild == null)
                {
                    return false;
                }

                encryptedMsg = firstChild["Encrypt"]?.InnerText?.ToString() ?? string.Empty;
                toUserName = firstChild["ToUserName"]?.InnerText?.ToString() ?? string.Empty;
                agentId = firstChild["AgentID"]?.InnerText?.ToString() ?? string.Empty;
                return !string.IsNullOrEmpty(encryptedMsg);
            }
            catch (XmlException)
            {
                return false;
            }
        }
    }
}
