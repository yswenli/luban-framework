/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Models
*文件名： BaseIdInput
*版本号： V1.0.0.0
*唯一标识：d5ffe92f-094f-438b-b91c-02bae33a74d3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 16:28:31
*描述：主键Id输入参数
*
*=================================================
*修改标记
*修改时间：2023/12/4 16:28:31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：主键Id输入参数
*
*****************************************************************************/

namespace LuBan.Orm.Models;

/// <summary>
/// 主键Id输入参数
/// </summary>
public class BaseIdInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public virtual long Id { get; set; }
}
