/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Models.Entities
*文件名： DictDataInput.cs
*版本号： V1.0.0.0
*唯一标识：e1186e51-2f9f-46fb-8164-9d253a42b08e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：DictDataInput 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：DictDataInput 类
*
*****************************************************************************/

namespace WebApplication1.Models.Entities
{

    public class DictDataInput : BaseIdInput
    {
        /// <summary>
        /// 状态
        /// </summary>
        public EnumEnableStatus Status { get; set; }
    }

    public class PageDictDataInput : BasePageInput
    {
        /// <summary>
        /// 字典类型Id
        /// </summary>
        public long DictTypeId { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }
    }

    public class AddDictDataInput : DbDictData
    {
    }

    public class UpdateDictDataInput : AddDictDataInput
    {
    }

    public class DeleteDictDataInput : BaseIdInput
    {
    }

    public class GetDataDictDataInput
    {
        /// <summary>
        /// 字典类型Id
        /// </summary>
        [Required(ErrorMessage = "字典类型Id不能为空")]
        public long DictTypeId { get; set; }
    }

    public class QueryDictDataInput
    {
        /// <summary>
        /// 编码
        /// </summary>
        [Required(ErrorMessage = "字典唯一编码不能为空")]
        public string Code { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int? Status { get; set; }
    }
}
