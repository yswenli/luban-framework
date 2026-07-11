/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Controls
*文件名： HtmlSelectOption
*版本号： V1.0.0.0
*唯一标识：9bdbfe89-c072-4999-812d-3216f7a57c30
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/10/25 14:34:35
*描述：html的select控件
*
*=================================================
*修改标记
*修改时间：2022/10/25 14:34:35
*修改人： yswenli
*版本号： V1.0.0.0
*描述：html的select控件
*
*****************************************************************************/

namespace LuBan.Web.Core.Controls
{
    /// <summary>
    /// html的select控件
    /// </summary>
    public class HtmlSelectOption
    {
        /// <summary>
        /// 文本
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public dynamic? Value { get; set; }

        /// <summary>
        /// 选项模型项
        /// </summary>
        public HtmlSelectOption()
        {

        }
        /// <summary>
        /// 选项模型项
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        public HtmlSelectOption(string text, dynamic value)
        {
            Text = text;
            Value = value;
        }
    }
}
