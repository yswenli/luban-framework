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
