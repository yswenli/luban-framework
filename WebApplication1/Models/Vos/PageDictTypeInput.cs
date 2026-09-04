/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Models.Vos
*文件名： PageDictTypeInput.cs
*版本号： V1.0.0.0
*唯一标识：f80c51d6-143f-45ea-8e94-4e5a1cf47486
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：PageDictTypeInput 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：PageDictTypeInput 类
*
*****************************************************************************/

namespace WebApplication1.Models.Vos
{

    public class DictTypeInput : BaseIdInput
    {
        /// <summary>
        /// 状态
        /// </summary>
        public EnumEnableStatus Status { get; set; }
    }

    public class PageDictTypeInput : BasePageInput
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }
    }

    public class AddDictTypeInput : DbDictType
    {
    }

    public class UpdateDictTypeInput : AddDictTypeInput
    {
    }

    public class DeleteDictTypeInput : BaseIdInput
    {
    }

    public class GetDataDictTypeInput
    {
        /// <summary>
        /// 编码
        /// </summary>
        [Required(ErrorMessage = "字典类型编码不能为空")]
        public string Code { get; set; }
    }
}
