/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.DI.Attrs
*文件名： InjectionAttribute
*版本号： V1.0.0.0
*唯一标识：3c308517-ca06-4e8a-a6ff-729cbc52f357
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/6 14:25:17
*描述：设置依赖注入方式
*
*=================================================
*修改标记
*修改时间：2023/12/6 14:25:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：设置依赖注入方式
*
*****************************************************************************/
namespace LuBan.DI.Attrs;


/// <summary>
/// 设置依赖注入方式
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class InjectionAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="exceptInterfaces"></param>
    public InjectionAttribute(params Type[] exceptInterfaces)
        : this(EnumInjectionActions.Add, exceptInterfaces)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="action"></param>
    /// <param name="exceptInterfaces"></param>
    public InjectionAttribute(EnumInjectionActions action, params Type[] exceptInterfaces)
    {
        Action = action;
        Pattern = EnumInjectionPatterns.All;
        ExceptInterfaces = exceptInterfaces ?? Array.Empty<Type>();
        Order = 0;
    }

    /// <summary>
    /// 添加服务方式，存在不添加，或继续添加
    /// </summary>
    public EnumInjectionActions Action { get; set; }

    /// <summary>
    /// 注册选项
    /// </summary>
    public EnumInjectionPatterns Pattern { get; set; }

    /// <summary>
    /// 注册别名
    /// </summary>
    /// <remarks>多服务时使用</remarks>
    public string? Named { get; set; }

    /// <summary>
    /// 排序，排序越大，则在后面注册
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 排除接口
    /// </summary>
    public Type[] ExceptInterfaces { get; set; }

    /// <summary>
    /// 代理类型，必须继承 AspectDispatchProxy，为 null 时不注册代理
    /// </summary>
    public Type? Proxy { get; set; }
}


