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
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： LogLineReaderUnitTest
*版本号： V1.0.0.0
*唯一标识：793bfd8e-384e-420e-882f-789c9c01b479
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9 17:24:56
*描述：日志行读取器单元测试（BOM、超长行、多字节与换行边界）
*
*=================================================
*修改标记
*修改时间：2026/10/9 17:24:56
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志行读取器单元测试（BOM、超长行、多字节与换行边界）
*
*****************************************************************************/

using System.Text;
using LuBan.AIAgent.LogAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class LogLineReaderUnitTest
{
    private static async Task<List<LogLine>> ReadAllAsync(string path, int maxLineBytes)
    {
        var lines = new List<LogLine>();
        var reader = new LogLineReader(path, maxLineBytes);
        await foreach (var line in reader.ReadAsync())
            lines.Add(line);
        return lines;
    }

    [TestMethod]
    public async Task Reads_Lines_Numbers_And_Strips_CrLf()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "a\r\nb\nc");
            var lines = await ReadAllAsync(path, 65536);

            Assert.AreEqual(3, lines.Count);
            Assert.AreEqual(1L, lines[0].LineNumber);
            Assert.AreEqual("a", lines[0].Text);
            Assert.AreEqual(2L, lines[1].LineNumber);
            Assert.AreEqual("b", lines[1].Text);
            Assert.AreEqual(3L, lines[2].LineNumber);
            Assert.AreEqual("c", lines[2].Text);
            Assert.IsFalse(lines[2].Truncated);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Skips_Utf8_Bom()
    {
        var path = Path.GetTempFileName();
        try
        {
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("hello\n")).ToArray();
            await File.WriteAllBytesAsync(path, bytes);
            var lines = await ReadAllAsync(path, 65536);

            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual("hello", lines[0].Text);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Keeps_Empty_Lines_And_Trailing_Newline_Adds_No_Extra_Line()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "a\n\nb\n");
            var lines = await ReadAllAsync(path, 65536);

            Assert.AreEqual(3, lines.Count);
            Assert.AreEqual("a", lines[0].Text);
            Assert.AreEqual(string.Empty, lines[1].Text);
            Assert.AreEqual("b", lines[2].Text);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Truncates_Long_Line_But_Keeps_Scanning()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, new string('x', 100) + "\nok\n");
            var lines = await ReadAllAsync(path, 10);

            Assert.AreEqual(2, lines.Count);
            Assert.AreEqual(10, lines[0].Text.Length);
            Assert.IsTrue(lines[0].Truncated);
            Assert.AreEqual("ok", lines[1].Text);
            Assert.IsFalse(lines[1].Truncated);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Multibyte_Utf8_Is_Not_Split_By_Newline_Boundary()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "中文日志\n第二行\n");
            var lines = await ReadAllAsync(path, 65536);

            Assert.AreEqual(2, lines.Count);
            Assert.AreEqual("中文日志", lines[0].Text);
            Assert.AreEqual("第二行", lines[1].Text);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Crlf_Split_Across_Block_Boundary_Is_Handled()
    {
        var path = Path.GetTempFileName();
        try
        {
            var bytes = new byte[65535 + 2 + 3];
            Array.Fill(bytes, (byte)'x', 0, 65535);
            bytes[65535] = (byte)'\r';
            bytes[65536] = (byte)'\n';
            bytes[65537] = (byte)'o';
            bytes[65538] = (byte)'k';
            bytes[65539] = (byte)'\n';
            await File.WriteAllBytesAsync(path, bytes);
            var lines = await ReadAllAsync(path, 131072);

            Assert.AreEqual(2, lines.Count);
            Assert.AreEqual(new string('x', 65535), lines[0].Text);
            Assert.IsFalse(lines[0].Truncated);
            Assert.AreEqual("ok", lines[1].Text);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Line_Exactly_At_Cap_Is_Not_Truncated()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, new string('x', 10) + "\n");
            var lines = await ReadAllAsync(path, 10);

            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual(10, lines[0].Text.Length);
            Assert.IsFalse(lines[0].Truncated);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Invalid_Utf8_Byte_Becomes_Replacement_Char()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(path, new byte[] { 0xFF, (byte)'\n' });
            var lines = await ReadAllAsync(path, 65536);

            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual("\uFFFD", lines[0].Text);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Bom_With_Short_No_Newline_Content()
    {
        var path = Path.GetTempFileName();
        try
        {
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("hi")).ToArray();
            await File.WriteAllBytesAsync(path, bytes);
            var lines = await ReadAllAsync(path, 65536);

            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual("hi", lines[0].Text);
        }
        finally { File.Delete(path); }
    }
}