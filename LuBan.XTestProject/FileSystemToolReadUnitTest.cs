/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__ _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： FileSystemToolReadUnitTest
*唯一标识：文件读取分页/编码/二进制守卫/表格读取单测
*创建时间：2026/11/6
*描述：验证 ReadFile 分页与编码回写、二进制守卫、ReadSpreadsheet、Grep 跳过提示
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
using LuBan.AIAgent.Tools.FileSystem;
using Microsoft.Extensions.Options;
using MiniExcelLibs;

namespace LuBan.XTestProject;

/// <summary>
/// 文件读取相关行为单测：分页、编码、二进制守卫、表格读取、Grep 跳过提示。
/// </summary>
[TestClass]
public class FileSystemToolReadUnitTest
{
    private static string _root = null!;
    private static FileSystemToolGroup _group = null!;

    [ClassInitialize]
    public static void Setup(TestContext context)
    {
        _root = Path.Combine(Path.GetTempPath(), "luban-fs-read-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(_root);

            var options = Options.Create(new LuBanAgentOptions
            {
                Tools = new ToolGroupOptions
                {
                    FileSystem = new FileSystemToolOptions
                    {
                        Enabled = true,
                        AllowedRoots = new List<string> { _root }
                    }
                }
            });

            var confirmationContext = new ToolConfirmationContext
            {
                Callback = (_, _) => Task.FromResult(ConfirmResult.Allow),
                WorkspacePathChecker = _ => true
            };

            _group = new FileSystemToolGroup(new ToolConfirmationService(confirmationContext, options));
        }
        catch
        {
            TryDeleteRoot();
            throw;
        }
    }

    [ClassCleanup]
    public static void Cleanup() => TryDeleteRoot();

    private static void TryDeleteRoot()
    {
        try
        {
            if (!string.IsNullOrEmpty(_root) && Directory.Exists(_root))
                Directory.Delete(_root, true);
        }
        catch { }
    }

    private static string NewFile(string name, string content)
    {
        var path = Path.Combine(_root, name);
        File.WriteAllText(path, content);
        return path;
    }

    [TestMethod]
    public async Task ReadFileAsync_OffsetLimit_ReturnsRequestedPageWithContinuation()
    {
        var lines = string.Join(Environment.NewLine, Enumerable.Range(1, 10).Select(i => $"line-{i}"));
        var path = NewFile("page.txt", lines);

        var result = await _group.ReadFileAsync(path, offset: 3, limit: 4);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.Data!.Contains("line-2"), "分页不应返回 offset 之前的行");
        StringAssert.Contains(result.Data, "line-3");
        StringAssert.Contains(result.Data, "line-6");
        Assert.IsFalse(result.Data.Contains("line-7"), "limit 之外的行不应返回");
        StringAssert.Contains(result.Data, "offset=7");
    }

    [TestMethod]
    public async Task ReadFileAsync_OffsetBeyondEnd_ReturnsActionableMessage()
    {
        var path = NewFile("short.txt", "only-line");

        var result = await _group.ReadFileAsync(path, offset: 100);

        Assert.IsTrue(result.IsSuccess);
        StringAssert.Contains(result.Data!, "超出文件末尾");
    }

    [TestMethod]
    public async Task ReadFileAsync_SpreadsheetFile_GuidesToReadSpreadsheet()
    {
        var path = Path.Combine(_root, "data.xlsx");
        File.WriteAllBytes(path, [0x50, 0x4B, 0x03, 0x04]);

        var result = await _group.ReadFileAsync(path);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message, "ReadSpreadsheet");
    }

    [TestMethod]
    public async Task ReadFileAsync_NulHeader_DetectedAsBinary()
    {
        var path = Path.Combine(_root, "opaque.dat2");
        File.WriteAllBytes(path, [0x41, 0x00, 0x42, 0x00]);

        var result = await _group.ReadFileAsync(path);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message, "二进制文件");
    }

    [TestMethod]
    public async Task ReadFileAsync_GbkFile_DecodesChineseAndKeepsEncodingOnEdit()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var gbk = Encoding.GetEncoding(936);

        var path = Path.Combine(_root, "gbk.txt");
        File.WriteAllBytes(path, gbk.GetBytes("第一行中文" + Environment.NewLine + "第二行中文"));

        var read = await _group.ReadFileAsync(path);
        Assert.IsTrue(read.IsSuccess);
        StringAssert.Contains(read.Data!, "第一行中文");

        var edit = await _group.EditFileAsync(path, "第二行中文", "改过的中文");
        Assert.IsTrue(edit.IsSuccess, edit.Message);

        // 回写必须仍为 GBK：按 UTF-8 解读会出现替换字符
        var bytes = File.ReadAllBytes(path);
        var decoded = gbk.GetString(bytes);
        StringAssert.Contains(decoded, "改过的中文");
        Assert.IsFalse(bytes.AsSpan().IndexOf((byte)'?') >= 0 && decoded.Contains('\uFFFD'), "回写不应损坏编码");
    }

    [TestMethod]
    public async Task ReadSpreadsheetAsync_Csv_ReturnsMarkdownTable()
    {
        var path = NewFile("table.csv", "name,age" + Environment.NewLine + "张三,30" + Environment.NewLine + "李四,25");

        var result = await _group.ReadSpreadsheetAsync(path);

        Assert.IsTrue(result.IsSuccess, result.Message);
        StringAssert.Contains(result.Data!, "| name | age |");
        StringAssert.Contains(result.Data, "张三");
        StringAssert.Contains(result.Data, "共 2 行数据");
    }

    [TestMethod]
    public async Task ReadSpreadsheetAsync_Xlsx_SelectSheetAndPage()
    {
        var path = Path.Combine(_root, "book.xlsx");
        var rows = Enumerable.Range(1, 30)
            .Select(i => new Dictionary<string, object> { ["名称"] = $"项目{i}", ["值"] = i })
            .ToList();
        MiniExcel.SaveAs(path, rows, sheetName: "明细", overwriteFile: true);

        var page = await _group.ReadSpreadsheetAsync(path, sheetName: "明细", offset: 11, limit: 5);
        Assert.IsTrue(page.IsSuccess, page.Message);
        StringAssert.Contains(page.Data!, "项目11");
        Assert.IsFalse(page.Data.Contains("项目1 "), "offset 之前的数据不应返回");

        var missing = await _group.ReadSpreadsheetAsync(path, sheetName: "不存在的表");
        Assert.IsFalse(missing.IsSuccess);
        StringAssert.Contains(missing.Message, "明细");
    }

    [TestMethod]
    public async Task ReadSpreadsheetAsync_LegacyXls_ReturnsSaveAsHint()
    {
        var path = Path.Combine(_root, "legacy.xls");
        File.WriteAllBytes(path, [0xD0, 0xCF, 0x11, 0xE0]);

        var result = await _group.ReadSpreadsheetAsync(path);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message, "另存为");
    }

    [TestMethod]
    public async Task GrepAsync_SkippedFiles_AreReported()
    {
        NewFile("match.txt", "needle here");
        File.WriteAllBytes(Path.Combine(_root, "blob.bin"), [0x00, 0x01, 0x02]);

        var result = await _group.GrepAsync(_root, "needle");

        Assert.IsTrue(result.IsSuccess);
        StringAssert.Contains(result.Data!, "match.txt");
        StringAssert.Contains(result.Data, "未检索");
        StringAssert.Contains(result.Data, "二进制文件");
    }

    [TestMethod]
    public async Task ListDirectoryAsync_ManyEntries_ReportsCap()
    {
        var dir = Path.Combine(_root, "many");
        Directory.CreateDirectory(dir);
        for (var i = 0; i < 520; i++)
            File.WriteAllText(Path.Combine(dir, $"f{i:D4}.txt"), "");

        var result = await _group.ListDirectoryAsync(dir);

        Assert.IsTrue(result.IsSuccess);
        StringAssert.Contains(result.Data!, "上限");
    }
}