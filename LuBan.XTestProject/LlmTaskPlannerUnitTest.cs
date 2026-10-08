/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： LlmTaskPlannerUnitTest
*唯一标识：LlmTaskPlanner 并行度 clamp 单测
*创建时间：2026/10/8
*描述：验证图级并行度 clamp 到 [1, 硬上限] 的边界语义（纯单元，不访问网络）
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration.Planner;

namespace LuBan.XTestProject;

[TestClass]
public class LlmTaskPlannerUnitTest
{
    [TestMethod]
    public void ClampParallelism_Null_ReturnsNull()
    {
        var options = new OrchestrationOptions { MaxParallelism = 5, MaxNodes = 10 };

        Assert.IsNull(LlmTaskPlanner.ClampParallelism(null, options));
    }

    [TestMethod]
    public void ClampParallelism_WithinRange_Unchanged()
    {
        var options = new OrchestrationOptions { MaxParallelism = 5, MaxNodes = 10 };

        Assert.AreEqual(3, LlmTaskPlanner.ClampParallelism(3, options));
    }

    [TestMethod]
    public void ClampParallelism_AboveHardLimit_ClampedToMaxParallelism()
    {
        var options = new OrchestrationOptions { MaxParallelism = 5, MaxNodes = 20 };

        Assert.AreEqual(5, LlmTaskPlanner.ClampParallelism(12, options));
    }

    [TestMethod]
    public void ClampParallelism_BelowOne_ClampedToOne()
    {
        var options = new OrchestrationOptions { MaxParallelism = 5, MaxNodes = 20 };

        Assert.AreEqual(1, LlmTaskPlanner.ClampParallelism(0, options));
        Assert.AreEqual(1, LlmTaskPlanner.ClampParallelism(-3, options));
    }

    [TestMethod]
    public void ClampParallelism_Unlimited_FallsBackToMaxNodes()
    {
        var options = new OrchestrationOptions { MaxParallelism = 0, MaxNodes = 10 };

        Assert.AreEqual(10, LlmTaskPlanner.ClampParallelism(100, options));
        Assert.AreEqual(3, LlmTaskPlanner.ClampParallelism(3, options));
    }
}
