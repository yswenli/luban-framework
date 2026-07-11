/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： MapsterUnitTest
*版本号： V1.0.0.0
*唯一标识：b0377160-4b59-4ecf-b9fb-c8111b57127e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/4 14:09:40
*描述：
*
*=================================================
*修改标记
*修改时间：2024/12/4 14:09:40
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using LuBan.Orm;
using LuBan.XTestProject.Models;

using Mapster;

namespace LuBan.XTestProject
{
    [TestClass]
    public class MapsterUnitTest
    {
        [TestMethod]
        public void MapsterTest()
        {
            TestInfo testInfo = new()
            {
                Name = "yswenli"
            };

            var data1 = testInfo.Adapt<TestInfo2>();
            Assert.AreEqual(data1.Name, testInfo.Name);

            var data2 = new TestInfo()
            {
                Description = "yswenli2"
            };
            data2 = data2.FillFrom(testInfo);
            Assert.AreNotEqual(data2.Description, testInfo.Description);
        }
    }

    public class TestInfo2
    {
        public string Name { get; set; }
    }
}
