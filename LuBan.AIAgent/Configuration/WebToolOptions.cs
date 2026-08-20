/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： WebToolOptions
*版本号： V1.0.0.0
*唯一标识：21dda661-7813-4492-9b27-899a39586d01
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：网页工具配置选项
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：网页工具配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Web 工具配置
/// </summary>
public class WebToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 最大响应字符数
    /// </summary>
    public int MaxCharacters { get; set; } = 12000;
}