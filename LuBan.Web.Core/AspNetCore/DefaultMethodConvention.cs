/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core
*文件名： DefaultMethodConvention
*版本号： V1.0.0.0
*唯一标识：8141966f-6bd6-4de3-867c-59c9874905ff
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/6 16:45:26
*描述：接口控制器应用模型转换器
*
*=================================================
*修改标记
*修改时间：2023/12/6 16:45:26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：接口控制器应用模型转换器
*
*****************************************************************************/

namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 接口控制器应用模型转换器,
/// 主要用于未指定接口谓词时，自动添加谓词
/// </summary>
public class DefaultMethodConvention : IApplicationModelConvention
{
    IServiceCollection _services;

    /// <summary>
    /// 接口控制器应用模型转换器
    /// </summary>
    /// <param name="services"></param>
    public DefaultMethodConvention(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// apply
    /// </summary>
    /// <param name="application"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Apply(ApplicationModel application)
    {
        //获取全部的controller
        var controllers = application.Controllers.Where(u => Penetrates.IsApiController(u.ControllerType));
        foreach (var controller in controllers)
        {
            var controllerType = controller.ControllerType;
            foreach (var action in controller.Actions)
            {
                // 配置动作方法规范化特性
                // 获取真实类型
                var returnType = action.ActionMethod.GetRealReturnType();
                if (returnType == typeof(void)) continue;
                var selectorModel = action.Selectors[0];
                // 跳过已配置请求谓词特性的配置
                if (selectorModel.ActionConstraints.Any(u => u is HttpMethodActionConstraint)) continue;

                // 解析请求谓词
                var words = action.ActionMethod.Name.SplitCamelCase();
                var verbKey = words.First().ToLower();

                // 处理类似 getlist,getall 多个单词
                if (words.Length > 1 && Penetrates.VerbToHttpMethods.ContainsKey((words[0] + words[1]).ToLower()))
                {
                    verbKey = (words[0] + words[1]).ToLower();
                }

                var succeed = Penetrates.VerbToHttpMethods.TryGetValue(verbKey, out var verbValue);
                var verb = succeed ? verbValue : "POST";

                // 添加请求约束
                selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint([verb!]));

                // 添加请求谓词特性
                HttpMethodAttribute httpMethodAttribute = verb switch
                {
                    "GET" => new HttpGetAttribute(),
                    "POST" => new HttpPostAttribute(),
                    "PUT" => new HttpPutAttribute(),
                    "DELETE" => new HttpDeleteAttribute(),
                    "PATCH" => new HttpPatchAttribute(),
                    "HEAD" => new HttpHeadAttribute(),
                    _ => throw new NotSupportedException($"{verb}")
                };

                selectorModel.EndpointMetadata.Add(httpMethodAttribute);
            }


        }
    }
}


/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 分组分隔符
    /// </summary>
    internal const string GroupSeparator = "##";

    /// <summary>
    /// 请求动词映射字典
    /// </summary>
    internal static ConcurrentDictionary<string, string> VerbToHttpMethods { get; private set; }

    /// <summary>
    /// 控制器排序集合
    /// </summary>
    internal static ConcurrentDictionary<string, (string, int, Type)> ControllerOrderCollection { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    static Penetrates()
    {
        ControllerOrderCollection = new ConcurrentDictionary<string, (string, int, Type)>();

        VerbToHttpMethods = new ConcurrentDictionary<string, string>
        {
            ["post"] = "POST",
            ["add"] = "POST",
            ["create"] = "POST",
            ["insert"] = "POST",
            ["submit"] = "POST",

            ["get"] = "GET",
            ["find"] = "GET",
            ["fetch"] = "GET",
            ["query"] = "GET",
            ["getlist"] = "GET",
            ["getall"] = "GET",

            ["put"] = "PUT",
            ["update"] = "PUT",
            ["delete"] = "DELETE",
            ["remove"] = "DELETE",
            ["clear"] = "DELETE",

            ["patch"] = "PATCH"
        };

        IsApiControllerCached = new ConcurrentDictionary<Type, bool>();
    }

    /// <summary>
    /// <see cref="IsApiController(Type)"/> 缓存集合
    /// </summary>
    private static readonly ConcurrentDictionary<Type, bool> IsApiControllerCached;

    /// <summary>
    /// 是否是Api控制器
    /// </summary>
    /// <param name="type">type</param>
    /// <returns></returns>
    internal static bool IsApiController(Type type)
    {
        return IsApiControllerCached.GetOrAdd(type, Function);

        // 本地静态方法
        static bool Function(Type type)
        {
            var typeName = type.Assembly.GetName().Name;

            // 排除 OData 控制器
            if (typeName.IsNullOrEmpty() || typeName.StartsWith("Microsoft.AspNetCore.OData")) return false;

            // 不能是非公开、基元类型、值类型、抽象类、接口、泛型类
            if (!type.IsPublic || type.IsPrimitive || type.IsValueType || type.IsAbstract || type.IsInterface || type.IsGenericType) return false;

            // 继承 ControllerBase 或 实现 IDynamicApiController 的类型 或 贴了 [DynamicApiController] 特性
            if (!typeof(Controller).IsAssignableFrom(type) && typeof(ControllerBase).IsAssignableFrom(type) || type.IsDefined(typeof(ApiControllerAttribute), true)) return true;

            return false;
        }
    }
}
