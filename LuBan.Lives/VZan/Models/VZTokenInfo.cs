/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives.VZan.Models
*文件名： VZTokenInfo
*版本号： V1.0.0.0
*唯一标识：f6a11384-b86c-4a48-bd7d-717836f51ca9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/19 17:08:53
*描述：VZTokenInfo 模型类
*
*=================================================
*修改标记
*修改时间：2024/9/19 17:08:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：VZTokenInfo 模型类
*
*****************************************************************************/
namespace LuBan.Lives.VZan.Models
{
    /// <summary>
/// VZTokenInfo 模型类
/// </summary>
    public class VZTokenInfo
    {
        /// <summary>
        /// 本地缓存中的 VZan 访问令牌字符串。
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 该令牌的获取时间，用于判断是否需要重新申请。
        /// </summary>
        public DateTime Date { get; set; }
    }
}
