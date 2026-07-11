/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Models
*文件名： VariableType
*版本号： V1.0.0.0
*唯一标识：29cffc2b-fe8d-4175-8532-4a17d45a19d6
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/16 10:38:59
*描述：变量类型
*
*=================================================
*修改标记
*修改时间：2025/9/16 10:38:59
*修改人： yswenli
*版本号： V1.0.0.0
*描述：变量类型
*
*****************************************************************************/
namespace LuBan.Web.Core.Models;

/// <summary>
/// 变量类型
/// </summary>
public enum EnumVariableType
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,
    /// <summary>
    /// 上下文用户
    /// </summary>
    [Description("上下文用户")]
    ContextUser = 1,
    /// <summary>
    /// 查询字符串
    /// </summary>
    [Description("查询字符串")]
    Query = 2,
    /// <summary>
    /// 表单数据
    /// </summary>
    [Description("表单数据")]
    Form = 3,
    /// <summary>
    /// 请求头
    /// </summary>
    [Description("请求头")]
    Header = 4,
    /// <summary>
    /// 模型
    /// </summary>
    [Description("模型")]
    Route = 5,
    /// <summary>
    /// Cookie
    /// </summary>
    [Description("Cookie")]
    Cookie = 6
}
