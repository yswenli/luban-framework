using LuBan.Redis;
using LuBan.Web.Core.Caches;

using System.Diagnostics;
using System.Text;

using WebApplication1.Services.ApiServices;

namespace WebApplication1.Controllers;

/// <summary>
/// home
/// </summary>
[AllowAnonymous, NoAraParameterFilter]
public class HomeController : BaseApiController
{
    /// <summary>
    /// hello
    /// </summary>
    /// <returns></returns>
    [HttpPost, OutputCache, NoApiLog, IgnoreDataScopePermission]
    public async Task<string> Hello()
    {
        var data = ProjectOptions.Instance;
        Logger.Info("hello" + SessionUser.UserId);
        return await Task.FromResult("Hello");
    }

    /// <summary>
    /// Hello2
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Hello2([FromQuery] int a = 1)
    {
        Logger.Info("hello2a");
        if (a < 1)
        {
            _ = 10 / a;
        }
        return await ContentAsync("hello2b");
    }

    /// <summary>
    ///测试3
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<Result> Hello3([FromQuery, Required] int a)
    {
        if (a < 1) throw FriendlyError.Ex(EnumErrorCode.D1002);
        return await HomeService.Instance.Hello3(a);
    }

    /// <summary>
    /// 测试4
    /// </summary>
    /// <returns></returns>

    [HttpGet]
    public Result Hello4()
    {
        return new Fail(FriendlyError.Ex(EnumErrorCode.D1002));
    }

    /// <summary>
    /// 测试5
    /// </summary>
    [HttpPost]
    public async Task<Result> GetPagedList([FromBody, Required] PageInput input)
    {
        if (input.Page > 10) throw new Exception("页数不能大于10");
        return await Task.FromResult(new Success(true));
    }


    /// <summary>
    /// 测试请求流的读取
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<string> Recieve()
    {
        var str1 = await WebApp.HttpContext!.GetRequestBodyTextAsync(1000, Encoding.UTF8);
        var str2 = await WebApp.HttpContext!.GetRequestBodyTextAsync(1000, Encoding.UTF8);
        return await WebApp.HttpContext!.GetRequestBodyTextAsync(1000, Encoding.UTF8);
    }

    /// <summary>
    /// 测试异常处理
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public Result TestAutoCatchException()
    {
        return ErrorResult(new Exception("TestAutoCatchEx"));
    }


    /// <summary>
    /// TestCallContext
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<DbUser?> TestCallContext()
    {
        var sysUser = new DbUser()
        {
            Id = 12321,
            NickName = "Walle",
            Account = "superadmin",
            RealName = "Walle",
        };

        CallContext.SetData(sysUser);

        var task1 = Task.Run(async () =>
        {
            await Task.Delay(500);
            sysUser.Account = "admin1";
            CallContext.SetData(sysUser);
        });

        var task2 = Task.Run(async () =>
        {
            await Task.Delay(1500);
            var sysUser2 = CallContext.GetData<DbUser>();
            Debug.Assert(sysUser2 != null && sysUser2.Account == "admin1");
            sysUser2.Account = "admin2";
            CallContext.SetData(sysUser2);
        });

        //await Task.WhenAll(task1, task2);

        await task1;

        await task2;

        return CallContext.GetData<DbUser>();
    }

    /// <summary>
    /// TestContextAndTempFile
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<DbUser> TestTempFile()
    {
        var sysUser1 = new DbUser()
        {
            Id = 12321,
            NickName = "Walle",
            Account = "superadmin",
            RealName = "Walle",
        };

        TempFile? tf = null;
        DbUser? sysUser2 = null;

        var task1 = Task.Run(async () =>
        {
            await Task.Delay(500);
            tf = await sysUser1.ToTempFileAsync();
        });

        var task2 = Task.Run(async () =>
        {
            await Task.Delay(1500);
            sysUser2 = await tf!.ToObjectAsync<DbUser>();
            tf!.Dispose();
        });

        await Task.WhenAll(task1, task2);

        return sysUser2!;
    }

    /// <summary>
    /// 不加谓词试试看
    /// </summary>
    /// <returns></returns>
    public async Task<bool> NoMethodTest()
    {
        return await Task.FromResult(true);
    }

    /// <summary>
    /// 测试空值
    /// </summary>
    /// <returns></returns>
    public async Task<DbUser> NullTest()
    {
        return await Task.FromResult<DbUser>(null!);
    }

    /// <summary>
    /// 测试数组1
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<StringArray> TestArray1(StringArray input)
    {
        return await Task.FromResult(input);
    }

    /// <summary>
    /// 测试数组2
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<StringArray> TestArray2([FromBody] StringArray input)
    {
        return await Task.FromResult(input);
    }

    /// <summary>
    /// 测试数组3
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<List<string>> TestArray3(List<string> input)
    {
        return await Task.FromResult(input);
    }

    /// <summary>
    /// 测试数组3
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<List<string>> TestArray4([FromBody] List<string> input)
    {
        return await Task.FromResult(input);
    }

    /// <summary>
    /// 测试httpclient1
    /// </summary>
    /// <returns></returns>
    public async Task<string> HttpClientTest1Async()
    {
        var httpClientProxy = HttpClientProxy.Create("https://www.baidu.com");
        return await httpClientProxy.GetAsync("/");
    }


    /// <summary>
    /// 测试redis队列
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<bool> EnqueueAsync(string topic, string val)
    {
        return await LuBanRedis.Instance.GetDatabase().EnqueueAsync(topic, val);
    }

    /// <summary>
    /// 测试redis队列
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<string?> DequeueAsync(string topic)
    {
        return await LuBanRedis.Instance.GetDatabase().DequeueAsync(topic);
    }

    /// <summary>
    /// 测试redis分布式锁1
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [DistributedLock("TestDistributedLock1")]
    public async Task<string> TestDistributedLock1Async()
    {
        return await Task.FromResult("TestDistributedLock1");
    }


    /// <summary>
    /// 测试redis分布式锁2
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<string> TestDistributedLock2Async()
    {
        using var dl = new DistributedLock("TestDistributedLock2");
        return await Task.FromResult("TestDistributedLock2");
    }

    /// <summary>
    /// 测试接口添加缓存
    /// </summary>
    /// <returns></returns>
    [HttpGet, Cacheable("test")]
    public async Task<string> GetFromCache()
    {
        return await Task.FromResult(GuidUtil.New);
    }

    /// <summary>
    /// 测试清除接口缓存
    /// </summary>
    /// <returns></returns>
    [HttpGet, CacheClear("test")]
    public async Task<bool> ClearCahce()
    {
        return await Task.FromResult(true);
    }

}

/// <summary>
/// PageInput
/// </summary>
public class PageInput : BasePageInput
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "为啥不填")]
    public string Key
    {
        get; set;
    }
}

//[SugarTable(tableName: "test")]
public class StringArray
{
    //[SugarColumn]
    public long Id { get; set; }

    //[SugarColumn]
    public string[]? Items { get; set; }

    //[SugarColumn]
    public List<string> Item2 { get; set; }
}