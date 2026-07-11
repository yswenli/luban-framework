/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： ObjectAsyncLockUnitTest
*版本号： V1.0.0.0
*唯一标识：a3813ac9-bcf3-4ebd-b559-74a27f43ddc7
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/1/9 11:52:09
*描述：测试异步锁
*
*=================================================
*修改标记
*修改时间：2025/1/9 11:52:09
*修改人： yswenli
*版本号： V1.0.0.0
*描述：测试异步锁
*
*****************************************************************************/
using LuBan.Threading;

namespace LuBan.UnitTestProject;

/// <summary>
/// 测试异步锁
/// </summary>
[TestClass]
public class ObjectAsyncLockUnitTest
{


    /// <summary>
    /// 测试异步锁
    /// </summary>
    [TestMethod]
    public async Task TestMethod1()
    {
        int i = 0;
        var taskList = new List<Task>();
        for (int j = 0; j < 10000; j++)
        {
            taskList.Add(Task.Run(() =>
            {
                Interlocked.Increment(ref i);
            }));
        }

        Task.WaitAll([.. taskList]);
        Assert.AreEqual(10000, i);


        i = 0;
        taskList.Clear();
        for (int j = 0; j < 10000; j++)
        {
            taskList.Add(Task.Run(async () =>
            {
                using var locker = await LockerBuilder.Default.CreateAsync("yswenli");
                i = i + 1;
            }));
        }

        Task.WaitAll([.. taskList]);
        Assert.AreEqual(10000, i);
    }
}
