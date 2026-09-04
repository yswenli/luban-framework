/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Internal
*文件名： HomeController.cs
*版本号： V1.0.0.0
*唯一标识：785c273d-a260-4706-8425-183d12e70396
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/21 18:21:26
*描述：HomeController 控制器
*
*=================================================
*修改标记
*修改时间：2026/8/21 18:21:26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：HomeController 控制器
*
*****************************************************************************/


namespace WebApplication1.Controllers.Internal;

/// <summary>
/// HomeController 控制器
/// </summary>

public class HomeController : BaseInternalController
{
    [HttpGet]
    public string Index()
    {
        return "hello internal api service";
    }
}
