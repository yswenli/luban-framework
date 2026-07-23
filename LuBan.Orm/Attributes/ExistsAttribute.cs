/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Attributes
*文件名： ExistsAttribute
*版本号： V1.0.0.0
*唯一标识：096e926b-2c66-4f81-a8c7-3de9e2414452
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/30 17:31:39
*描述：检查数据是否存在
*
*=================================================
*修改标记
*修改时间：2024/7/30 17:31:39
*修改人： yswenli
*版本号： V1.0.0.0
*描述：检查数据是否存在
*
*****************************************************************************/
namespace LuBan.Orm.Attributes;

/// <summary>
/// 检查数据是否存在
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ExistsAttribute : ValidationAttribute
{
    string _errorMsg;

    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; }
    /// <summary>
    /// 列名
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 检查数据是否存在
    /// </summary>
    public ExistsAttribute(string errorMsg = "")
    {
        _errorMsg = errorMsg;
    }

    /// <summary>
    /// 检查数据是否存在
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var className = validationContext.ObjectType.Name;
        var propertyName = validationContext.MemberName;
        if (propertyName.IsNullOrEmpty()) return new ValidationResult($"The input {validationContext.MemberName} value cannot be empty.");
        var tableName = LuBanOrm.GetTableName(className);
        if (TableName.IsNotNullOrEmpty())
        {
            tableName = TableName;
        }
        var columnName = LuBanOrm.GetColumnName(propertyName);
        if (ColumnName.IsNotNullOrEmpty())
        {
            columnName = ColumnName;
        }
        if (value == null) return new ValidationResult($"The input {validationContext.DisplayName} value cannot be empty.");
        if (value is string str && str.IsNullOrEmpty()) return new ValidationResult($"The input {validationContext.DisplayName} value cannot be empty.");
        if (columnName.IsNullOrEmpty())
        {
            return ValidationResult.Success;
        }
        try
        {
            if (!SqlUtil.Exists(tableName, columnName, value))
            {
                if (_errorMsg.IsNullOrEmpty())
                {
                    return new ValidationResult($"The data with attribute '{propertyName}' value '{value}' does not exist.");
                }
                else
                {
                    return new ValidationResult(_errorMsg);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"ExistsAttribute validation failed for {className}.{propertyName}", ex);
            return new ValidationResult("数据校验时发生异常，请稍后重试。");
        }
        return ValidationResult.Success;
    }

}
