/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow
*文件名： IAIClient.cs
*版本号： V1.0.0.0
*唯一标识：7ad77eab-3602-44ab-b9a5-c12713bd1dd3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：IAIClient 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：IAIClient 类
*
*****************************************************************************/

namespace LuBan.AIFlow;

/// <summary>
/// AI客户端接口
/// </summary>
public interface IAIClient
{
    /// <summary>
    /// AI客户端选项
    /// </summary>
    AIOptions Options { get; }
}
