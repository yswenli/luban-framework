namespace LuBan.Orm.Attributes;

/// <summary>
/// 所属用户数据权限
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class OwnerUserAttribute : Attribute
{
}
