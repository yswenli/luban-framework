/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： AttachmentsOptions
*版本号： V1.0.0.0
*唯一标识：b7d4f0a2-3c61-4e8f-9a52-1d7c6e0b8f34
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：附件来源解析与落盘配置。
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件来源解析与落盘配置。
*
*****************************************************************************/

namespace LuBan.AIAgent.Configuration;

/// <summary>附件来源解析与落盘配置。</summary>
public class AttachmentsOptions
{
    /// <summary>是否启用附件能力；关闭时宿主应直接拒绝带附件的请求。</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>单轮附件来源数上限（≤0 表示不限）；超出部分由宿主截断并下发 <c>attachment.skipped</c>。</summary>
    public int MaxCount { get; set; } = 5;

    /// <summary>单文件大小上限（字节）：URL 下载闸门，也是本地文件闸门。默认 25MB。</summary>
    public long MaxFileBytes { get; set; } = 25 * 1024 * 1024;

    /// <summary>单个 URL 下载超时（秒）。</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>URL 落盘目录（相对当前用户工作区根）。</summary>
    public string RootDirectory { get; set; } = ".luban-agent/attachments";

    /// <summary>
    /// 相对路径的第二基准（站点/程序内容根）。留空 = 不做站点相对解析。
    /// 由 <c>LuBanAgentConfigResolver.CreateEffectiveConfiguration</c> 归一化为绝对路径。
    /// </summary>
    public string? ContentRoot { get; set; }
}