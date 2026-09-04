/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Services.JobServices
*文件名： TestJobService.cs
*版本号： V1.0.0.0
*唯一标识：7c7048d5-7d88-4c0b-8085-d6bbab8c5246
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：TestJobService 服务类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：TestJobService 服务类
*
*****************************************************************************/

using LuBan.Service;

using WebApplication1.Services.ApiServices;

namespace WebApplication1.Services.JobServices;


public abstract class ClassA : BaseJobService
{
    public ClassA(int intervalTime = 60 * 1000, bool sequentially = true) : base(intervalTime, sequentially)
    {

    }
}

public abstract class ClassB : ClassA
{
    public ClassB(int intervalTime = 60 * 1000, bool sequentially = true) : base(intervalTime, sequentially)
    {

    }
}

public abstract class ClassC : ClassA
{
    public ClassC(int intervalTime = 60 * 1000, bool sequentially = true) : base(intervalTime, sequentially)
    {

    }
}

/// <summary>
/// test
/// </summary>
[JobInfo<TestAsyncService>("test")]
public class TestJobService : ClassC
{
    /// <summary>
    /// test
    /// </summary>
    public TestJobService() : base(3000)
    {

    }


    /// <summary>
    /// test
    /// </summary>
    public override void Run()
    {
    }
}
