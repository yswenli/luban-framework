/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： BrowserToolOptions
*版本号： V1.0.0.0
*唯一标识：00a76bc2-0feb-40ff-8bf0-78babbb51a09
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：浏览器工具配置选项
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：浏览器工具配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 浏览器工具配置
/// </summary>
public class BrowserToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 是否使用无头模式
    /// </summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int Timeout { get; set; } = 30000;

    /// <summary>
    /// 浏览器引擎：chromium, firefox, webkit
    /// </summary>
    public string Engine { get; set; } = "chromium";
}