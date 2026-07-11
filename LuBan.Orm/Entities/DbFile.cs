/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：系统文件表
*文件名： 
*版本号： V1.0.0.0
*唯一标识：a5bb6173-b22d-4edd-852f-9b02bb075167
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/11/03 14:00:15
*描述：
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统文件表
*
*****************************************************************************/

namespace LuBan.Orm.Entities;

/// <summary>
/// 系统文件表
/// </summary>
[SugarTable("db_file", "系统文件表")]
[SysTable]
public class DbFile : EntityBase
{
    /// <summary>
    /// 提供者
    /// </summary>
    [SugarColumn(ColumnDescription = "提供者", Length = 128)]
    [MaxLength(128)]
    public string? Provider { get; set; }

    /// <summary>
    /// 仓储名称
    /// </summary>
    [SugarColumn(ColumnDescription = "仓储名称", Length = 128)]
    [MaxLength(128)]
    public string? BucketName { get; set; }

    /// <summary>
    /// 文件名称（源文件名）
    /// </summary>
    [SugarColumn(ColumnDescription = "文件名称", Length = 128)]
    [MaxLength(128)]
    public string? FileName { get; set; }

    /// <summary>
    /// 文件后缀
    /// </summary>
    [SugarColumn(ColumnDescription = "文件后缀", Length = 16)]
    [MaxLength(16)]
    public string? Suffix { get; set; }

    /// <summary>
    /// 存储路径
    /// </summary>
    [SugarColumn(ColumnDescription = "存储路径", Length = 128)]
    [MaxLength(1024)]
    public string? FilePath { get; set; }

    /// <summary>
    /// 文件大小KB
    /// </summary>
    [SugarColumn(ColumnDescription = "文件大小KB", Length = 16)]
    [MaxLength(16)]
    public string? SizeKb { get; set; }

    /// <summary>
    /// 文件大小信息-计算后的
    /// </summary>
    [SugarColumn(ColumnDescription = "文件大小信息", Length = 64)]
    [MaxLength(64)]
    public string? SizeInfo { get; set; }

    /// <summary>
    /// 外链地址
    /// </summary>
    [SugarColumn(ColumnDescription = "外链地址", Length = 1024)]
    [MaxLength(1024)]
    public string? Url { get; set; }

    /// <summary>
    /// 海报外链地址
    /// </summary>
    [SugarColumn(ColumnDescription = "海报外链地址", Length = 1024)]
    [MaxLength(1024)]
    public string? PosterUrl { get; set; }

    /// <summary>
    /// 文件MD5
    /// </summary>
    [SugarColumn(ColumnDescription = "文件MD5", Length = 128)]
    [MaxLength(128)]
    public string? FileMd5 { get; set; }

    /// <summary>
    /// 是否是私有
    /// </summary>
    [SugarColumn(ColumnDescription = "是否是私有")]
    public bool IsPrivate { get; set; }
}