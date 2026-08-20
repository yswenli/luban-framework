/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： RetrievalToolOptions
*版本号： V1.0.0.0
*唯一标识：44023d29-f825-4618-b68e-2b2c8add125e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：检索工具配置选项
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：检索工具配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 语义检索工具配置
/// </summary>
public class RetrievalToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 嵌入模型标识
    /// </summary>
    public string ModelId { get; set; } = "bge-small-zh-v1.5";

    /// <summary>
    /// 启动时是否自动下载缺失的模型
    /// </summary>
    public bool AutoDownload { get; set; } = true;

    /// <summary>
    /// 索引单文件最大体积（KB）
    /// </summary>
    public int MaxFileSizeKB { get; set; } = 5120;

    /// <summary>
    /// 默认返回结果数
    /// </summary>
    public int DefaultTopK { get; set; } = 5;

    /// <summary>
    /// 单次搜索返回内容最大字符数
    /// </summary>
    public int MaxResultChars { get; set; } = 8000;
}
