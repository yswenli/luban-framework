/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Interfaces
*文件名： ILuBanOrmView
*版本号： V1.0.0.0
*唯一标识：00505bc9-9e44-4229-a7a2-0b99957c4d37
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/23 14:24:57
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/23 14:24:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Orm.Interfaces;

/// <summary>
/// 视图实体接口
/// </summary>
public interface ILuBanOrmView
{
    /// <summary>
    /// 获取查询sql语句
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public string GetViewSql(SqlSugarScopeProvider db);
}
