/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Models
*文件名： ViewSysUser
*版本号： V1.0.0.0
*唯一标识：d4829a0e-5080-4b95-8426-d4632cd7f051
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/23 14:53:13
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/23 14:53:13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using LuBan.Orm.Attributes;
using LuBan.Orm.Interfaces;

namespace WebApplication1.Models;


/// <summary>
/// 用户表视图（必须加IgnoreTable，防止被生成为表）
/// </summary>
[SugarTable("db_sys_user", "用户表视图"), IgnoreTable]
public class ViewSysUser : EntityBase, ILuBanOrmView
{
    /// <summary>
    /// 账号
    /// </summary>
    [SugarColumn(ColumnDescription = "账号")]
    public virtual string Account { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "真实姓名")]
    public virtual string RealName { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarColumn(ColumnDescription = "昵称")]
    public string? NickName { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    [SugarColumn(ColumnDescription = "机构名称")]
    public string? OrgName { get; set; }

    /// <summary>
    /// 职位名称
    /// </summary>
    [SugarColumn(ColumnDescription = "职位名称")]
    public string? PosName { get; set; }

    /// <summary>
    /// 查询实例
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public string GetViewSql(SqlSugarScopeProvider db)
    {
        return db.Queryable<DbUser>()
            .LeftJoin<DbOrg>((u, a) => u.OrgId == a.Id)
            .LeftJoin<DbPos>((u, a, b) => u.PosId == b.Id)
            .Select((u, a, b) => new ViewSysUser
            {
                Id = u.Id,
                Account = u.Account,
                RealName = u.RealName,
                NickName = u.NickName,
                OrgName = a.Name,
                PosName = b.Name,
            }).GetQueryableSqlString();
    }
}