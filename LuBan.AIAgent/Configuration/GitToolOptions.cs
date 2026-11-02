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
*文件名： GitToolOptions
*版本号： V1.0.0.0
*唯一标识：e8fdfd77-bb9a-4acb-ac79-76d7a678e6ac
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9
*描述：Git 工具配置选项
*
*****************************************************************************/

namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Git 工具配置。
/// </summary>
public class GitToolOptions
{
    /// <summary>
    /// 是否启用 Git 工具组（默认启用）。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 默认超时时间（毫秒），与脚本工具对齐。
    /// </summary>
    public int DefaultTimeout { get; set; } = 120000;
}
