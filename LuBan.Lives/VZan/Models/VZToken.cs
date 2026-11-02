/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives.VZan.Models
*文件名： VZToken
*版本号： V1.0.0.0
*唯一标识：7248f795-d744-403a-a21b-91e99ed87759
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/19 17:14:02
*描述：VZToken 类
*
*=================================================
*修改标记
*修改时间：2024/9/19 17:14:02
*修改人： yswenli
*版本号： V1.0.0.0
*描述：VZToken 类
*
*****************************************************************************/
namespace LuBan.Lives.VZan.Models
{
    /// <summary>
/// VZToken 类
/// </summary>
    public class VZToken
    {
        /// <summary>
        /// 调用 VZan 接口使用的访问令牌（接口字段 token）。
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 令牌的过期时间（接口字段 tokenExpires）。
        /// </summary>
        public string tokenExpires { get; set; }
    }
}
