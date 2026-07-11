/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication.Models
*文件名： BusUserInfo
*版本号： V1.0.0.0
*唯一标识：17de56fc-cacb-4ce5-a458-b60183884d87
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/2 20:47:31
*描述：医院信息表
*
*=================================================
*修改标记
*修改时间：2024/12/2 20:47:31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：医院信息表
*
*****************************************************************************/
using System.ComponentModel.DataAnnotations;

using LuBan.Orm.Models;

using SqlSugar;

namespace WebApplication.Models
{
    /// <summary>
    /// 医院信息表
    /// </summary>
    [SugarTable("db_hospital_info", "医院信息表")]
    public class BusHospitalInfo : EntityBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(ColumnDescription = "名称", Length = 150)]
        [MaxLength(150)]
        public string Name { get; set; }
    }
}
