

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
