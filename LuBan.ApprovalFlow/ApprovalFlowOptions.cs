/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow
*文件名： ApprovalFlowOptions
*版本号： V1.0.0.0
*唯一标识：20c343a4-765b-4d7c-97de-f5004b2ad757
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:37:50
*描述：审批流配置选项
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:37:50
*修改人： yswenli
*版本号： V1.0.0.0
*描述：审批流配置选项
*
*****************************************************************************/
namespace LuBan.ApprovalFlow;

/// <summary>
/// 审批流配置选项
/// </summary>
public class ApprovalFlowOptions
{
    /// <summary>
    /// 数据目录，默认为 "data"
    /// </summary>
    public string? DataDir { get; set; } = "data";

    /// <summary>
    /// 是否自动审批，默认为 false
    /// </summary>
    public bool? AutoApproval { get; set; } = false;

    /// <summary>
    /// 线程池大小，默认为 10
    /// </summary>
    public int ThreadPoolSize { get; set; } = 10;


    /// <summary>
    /// 默认配置
    /// </summary>
    public static ApprovalFlowOptions Default
    {
        get
        {
            return NacosConfigUtil.Read<ApprovalFlowOptions>("ApprovalFlowOptions") ?? new ApprovalFlowOptions();
        }
    }
}
