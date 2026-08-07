using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
using LuBan.AIAgent.Tools.Script;

namespace LuBan.AIAgent.Tests.Tools;

/// <summary>
/// 测试 ScriptToolGroup 的 RunShellAsync 方法
/// </summary>
[TestClass]
public class ScriptToolGroupTests
{
    private class MockConfirmationService : IToolConfirmationService
    {
        public bool RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments) => true;
        public bool TryConfirmByPath(string toolName, string path, IReadOnlyDictionary<string, object?> arguments) => true;
        public bool RequiresConfirmation(string toolName) => false;
        public string FormatArguments(IReadOnlyDictionary<string, object?> arguments, int maxLength = 200) => "";
    }

    [TestMethod]
    public async Task RunShellAsync_PythonVersion_ReturnsOutput()
    {
        // Arrange
        var confirmationService = new MockConfirmationService();
        var options = new ScriptToolOptions
        {
            Shell = "cmd",
            DefaultTimeout = 10000
        };
        var processRunner = new ProcessRunner();
        var toolGroup = new ScriptToolGroup(options, processRunner, confirmationService);

        // Act
        var result = await toolGroup.RunShellAsync("python --version");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        var data = System.Text.Json.JsonDocument.Parse(result.Data!);
        var exitCode = data.RootElement.GetProperty("exitCode").GetInt32();
        var stdout = data.RootElement.GetProperty("stdout").GetString();
        var stderr = data.RootElement.GetProperty("stderr").GetString();

        Assert.AreEqual(0, exitCode, $"ExitCode 应为 0，实际: {exitCode}");
        Assert.IsTrue(!string.IsNullOrEmpty(stdout) || !string.IsNullOrEmpty(stderr), 
            $"应捕获输出，stdout: '{stdout}', stderr: '{stderr}'");
    }

    [TestMethod]
    public async Task RunShellAsync_EchoCommand_ReturnsOutput()
    {
        // Arrange
        var confirmationService = new MockConfirmationService();
        var options = new ScriptToolOptions
        {
            Shell = "cmd",
            DefaultTimeout = 10000
        };
        var processRunner = new ProcessRunner();
        var toolGroup = new ScriptToolGroup(options, processRunner, confirmationService);

        // Act
        var result = await toolGroup.RunShellAsync("echo hello world");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        var data = System.Text.Json.JsonDocument.Parse(result.Data!);
        var exitCode = data.RootElement.GetProperty("exitCode").GetInt32();
        var stdout = data.RootElement.GetProperty("stdout").GetString();
        var stderr = data.RootElement.GetProperty("stderr").GetString();

        Assert.AreEqual(0, exitCode, $"ExitCode 应为 0，实际: {exitCode}");
        Assert.IsTrue(stdout!.Contains("hello"), 
            $"应捕获输出 'hello'，stdout: '{stdout}', stderr: '{stderr}'");
    }
}
