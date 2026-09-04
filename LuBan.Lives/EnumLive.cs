/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Lives
*文件名： EnumLive.cs
*版本号： V1.0.0.0
*唯一标识：48943cde-0964-4a15-8cab-e1d22d89a8dc
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：EnumLive 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：EnumLive 类
*
*****************************************************************************/


namespace LuBan.Lives;

/// <summary>
/// 直播类别
/// </summary>
public enum EnumLive
{
    /// <summary>
    /// 拓麦
    /// </summary>
    [Description("拓麦")]
    TalkMed = 1,
    /// <summary>
    /// 100直播
    /// </summary>
    [Description("100直播")]
    YiBai = 2,
    /// <summary>
    /// 会畅直播
    /// </summary>
    [Description("会畅直播")]
    HuiChang = 3,
    /// <summary>
    /// 微赞直播
    /// </summary>
    [Description("微赞直播")]
    VZan = 4

}
