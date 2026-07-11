

namespace WebApplication1.Controllers.Open;

/// <summary>
/// 验证
/// </summary>
public class AuthController : BaseOpenController
{
    /// <summary>
    /// 参数生成工具,
    /// 根据输入的内容生成对应的参数
    /// </summary>
    /// <param name="input">此处可为qeury、form的字符串或json等格式的值;query或form的value要使用url编码后的值</param>
    /// <returns></returns>
    [HttpPost, NoAraParameterFilter, NoOpenApiAccess]
    public dynamic AuthParmasTool([FromBody, DefaultValue("")] dynamic input)
    {
        return OpenApiAccessUtil.AuthParmasTool(input);
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [NoOpenApiAccess, HttpPost]
    public async Task<OpenAccessToken> LoginAsync([Required, FromBody] OpenAccessTokenInput input)
    {
        var timeout = 7200;
        return await OpenApiAccessUtil.GetRefreshTokenAsync(input.AccessKey, input.AccessSecret, timeout);
    }

    /// <summary>
    /// 获取acessToken
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    [NoOpenApiAccess, HttpPost]
    public async Task<AccessToken> GetAccessTokenAsync([Required] string refreshToken)
    {
        return await OpenApiAccessUtil.GetAccessTokenAsync(refreshToken, 7200);
    }

    /// <summary>
    /// 测试验证接口，
    /// 需要传入accesstoken,
    /// header:bearer {jwt}，
    /// 另外需加入防重放参数
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<bool> TestAsync()
    {
        return await Task.FromResult(true);
    }

}
