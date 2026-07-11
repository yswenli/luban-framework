namespace WebApplication1.Controllers.Mobile;


/// <summary>
/// 测试控制器，提供测试相关的接口
/// </summary>
public class TestController : BaseMobileController
{
    /// <summary>
    /// 无需认证的Ping接口，返回当前时间和客户端IP及区域信息
    /// </summary>
    /// <returns>包含时间、IP和区域信息的字符串</returns>
    [AllowAnonymous, HttpGet]
    public string Ping1()
    {
        return $"{DateTime.Now}\tip:{IPRegion.IP},region:{IPRegion.Region}";
    }

    /// <summary>
    /// 需要认证的Ping接口，返回当前时间、JWT令牌、客户端IP及区域信息
    /// </summary>
    /// <returns>包含时间、JWT令牌、IP和区域信息的字符串</returns>
    [HttpGet]
    public string Ping2()
    {
        return $"{DateTime.Now}\t{JwtTokenString}\r\nip:{IPRegion.IP},region:{IPRegion.Region}";
    }

    /// <summary>
    /// 测试错误的接口，用于验证异常处理
    /// </summary>
    /// <returns>一个整数值</returns>
    /// <exception cref="DivideByZeroException">当发生除以零的情况时抛出</exception>
    [HttpGet, AllowAnonymous, NoAraParameterFilter]
    public int Test()
    {
        var a = 1;
        var b = 1;
        var c = a / (b - 1); // 此处会抛出除以零的异常
        return c;
    }

    /// <summary>
    /// 获取sse流的测试接口
    /// </summary>
    /// <returns></returns>
    [HttpGet, AllowAnonymous, NoAraParameterFilter]
    public async Task GetSseStreamAsync()
    {
        var msg = new StringPlus("data: 这是一个测试消息。\r\n");
        msg.AppendLine("data: 试试sse");
        msg.AppendLine("data: 看看行不行");
        msg.AppendLine("data: [END]\r\n");
        using var stream = new SseStream(msg.ToStream());
        await stream.SendAsync();
    }
}
