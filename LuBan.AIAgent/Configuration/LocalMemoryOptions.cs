/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： LocalMemoryOptions
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：本地长期记忆配置选项
*
*=================================================
*修改标记
*修改时间：2026/8/4
*修改人： yswenli
*版本号： V1.0.0.0
*描述：本地长期记忆配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 本地长期记忆配置选项
/// </summary>
public class LocalMemoryOptions
{
    /// <summary>
    /// 是否启用本地长期记忆工具
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// SQLite 数据库路径，留空则使用默认用户数据目录
    /// </summary>
    public string? DatabasePath { get; set; }

    /// <summary>
    /// 语义搜索返回的最大条数
    /// </summary>
    public int DefaultTopK { get; set; } = 5;

    /// <summary>
    /// 无本地 Embedding 模型时的退化向量维度
    /// </summary>
    public int FallbackDimension { get; set; } = 256;
}
