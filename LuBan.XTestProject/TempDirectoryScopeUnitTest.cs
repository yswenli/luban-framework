/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： TempDirectoryScopeUnitTest
*创建时间：2026/10/10
*描述：验证 TempDirectory 支持按作用域 Push 覆盖并可恢复
*
*****************************************************************************/
using LuBan.Common.IO;

namespace LuBan.XTestProject;

[TestClass]
public class TempDirectoryScopeUnitTest
{
    [TestMethod]
    public void Push_Overrides_And_Restores()
    {
        var a = Path.Combine(Path.GetTempPath(), "luban-a");
        var b = Path.Combine(Path.GetTempPath(), "luban-b");
        using (TempDirectory.Push(new StaticTempDirectoryProvider(a)))
        {
            Assert.AreEqual(Path.GetFullPath(a), Path.GetFullPath(TempDirectory.GetTempDir()));
            using (TempDirectory.Push(new StaticTempDirectoryProvider(b)))
                Assert.AreEqual(Path.GetFullPath(b), Path.GetFullPath(TempDirectory.GetTempDir()));
            Assert.AreEqual(Path.GetFullPath(a), Path.GetFullPath(TempDirectory.GetTempDir()));
        }
    }

    [TestMethod]
    public void Default_Falls_Back_To_System_Temp()
    {
        Assert.AreEqual(
            Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(TempDirectory.GetTempDir()).TrimEnd(Path.DirectorySeparatorChar));
    }
}