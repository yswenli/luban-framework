/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： ModelUtilUnitTest
*版本号： V1.0.0.0
*唯一标识：47dc4cbf-58d8-4877-9d64-baf4267dff32
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/6 16:59:04
*描述：
*
*=================================================
*修改标记
*修改时间：2025/8/6 16:59:04
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using LuBan.Common;

using Newtonsoft.Json;

namespace LuBan.UnitTestProject;


[TestClass]
public class ModelUtilUnitTest
{

    /// <summary>
    /// 测试从实体类生成url参数
    /// </summary>
    [TestMethod]
    public void TestMehtod1()
    {
        var model = new Model1()
        {
            PageIndex = 1,
            PageSize = 10,
            Key = "癫痫（部分性发作）",
            Test1 = "1",
            Test2 = 1
        };

        var query = model.ToUrlQueryText();

        Assert.IsTrue(query == "?pageNo=1&pageSize=10&keyword=key&test2=1");
    }

    /// <summary>
    /// 测试从实体类生成url参数
    /// </summary>
    [TestMethod]
    public void TestMehtod2()
    {
        var model = new
        {
            PageIndex = 1,
            PageSize = 10,
            Key = "key",
            Test1 = "a",
            Test2 = 1
        };

        var query = model.GetUrlQueryTextByObj();

        Assert.IsTrue(query == "?pageindex=1&pagesize=10&key=key&test1=a&test2=1");
    }

}


public class Model1
{
    // <summary>
    /// 页码
    /// </summary>
    [JsonProperty("pageNo")]
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页数量
    /// </summary>
    [JsonProperty("pageSize")]
    public int PageSize { get; set; } = 100;

    /// <summary>
    /// 搜索关键字
    /// </summary>
    [JsonProperty("keyword")]
    public string Key { get; set; }

    [JsonIgnore]
    public string Test1 { get; set; }

    public int Test2 { get; set; }
}
