/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： RegionSeedUnitTest
*版本号： V1.0.0.0
*唯一标识：f5ec0e73-e26d-4bc4-8430-158a93e94b1d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/5 15:58:39
*描述：区域种子单元测试
*
*=================================================
*修改标记
*修改时间：2024/9/5 15:58:39
*修改人： yswenli
*版本号： V1.0.0.0
*描述：区域种子单元测试
*
*****************************************************************************/
using LuBan.Orm;
using LuBan.Orm.Entities;

namespace LuBan.XTestProject;

/// <summary>
/// 区域种子单元测试
/// </summary>
[TestClass]
public class RegionSeedUnitTest
{

    BaseRepository<DbRegion> _regionSeedRepository;

    /// <summary>
    /// 测试方法1
    /// </summary>
    [TestMethod]
    public void TestMethod1()
    {
        _regionSeedRepository = new BaseRepository<DbRegion>();

        var list = _regionSeedRepository.GetList();

        var sp = new StringPlus();

        foreach (var item in list)
        {
            sp.AppendLine($"");
        }
    }

}
