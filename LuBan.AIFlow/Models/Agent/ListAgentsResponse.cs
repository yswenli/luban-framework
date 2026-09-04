/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Agent
*文件名： ListAgentsResponse.cs
*版本号： V1.0.0.0
*唯一标识：e681adbe-73e6-4b1f-aff7-832e471603d4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ListAgentsResponse 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ListAgentsResponse 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Agent;

/// <summary>
/// Agent 列表响应模型
/// </summary>
public class ListAgentsResponse
{
    /// <summary>
    /// 响应代码，0 表示成功
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Agent 列表数据
    /// </summary>
    [JsonPropertyName("data")]
    public List<AgentInfo> Data { get; set; } = new();
}