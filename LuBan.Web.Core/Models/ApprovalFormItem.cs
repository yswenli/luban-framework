/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Models
*文件名： ApprovalFormItem
*版本号： V1.0.0.0
*唯一标识：00ee4a55-db7a-4e27-a2d5-febf8b983aca
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/22 14:51:51
*描述：ApprovalFormItem
*
*=================================================
*修改标记
*修改时间：2025/10/22 14:51:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ApprovalFormItem
*
*****************************************************************************/
namespace LuBan.Web.Core.Models;
/// <summary>
/// ApprovalFormItem
/// </summary>
public class ApprovalFormItem
{
    /// <summary>
    /// 审批表单配置的标识（JSON 字段 configId）。
    /// </summary>
    [JsonPropertyName("configId")]
    public string ConfigId { get; set; }

    /// <summary>
    /// 审批表单对应的数据库表名（JSON 字段 tableName）。
    /// </summary>
    [JsonPropertyName("tableName")]
    public string TableName { get; set; }

    /// <summary>
    /// 审批表单对应的实体类名称（JSON 字段 entityName）。
    /// </summary>
    [JsonPropertyName("entityName")]
    public string EntityName { get; set; }

    /// <summary>
    /// 审批表单的类型名称（JSON 字段 typeName）。
    /// </summary>
    [JsonPropertyName("typeName")]
    public string TypeName { get; set; }

    /// <summary>
    /// 由实体名首字母小写与类型名拼接出的前端访问路由（JSON 字段 route）。
    /// </summary>
    [JsonPropertyName("route")]
    public string Route => EntityName[..1].ToLower() + EntityName[1..] + "/" + TypeName;
}


/// <summary>
/// 审批流输出参数
/// </summary>
public class ApprovalFlowOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 编号
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 表单
    /// </summary>
    public string? FormJson { get; set; }

    /// <summary>
    /// 流程
    /// </summary>
    public string? FlowJson { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }

    /// <summary>
    /// 创建者部门Id
    /// </summary>
    public long? CreateOrgId { get; set; }

    /// <summary>
    /// 创建者部门名称
    /// </summary>
    public string? CreateOrgName { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    public bool IsDelete { get; set; }
}