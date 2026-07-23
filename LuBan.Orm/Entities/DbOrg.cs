/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： SysOrg
*版本号： V1.0.0.0
*唯一标识：58e52580-4207-455a-bb46-efd8c0bc6b0a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:21:22
*描述：系统机构表
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:21:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统机构表
*
*****************************************************************************/
namespace LuBan.Orm.Entities
{

    /// <summary>
    /// 系统机构表
    /// </summary>
    [SugarTable("db_org", "系统机构表")]
    [SysTable]
    public class DbOrg : EntityDataScoreBase
    {
        /// <summary>
        /// 父Id
        /// </summary>
        [SugarColumn(ColumnDescription = "父Id")]
        public long Pid { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(ColumnDescription = "名称", Length = 64)]
        [Required, MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        [SugarColumn(ColumnDescription = "编码", Length = 64)]
        [MaxLength(64)]
        public string? Code { get; set; }

        /// <summary>
        /// 级别
        /// </summary>
        [SugarColumn(ColumnDescription = "级别")]
        public int? Level { get; set; }

        /// <summary>
        /// 机构类型-数据字典
        /// </summary>
        [SugarColumn(ColumnDescription = "机构类型", Length = 64)]
        [MaxLength(64)]
        public string? Type { get; set; }

        /// <summary>
        /// 负责人Id
        /// </summary>
        [SugarColumn(ColumnDescription = "负责人Id", IsNullable = true)]
        public long? DirectorId { get; set; }

        /// <summary>
        /// 负责人
        /// </summary>
        [Navigate(NavigateType.OneToOne, nameof(DirectorId)), JsonIgnore]
        public DbUser Director { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SugarColumn(ColumnDescription = "排序")]
        public int OrderNo { get; set; } = 100;

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(ColumnDescription = "状态")]
        public EnumEnableStatus Status { get; set; } = EnumEnableStatus.Enable;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnDescription = "备注", Length = 128)]
        [MaxLength(128)]
        public string? Remark { get; set; }

        /// <summary>
        /// 机构子项
        /// </summary>
        [SugarColumn(IsIgnore = true), JsonIgnore]
        public List<DbOrg> Children { get; set; }

        /// <summary>
        /// 是否禁止选中
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool Disabled { get; set; }
    }
}
