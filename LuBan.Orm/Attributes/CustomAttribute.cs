/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Attributes
*文件名： CustomAttribute
*版本号： V1.0.0.0
*唯一标识：17ac920f-0788-4eee-9480-8b747d3201cd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/31 11:53:25
*描述：自定义验证
*
*=================================================
*修改标记
*修改时间：2024/7/31 11:53:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：自定义验证
*
*****************************************************************************/
namespace LuBan.Orm.Attributes;

/// <summary>
/// 自定义验证
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class CustomAttribute<T> : ValidationAttribute where T : class, new()
{
    string _errorMsg, _methodName;

    static readonly T _instance = new();
    static ConcurrentDictionary<string, MethodInfo?> _cache = new ConcurrentDictionary<string, MethodInfo?>();

    /// <summary>
    /// 自定义验证
    /// </summary>
    /// <param name="errorMsg"></param>
    /// <param name="methodName"></param>
    public CustomAttribute(string errorMsg, string methodName)
    {
        _errorMsg = errorMsg;
        _methodName = methodName;
    }

    /// <summary>
    /// 验证
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var type = typeof(T);
        var key = $"{type.FullName}.{_methodName}";
        var method = _cache.GetOrAdd(key, (k) => type.GetMethod(_methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

        if (method == null) throw new Exception($"Must implement the specified custom validation logic method '{_methodName}'");
        try
        {
            var result = method.Invoke(_instance, [value]);
            if (result != null && result is bool r)
            {
                if (!r)
                {
                    return new ValidationResult(_errorMsg);
                }
            }
            else
            {
                throw new Exception($"Method '{_methodName}' must have a boolean return value.");
            }
        }
        catch (Exception ex)
        {
            return new ValidationResult($"{ex}");
        }
        return ValidationResult.Success;
    }
}
