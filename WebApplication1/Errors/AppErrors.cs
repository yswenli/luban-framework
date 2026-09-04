/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Errors
*文件名： AppErrors.cs
*版本号： V1.0.0.0
*唯一标识：6f351637-84ef-43dd-bc32-e4dd0e643c29
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/17 13:56:05
*描述：AppErrors 类
*
*=================================================
*修改标记
*修改时间：2026/8/17 13:56:05
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AppErrors 类
*
*****************************************************************************/

namespace WebApplication1.Errors;

using LuBan.Common.Errors;

/// <summary>
/// 业务项目自定义错误码定义（90001-90003）。
/// 通过 services.AddErrorCodes(AppErrors.All) 注册到依赖注入。
/// </summary>
public static class AppErrors
{
    /// <summary>已存在同名或同编码项目</summary>
    public static readonly ErrorDescriptor ProjectDuplicate = new(90001, "已存在同名或同编码项目", ErrorCategory.Conflict);

    /// <summary>已存在相同证件号码人员</summary>
    public static readonly ErrorDescriptor IdNumberDuplicate = new(90002, "已存在相同证件号码人员", ErrorCategory.Conflict);

    /// <summary>检测数据不存在</summary>
    public static readonly ErrorDescriptor TestDataNotFound = new(90003, "检测数据不存在", ErrorCategory.NotFound);

    /// <summary>
    /// 所有业务错误码集合（用于注册到 ErrorCodeRegistry）
    /// </summary>
    public static IReadOnlyList<ErrorDescriptor> All => new[]
    {
        ProjectDuplicate, IdNumberDuplicate, TestDataNotFound
    };
}
