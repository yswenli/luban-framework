/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AI
*文件名： AIOptions
*版本号： V1.0.0.0
*唯一标识：8daaeb1d-946a-4f95-94b9-24002252d3a1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/6/10 11:12:05
*描述：AI 客户端选项
*
*=================================================
*修改标记
*修改时间：2025/6/10 11:12:05
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AI 客户端选项
*
*****************************************************************************/
namespace LuBan.AIFlow;


/// <summary>
/// AI 客户端选项
/// </summary>
public class AIOptions
{
    /// <summary>
    /// ai客户端类型
    /// </summary>
    public EnumAIType AIType { get; set; }
    /// <summary>
    /// AI 服务的基础 URL
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// 用于身份验证的 API 密钥
    /// </summary>
    public string? ApiKey { get; set; }
}
