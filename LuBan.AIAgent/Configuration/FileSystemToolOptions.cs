/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： FileSystemToolOptions
*版本号： V1.0.0.0
*唯一标识：613ab72b-302e-4045-b076-56eae71137f0
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：文件系统工具配置选项
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文件系统工具配置选项
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 文件系统工具配置
/// </summary>
public class FileSystemToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 允许访问的根目录列表
    /// </summary>
    public List<string> AllowedRoots { get; set; } = new();
}