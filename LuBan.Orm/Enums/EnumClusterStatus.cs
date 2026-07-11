/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumClusterStatus
*版本号： V1.0.0.0
*唯一标识：09c766d1-cfa1-4281-9ed2-7195454c1bf3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:06:16
*描述：作业集群状态
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:06:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：作业集群状态
*
*****************************************************************************/
namespace LuBan.Orm.Enums;


/// <summary>
/// 作业集群状态
/// </summary>
public enum EnumClusterStatus : uint
{
    /// <summary>
    /// 宕机
    /// </summary>
    Crashed = 0,

    /// <summary>
    /// 工作中
    /// </summary>
    Working = 1,

    /// <summary>
    /// 等待被唤醒
    /// </summary>
    Waiting = 2
}
