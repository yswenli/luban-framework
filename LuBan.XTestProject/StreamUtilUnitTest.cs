using System.Text;

namespace LuBan.Common.Tests;

[TestClass]
public class StreamUtilUnitTest
{
    static MemoryStream CreateStream(string content)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    [TestMethod]
    public async Task ReadToEndAsync_SmallContent_ReadsAll()
    {
        var content = "hello, 鲁班 framework!";
        using var ms = CreateStream(content);
        var result = await ms.ReadToEndAsync(Encoding.UTF8);
        Assert.AreEqual(content, result);
    }

    [TestMethod]
    public async Task ReadToEndAsync_LargeContent_StopsAtMaxLength()
    {
        //构造1MB内容，maxLength=1024时只应返回前1024个字符，不再读取剩余流
        var content = new string('a', 1024 * 1024);
        using var ms = CreateStream(content);
        var result = await ms.ReadToEndAsync(Encoding.UTF8, 1024);
        Assert.AreEqual(1024, result.Length);
        Assert.AreEqual(content[..1024], result);
        //底层流未被排空（StreamReader有4KB缓冲预读，阈值取64K）
        Assert.IsTrue(ms.Position <= 64 * 1024, $"流被过度读取，Position={ms.Position}");
    }

    [TestMethod]
    public async Task ReadToEndAsync_MaxLengthCountsCharsNotBytes()
    {
        //UTF-8中文每字3字节，maxLength按字符数截断且不得出现乱码
        var content = new string('鲁', 5000);
        using var ms = CreateStream(content);
        var result = await ms.ReadToEndAsync(Encoding.UTF8, 100);
        Assert.AreEqual(100, result.Length);
        Assert.AreEqual(new string('鲁', 100), result);
    }

    [TestMethod]
    public async Task ReadToEndAsync_ZeroMaxLength_ReadsAll()
    {
        var content = new string('b', 100 * 1024);
        using var ms = CreateStream(content);
        var result = await ms.ReadToEndAsync(Encoding.UTF8, 0);
        Assert.AreEqual(content, result);
    }

    [TestMethod]
    public async Task ReadToEndAsync_LeaveOpen_ResetsPosition()
    {
        var content = "position test";
        using var ms = CreateStream(content);
        await ms.ReadToEndAsync(Encoding.UTF8, leaveOpen: true);
        Assert.AreEqual(0, ms.Position);
    }

    [TestMethod]
    public async Task ReadToEndAsync_NullStream_ReturnsEmpty()
    {
        Stream? stream = null;
        var result = await stream.ReadToEndAsync(Encoding.UTF8);
        Assert.AreEqual(string.Empty, result);
    }
}
