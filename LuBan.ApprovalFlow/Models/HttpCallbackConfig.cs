/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： HttpCallbackConfig.cs
*版本号： V1.0.0.0
*唯一标识：b1efd0d2-c2ab-41af-addf-574d0fc8ae4d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：HttpCallbackConfig 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：HttpCallbackConfig 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// HTTP回调配置，用于定义流程节点中的HTTP回调请求参数。
/// </summary>
public class HttpCallbackConfig
{
    /// <summary>
    /// 回调请求的URL地址。
    /// </summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>
    /// HTTP请求方法，默认为POST。
    /// </summary>
    public string Method { get; set; } = "POST";
    /// <summary>
    /// URL查询参数字典。
    /// </summary>
    public Dictionary<string, object>? QueryParams { get; set; }
    /// <summary>
    /// 请求体类型：json、formData、raw。
    /// </summary>
    public string BodyType { get; set; } = "json";
    /// <summary>
    /// 请求体JSON数据（当 BodyType=json 时使用）。
    /// </summary>
    public object? BodyJson { get; set; }
    /// <summary>
    /// 表单数据字典（当 BodyType=formData 时使用）。
    /// </summary>
    public Dictionary<string, object>? FormData { get; set; }
    /// <summary>
    /// 原始请求体内容（当 BodyType=raw 时使用）。
    /// </summary>
    public string? BodyRaw { get; set; }
    /// <summary>
    /// 认证配置。
    /// </summary>
    public AuthConfig? Auth { get; set; }
    /// <summary>
    /// 请求头字典。
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
    /// <summary>
    /// 请求超时时间（毫秒），默认30000毫秒。
    /// </summary>
    public int Timeout { get; set; } = 30000;
    /// <summary>
    /// 重试次数，默认0次。
    /// </summary>
    public int RetryCount { get; set; } = 0;
    /// <summary>
    /// 重试间隔时间（毫秒），默认5000毫秒。
    /// </summary>
    public int RetryInterval { get; set; } = 5000;
    /// <summary>
    /// 是否异步执行，默认false。
    /// </summary>
    public bool Async { get; set; } = false;
/// <summary>
/// 触发动作类型列表（用于onApprovalAction事件过滤），空表示所有动作都触发。
/// 可选值：approve（审批通过）、reject（审批拒绝）、return（退回）、transfer（转办）、delegate（委托）、withdraw（撤回）
/// </summary>
public List<string>? TriggerActions { get; set; }
}