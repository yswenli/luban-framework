/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage
*文件名： EnumSupplierType
*版本号： V1.0.0.0
*唯一标识：fd044504-2b12-42be-820b-227fa6d61e5b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/9 18:02:23
*描述：云存储类型
*
*=================================================
*修改标记
*修改时间：2024/7/9 18:02:23
*修改人： yswenli
*版本号： V1.0.0.0
*描述：云存储类型
*
*****************************************************************************/
namespace LuBan.CloudStorage;

/// <summary>
/// 云存储类型
/// </summary>
public enum EnumSupplierType
{
    [Description("未知")]
    None = 0,

    /// <summary>
    /// 阿里云
    /// </summary>
    [Description("阿里云")]
    Aliyun = 1,

    /// <summary>
    /// 微软云
    /// </summary>
    [Description("微软云")]
    Azure = 2,
    /// <summary>
    /// MinIO
    /// </summary>
    [Description("MinIO")]
    MinIO = 3,
}
