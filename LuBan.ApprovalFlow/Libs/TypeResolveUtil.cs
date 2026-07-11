/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Libs
*文件名： TypeResolveUtil
*版本号： V1.0.0.0
*唯一标识：6b62763c-a55a-4987-ab70-dfd421721d4d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:49:58
*描述：类型解析工具：通过类型全名或简单名在已加载类型中查找。
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:49:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：类型解析工具：通过类型全名或简单名在已加载类型中查找。
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Libs;

/// <summary>
/// 类型解析工具：通过类型全名或简单名在已加载类型中查找。
/// </summary>
public static class TypeResolveUtil
{
    /// <summary>
    /// 尝试解析类型（忽略大小写）。
    /// </summary>
    /// <param name="name">类型全名或简单类名。</param>
    /// <returns>解析到的类型；未找到返回 <c>null</c>。</returns>
    public static Type? TryResolveTypeByName(string name)
    {
        var types = RuntimeUtil.GetTypes();
        var t = types?.FirstOrDefault(u => string.Equals(u.FullName, name, StringComparison.OrdinalIgnoreCase) || string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase));
        if (t != null) return t;
        t = types?.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || (u.FullName?.EndsWith("." + name, StringComparison.OrdinalIgnoreCase) ?? false));
        return t;
    }
}