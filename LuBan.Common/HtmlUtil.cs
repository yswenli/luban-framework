/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： HtmlUtil
*版本号： V1.0.0.0
*唯一标识：13dcfc04-b708-4fe1-979d-bce72807d139
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/1/7 10:54:10
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/1/7 10:54:10
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/


using UAParser;

namespace LuBan.Common
{
    /// <summary>
    /// html工具类
    /// </summary>
    public static class HtmlUtil
    {
        /// <summary>
        /// 清理html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetTextFromHtml(this string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var txt = doc.DocumentNode.InnerText;
            if (string.IsNullOrEmpty(txt)) return txt;
            return HttpUtility.HtmlDecode(txt).Trim();
        }

        /// <summary>
        /// 清理html,
        /// 包括内容中的html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ClearHtml(this string html)
        {
            if (string.IsNullOrEmpty(html)) return html;

            var txt = html.GetTextFromHtml();

            if (string.IsNullOrEmpty(txt)) return txt;

            return txt.Trim().HtmlEncode();
        }

        /// <summary>
        /// 获取属性列表
        /// </summary>
        /// <param name="html"></param>
        /// <param name="htmlPath"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static List<string> GetAttrbutes(string html, string htmlPath, string attributeName = "")
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(htmlPath);
            if (nodes == null || nodes.Count < 1) return [];
            var attrcs = nodes.Select(q => q.Attributes).ToList();
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(attributeName))
            {
                foreach (var attrc in attrcs)
                {
                    if (attrc == null || attrc.Count < 1) continue;
                    foreach (var item in attrc)
                    {
                        result.Add(item.Value);
                    }
                }
            }
            else
            {
                foreach (var attrc in attrcs)
                {
                    if (attrc == null || attrc.Count < 1) continue;
                    foreach (var item in attrc)
                    {
                        if (item.Name == attributeName)
                            result.Add(item.Value);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取全部内容
        /// </summary>
        /// <param name="html"></param>
        /// <param name="htmlPath"></param>
        /// <returns></returns>
        public static List<string> GetInnerTexts(string html, string htmlPath)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(htmlPath);
            if (nodes == null || nodes.Count < 1) return [];
            var innerTexts = nodes.Select(q => q.InnerText).ToList();
            List<string> result = new List<string>();
            foreach (var innerText in innerTexts)
            {
                if (!string.IsNullOrEmpty(innerText)) continue;
                result.Add(innerText);
            }
            return result;
        }

        /// <summary>
        /// 获取指定节点的html 列表
        /// </summary>
        /// <param name="html"></param>
        /// <param name="htmlPath"></param>
        /// <param name="containSelf"></param>
        /// <returns></returns>
        public static List<string> GetNodeHtmls(string html, string htmlPath, bool containSelf = false)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(htmlPath);
            if (nodes == null || nodes.Count < 1) return [];
            List<string> result = new List<string>();
            foreach (var node in nodes)
            {
                result.Add(containSelf ? node.OuterHtml : node.InnerHtml);
            }
            return result;
        }

        /// <summary>
        /// html 加密
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlEncode(this string html)
        {
            return HttpUtility.HtmlEncode(html);
        }

        /// <summary>
        /// html 解密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string HtmlDecode(this string txt)
        {
            return HttpUtility.HtmlDecode(txt);
        }


        static Parser _parser = Parser.GetDefault();

        /// <summary>
        /// 获取设备名称
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static (string, string, string)? GetDeviceInfo(this string userAgent)
        {
            try
            {
                var clientInfo = _parser.Parse(userAgent);
                return ($"{clientInfo.Device.Family} {clientInfo.Device.Brand}", $"{clientInfo.OS.Family} {clientInfo.OS.Major}", $"{clientInfo.Browser.Family} {clientInfo.Browser.Major}");
            }
            catch
            {
                return null;
            }
        }
    }
}
