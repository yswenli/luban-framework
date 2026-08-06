using LuBan.AIAgent.Infrastructure;
using LuBan.AIAgent.Tools.Script;

namespace LuBan.AIAgent.Tests.Tools;

/// <summary>
/// 测试 ProcessRunner 的 stdout/stderr 捕获行为
/// </summary>
[TestClass]
public class ProcessRunnerTests
{
    [TestMethod]
    public async Task RunAsync_PythonVersion_CapturesOutput()
    {
        // Arrange
        var runner = new ProcessRunner();
        
        // Act - 执行 python --version
        var result = await runner.RunAsync(
            "python",
            "--version",
            timeoutMs: 10000);

        // Assert
        Assert.AreEqual(0, result.ExitCode, $"ExitCode 应为 0，实际: {result.ExitCode}");
        Assert.IsTrue(result.StandardOutput.Length > 0 || result.StandardError.Length > 0, 
            $"应捕获输出，stdout: '{result.StandardOutput}', stderr: '{result.StandardError}'");
    }

    [TestMethod]
    public async Task RunAsync_PythonStdinScript_CapturesOutput()
    {
        // Arrange
        var runner = new ProcessRunner();
        var script = "print('hello from python')";
        
        // Act - 通过 stdin 执行脚本
        var result = await runner.RunAsync(
            "python",
            "-",  // 从 stdin 读取
            stdin: script,
            timeoutMs: 10000);

        // Assert
        Assert.AreEqual(0, result.ExitCode, $"ExitCode 应为 0，实际: {result.ExitCode}");
        Assert.IsTrue(result.StandardOutput.Contains("hello"), 
            $"应捕获输出，stdout: '{result.StandardOutput}', stderr: '{result.StandardError}'");
    }

    [TestMethod]
    public async Task RunAsync_EchoCommand_CapturesOutput()
    {
        // Arrange
        var runner = new ProcessRunner();
        
        // Act - 执行 echo 命令（Windows）
        var result = await runner.RunAsync(
            "cmd",
            "/c echo hello world",
            timeoutMs: 10000);

        // Assert
        Assert.AreEqual(0, result.ExitCode);
        Assert.IsTrue(result.StandardOutput.Contains("hello"), 
            $"应捕获输出，stdout: '{result.StandardOutput}', stderr: '{result.StandardError}'");
    }
}
