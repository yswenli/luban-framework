/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： SimpleThreadPoolUnitTest
*版本号： V1.0.0.0
*唯一标识：8525e274-d733-4907-9f52-1bf9c18a4758
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/5/21 10:35:01
*描述：
*
*=================================================
*修改标记
*修改时间：2025/5/21 10:35:01
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using LuBan.Threading;

namespace LuBan.UnitTestProject
{
    /// <summary>
    /// 线程池单地测试
    /// </summary>
    [TestClass]
    public class SimpleThreadPoolUnitTest
    {
        [TestMethod]
        public void Test1()
        {
            var pool = new SimpleThreadPool("test", 100);

            ThreadUtil.ThreadWhile(() =>
            {
                pool.Enqueue(() =>
                {
                    Thread.Sleep(100);
                });
            }, 10);

            Assert.IsTrue(true);
        }
    }
}
