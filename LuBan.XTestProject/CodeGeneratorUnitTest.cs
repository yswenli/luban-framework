/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： CodeGeneratorUnitTest
*版本号： V1.0.0.0
*唯一标识：2cc4033a-b9b5-4316-be64-270bccad1d46
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/1/15 16:00:25
*描述：orm 代码生成单元测试
*
*=================================================
*修改标记
*修改时间：2025/1/15 16:00:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：orm 代码生成单元测试
*
*****************************************************************************/
using LuBan.Orm;
using LuBan.Orm.Entities;
using LuBan.Orm.Generating;

namespace LuBan.UnitTestProject;


/// <summary>
/// orm 代码生成单元测试
/// </summary>
[TestClass]
public class CodeGeneratorUnitTest
{
    /// <summary>
    /// 初始化
    /// </summary>
    [TestInitialize]
    public void Init()
    {
        LuBanOrm.Init();
    }

    /// <summary>
    /// 递归获取菜单数据
    /// </summary>
    /// <param name="repository">菜单仓储</param>
    /// <param name="parentId">父节点ID</param>
    /// <param name="maxDepth">最大递归深度</param>
    /// <param name="currentDepth">当前递归深度</param>
    /// <returns>菜单列表</returns>
    private List<DbMenu> GetMenuDataRecursively(BaseRepository<DbMenu> repository, long parentId, int maxDepth, int currentDepth = 0)
    {
        if (currentDepth >= maxDepth)
        {
            return new List<DbMenu>();
        }

        var nodes = repository.Where(q => q.IsDelete == false && q.Status == Orm.Enums.EnumEnableStatus.Enable && q.Pid == parentId)
                              .OrderBy(q => q.OrderNo)
                              .ToList();

        var result = new List<DbMenu>();
        foreach (var node in nodes)
        {
            result.Add(node);
            var children = GetMenuDataRecursively(repository, node.Id, maxDepth, currentDepth + 1);
            result.AddRange(children);
        }

        return result;
    }

    /// <summary>
    /// 测试生成种子数据
    /// </summary>
    [TestMethod]
    public void TestGetDbConfigSeedsCode()
    {
        var menuCode = CodeGenerator.GetSeedDataCSCode<DbConfig>(q => q.IsDelete == false);
        Assert.IsNotNull(menuCode);
    }

    /// <summary>
    /// 测试方法1（使用递归）
    /// </summary>
    [TestMethod]
    public void TestMethod1()
    {
        // 初始化菜单仓储
        var menuResp = new BaseRepository<DbMenu>();

        // 设置最大递归深度
        int maxDepth = 3;

        // 获取菜单数据
        var targetList = GetMenuDataRecursively(menuResp, 0, maxDepth);

        // 验证生成的种子数据
        var menuCode = CodeGenerator.GetSeedDataCSCode(targetList);
        Assert.IsNotNull(menuCode);
    }


    /// <summary>
    /// 测试方法2
    /// </summary>
    [TestMethod]
    public void TestMethod2()
    {
        var files = CodeGenerator.GetEntityCSCodeFilePathBySqlSugar();

        Assert.IsNotNull(files);
    }


    /// <summary>
    /// 测试方法3
    /// </summary>
    [TestMethod]
    public void TestMethod3()
    {
        var files = CodeGenerator.GetEntityCSCodeFilePath();

        Assert.IsNotNull(files);
    }

    /// <summary>
    /// 测试方法4
    /// </summary>
    [TestMethod]
    public void TestMethod4()
    {
        var code = CodeGenerator.GetEntityCSCode("sys_dict_data");

        Assert.IsNotNull(code);
    }

    [TestMethod]
    public void TestMehtod5()
    {
        var code = CodeGenerator.GetEntityCSCode("db_region");

        code = CodeGenerator.GetSeedDataCSCode<DbRegion>(q => q.IsDelete == false);

        Assert.IsNotNull(code);
    }


    [TestMethod]
    public void TestMehtod6()
    {
        var code = CodeGenerator.GetEntityCSCode("db_pos");

        code = CodeGenerator.GetSeedDataCSCode<DbPos>(q => q.IsDelete == false);

        Assert.IsNotNull(code);
    }
}
