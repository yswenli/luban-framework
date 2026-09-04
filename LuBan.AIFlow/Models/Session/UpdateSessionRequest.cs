/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Session
*文件名： UpdateSessionRequest.cs
*版本号： V1.0.0.0
*唯一标识：9e8c572f-0693-4a41-9515-6fad92816151
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：UpdateSessionRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：UpdateSessionRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 更新会话请求模型
/// </summary>
public class UpdateSessionRequest
{
    /// <summary>
    /// 会话标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}