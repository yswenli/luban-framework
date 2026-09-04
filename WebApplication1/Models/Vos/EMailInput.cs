/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Models.Vos
*文件名： EMailInput.cs
*版本号： V1.0.0.0
*唯一标识：cb1d8afa-1647-4fb5-af66-13d3fdead473
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：EMailInput 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：EMailInput 类
*
*****************************************************************************/

using LuBan.EMailKit.Models;

namespace WebApplication1.Models.Vos;

public class EMailInput : MsgInput
{
    /// <summary>
    /// 上传文件列表
    /// </summary>
    [Required(ErrorMessage = "上传文件不能为空")]
    public List<IFormFile> Files { get; set; }
}
