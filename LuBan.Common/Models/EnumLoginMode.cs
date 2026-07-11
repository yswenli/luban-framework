/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Models
*文件名： LoginModeEnum
*版本号： V1.0.0.0
*唯一标识：55839dbd-bb40-42e9-a13b-522d764b8f2f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 16:33:25
*描述：登录模式枚举
*
*=================================================
*修改标记
*修改时间：2023/12/5 16:33:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：登录模式枚举
*
*****************************************************************************/
namespace LuBan.Common.Models
{

    /// <summary>
    /// 登录模式枚举
    /// </summary>
    [Description("登录模式枚举")]
    public enum EnumLoginMode
    {
        /// <summary>
        /// PC模式
        /// </summary>
        [Description("PC模式")]
        PC = 1,

        /// <summary>
        /// APP
        /// </summary>
        [Description("APP")]
        APP = 2
    }
}
