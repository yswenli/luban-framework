/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage
*文件名： NewUrlAttribute
*版本号： V1.0.0.0
*唯一标识：af627f18-3e46-44d4-9da6-6eb1b5bf4a05
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/9 18:04:01
*描述：
*用于标记某些字段需要过期的url更新为有效的临时url
*=================================================
*修改标记
*修改时间：2024/7/9 18:04:01
*修改人： yswenli
*版本号： V1.0.0.0
*描述：用于标记某些字段需要过期的url更新为有效的临时url
*
*****************************************************************************/
namespace LuBan.CloudStorage;

/// <summary>
/// 用于标记某些字段需要过期的url更新为有效的临时url
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class NewUrlAttribute : Attribute
{
    /// <summary>
    /// 获取或设置过期时间（秒），默认一天
    /// </summary>
    public int ExpireMinutes { get; set; }

    /// <summary>
    /// 初始化NewUrlAttribute实例
    /// </summary>
    public NewUrlAttribute()
    {
    }

    /// <summary>
    /// 使用指定的过期时间初始化NewUrlAttribute实例
    /// </summary>
    /// <param name="expireMinutes">过期时间（秒），默认一天</param>
    public NewUrlAttribute(int expireMinutes = 60 * 60 * 24)
    {
        ExpireMinutes = expireMinutes;
    }

}
