/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Consts
*文件名： ConstAuthType.cs
*版本号： V1.0.0.0
*唯一标识：385ab480-e4ad-417f-931c-08234245b13f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ConstAuthType 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ConstAuthType 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 认证类型常量，用于定义HTTP节点的认证方式
/// </summary>
public class ConstAuthType
{
    /// <summary>
    /// 无认证
    /// </summary>
    public const string None = "none";

    /// <summary>
    /// 基础认证（Basic Authentication）
    /// </summary>
    public const string Basic = "basic";

    /// <summary>
    /// Bearer Token认证
    /// </summary>
    public const string Bearer = "bearer";

    /// <summary>
    /// API密钥认证（通过请求头或查询参数传递 API Key）
    /// </summary>
    /// <remarks>
    /// 常量名与取值一致（"apiKey"）容易造成混淆：此处的 "apiKey" 是认证方式的标识符，
    /// 并非某个具体的密钥值。配置 <c>AuthConfig.Type</c> 时使用本常量。
    /// </remarks>
    public const string ApiKey = "apiKey";
}