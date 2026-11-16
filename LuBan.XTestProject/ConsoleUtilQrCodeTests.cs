/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Tests
*文件名： ConsoleUtilQrCodeTests.cs
*版本号： V1.0.0.0
*描述：ConsoleUtil 终端二维码的行数、静区与半块字符行宽
*
*=================================================
*修改标记
*修改时间：2026/11/12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ConsoleUtil 终端二维码的行数、静区与半块字符行宽
*
*****************************************************************************/

namespace LuBan.Common.Tests;

/// <summary>
/// 终端二维码渲染单元测试类
/// </summary>
[TestClass]
public class ConsoleUtilQrCodeTests
{
    /// <summary>与真实入口等长的地址：局域网 IP + 端口 + 43 字符令牌。</summary>
    private static readonly string EntryUrl =
        "http://192.168.3.215:18080/?t=0123456789012345678901234567890123456789abc";

    [TestMethod]
    public void 纯字符版每行等宽且只含整块与空格()
    {
        var lines = ConsoleUtil.RenderQrCode(EntryUrl);

        Assert.IsTrue(lines.Count > 0);
        var widths = new HashSet<int>();
        foreach (var line in lines)
        {
            widths.Add(line.Length);
            Assert.IsTrue(line.All(ch => ch == '█' || ch == ' '), "纯字符版只输出整块字符与空格");
            Assert.IsFalse(line.Contains('\u001b'), "纯字符版不能带转义序列，否则会进日志文件");
        }

        Assert.AreEqual(1, widths.Count, "所有行必须等宽");
    }

    [TestMethod]
    public void 四周都留出静区()
    {
        var lines = ConsoleUtil.RenderQrCode(EntryUrl);

        // 静区至少 2 个模块：前两行全空，每行前两列与最后两列为空
        for (var y = 0; y < 2; y++)
        {
            Assert.AreEqual(new string(' ', lines[y].Length), lines[y], $"第 {y} 行应属于静区");
        }

        foreach (var line in lines)
        {
            Assert.AreEqual("  ", line[..2], "左侧静区");
            Assert.AreEqual("  ", line[^2..], "右侧静区");
        }
    }

    [TestMethod]
    public void 半块版每行等宽且宽度等于纯字符版()
    {
        var plainWidth = ConsoleUtil.RenderQrCode(EntryUrl)[0].Length;
        var rows = ConsoleUtil.BuildQrRuns(EntryUrl);

        Assert.IsTrue(rows.Count > 0);
        foreach (var row in rows)
        {
            Assert.AreEqual(plainWidth, row.Sum(run => run.Width), "半块版一行对应纯字符版两行，宽度必须一致");
            Assert.IsTrue(row.All(run => run.Width > 0), "颜色段不能为空");
        }
    }

    [TestMethod]
    public void 半块版行数只有纯字符版的一半()
    {
        var halfBlockRows = ConsoleUtil.BuildQrRuns(EntryUrl).Count;
        var plainRows = ConsoleUtil.RenderQrCode(EntryUrl).Count;

        Assert.IsTrue(halfBlockRows * 2 >= plainRows && halfBlockRows * 2 <= plainRows + 1,
            $"半块字符应把两行并成一行：半块版 {halfBlockRows} 行，纯字符版 {plainRows} 行");
    }

    [TestMethod]
    public void 真实入口的二维码能塞进默认终端窗口()
    {
        // 默认 30 行窗口：二维码本身必须放得下，否则用户还要滚动才能扫全
        var rows = ConsoleUtil.BuildQrRuns(EntryUrl).Count;
        Assert.IsTrue(rows <= 20, $"半块版行数 {rows} 超过 20 行");
    }

    [TestMethod]
    public void 半块版只用黑白两种颜色且每段同色()
    {
        static bool IsInk(ConsoleColor color) => color is ConsoleColor.Black or ConsoleColor.White;

        foreach (var row in ConsoleUtil.BuildQrRuns(EntryUrl))
        {
            foreach (var run in row)
            {
                Assert.IsTrue(IsInk(run.Foreground), $"意外的前景色 {run.Foreground}");
                Assert.IsTrue(IsInk(run.Background), $"意外的背景色 {run.Background}");
            }

            // 相邻段必须换色，否则说明合并规则失效、写出次数会被放大
            for (var i = 1; i < row.Length; i++)
            {
                Assert.IsFalse(
                    row[i - 1].Foreground == row[i].Foreground && row[i - 1].Background == row[i].Background,
                    "相邻同色段应合并");
            }
        }
    }

    [TestMethod]
    public void 相同内容重复渲染结果一致()
    {
        CollectionAssert.AreEqual(
            ConsoleUtil.RenderQrCode(EntryUrl).ToArray(),
            ConsoleUtil.RenderQrCode(EntryUrl).ToArray());
    }

    [TestMethod]
    public void 不同令牌产出不同矩阵()
    {
        var first = ConsoleUtil.RenderQrCode("http://127.0.0.1:18080/?t=aaa");
        var second = ConsoleUtil.RenderQrCode("http://127.0.0.1:18080/?t=bbb");

        Assert.IsFalse(first.SequenceEqual(second), "令牌不同时二维码必须不同，否则扫码会拿到别人的凭据");
    }

    [TestMethod]
    public void 空内容拒绝渲染()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ConsoleUtil.RenderQrCode(string.Empty));
        Assert.ThrowsExactly<ArgumentException>(() => ConsoleUtil.BuildQrRuns("   "));
    }
}