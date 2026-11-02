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
*命名空间：LuBan.AIAgent.Tools.FileSystem
*文件名： EditOperation
*版本号： V1.0.0.0
*唯一标识：3d2662fb-6b06-4365-8bdb-2e2eb25b415b
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9
*描述：批量编辑的单条替换操作
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.FileSystem;

/// <summary>
/// 批量编辑（MultiEdit）中的单条精确字符串替换操作。
/// 使用 class + 可写属性，供 <c>AIFunctionFactory</c> 从 JSON 反序列化数组元素。
/// </summary>
public class EditOperation
{
    /// <summary>
    /// 待替换的原文（必须唯一或配合 <see cref="ReplaceAll"/>）。
    /// </summary>
    public string OldString { get; set; } = "";

    /// <summary>
    /// 替换后的新文本。
    /// </summary>
    public string NewString { get; set; } = "";

    /// <summary>
    /// 是否允许替换全部匹配；false 时多匹配视为歧义并报错。
    /// </summary>
    public bool ReplaceAll { get; set; }
}
