/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Mobile
*文件名： SafeController.cs
*版本号： V1.0.0.0
*唯一标识：0effd86b-8eb2-4484-8ecc-857b8e7007ea
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：SafeController 控制器
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SafeController 控制器
*
*****************************************************************************/



namespace WebApplication1.Controllers.Mobile;

/// <summary>
/// 接口安全参数校验测试
/// </summary>
[AraParameterFilter]
public class SafeController : BaseMobileController
{
    /// <summary>
    /// test1
    /// </summary>
    /// <returns></returns>
    [HttpGet, AllowAnonymous, NoAraParameterFilter]
    public Result Test1()
    {
        return SuccessResult();
    }

    /// <summary>
    /// Test2
    /// </summary>
    /// <param name="testInfo"></param>
    /// <returns></returns>
    [HttpPost]
    public Result Test2([FromBody] TestInfo testInfo)
    {
        return SuccessResult(testInfo);
    }


}

public class TestInfo
{
    public int Id { get; set; }
}
