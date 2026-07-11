/****************************************************************************
*Copyright (c) 2023 Walle All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumTenantType
*版本号： V1.0.0.0
*唯一标识：27472d26-1daa-43a4-aa85-b2ea480a0683
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:17:01
*描述：租户类型枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:17:01
*修改人： yswenli
*版本号： V1.0.0.0
*描述：租户类型枚举
*
*****************************************************************************/
namespace WebApplication1.Models.Enums
{
    /// <summary>
    /// 栏目状态枚举
    /// </summary>
    [Description("栏目状态枚举")]
    public enum EnumBlockStatus
    {
        /// <summary>
        /// 启用
        /// </summary>
        [Description("启用")]
        Enabled = 1,

        /// <summary>
        /// 禁用
        /// </summary>
        [Description("禁用")]
        Disabled = 2,




    }
}
