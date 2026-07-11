/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AI.Models
*文件名： EnumAIType
*版本号： V1.0.0.0
*唯一标识：b5f90226-724a-4c5b-96dd-b667f2d0e083
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/6/10 11:13:08
*描述：ai类型
*
*=================================================
*修改标记
*修改时间：2025/6/10 11:13:08
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ai类型
*
*****************************************************************************/
namespace LuBan.AIFlow.Models;

/// <summary>
/// ai类型
/// </summary>
[Description("ai类型")]
public enum EnumAIType
{
    /// <summary>
    /// OpenAI
    /// </summary>
    [Description("OpenAI")]
    OpenAI = 0,
    /// <summary>
    /// Azure OpenAI
    /// </summary>
    [Description("AzureOpenAI")]
    AzureOpenAI = 1,
    /// <summary>
    /// AliyunBaiLian
    /// </summary>
    [Description("AliyunBaiLian")]
    AliyunBaiLian = 2,
    /// <summary>
    /// Baidu
    /// </summary>
    [Description("Baidu")]
    Baidu = 3,
    /// <summary>
    /// TengXun
    /// </summary>
    [Description("TengXun")]
    TengXun = 4,
    /// <summary>
    /// RagFlow
    /// </summary>
    [Description("RagFlow")]
    RagFlow = 5,
    /// <summary>
    /// Dify
    /// </summary>
    [Description("Dify")]
    Dify = 6,
    /// <summary>
    /// Coze
    /// </summary>
    [Description("字节扣子")]
    Coze = 7,
}
