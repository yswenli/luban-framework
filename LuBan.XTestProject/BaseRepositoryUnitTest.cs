/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： BaseRepositoryUnitTest
*版本号： V1.0.0.0
*唯一标识：e1d39a93-c9c9-4cb5-b33a-7f34dc899033
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/4 15:16:13
*描述：基础仓储单元测试
*
*=================================================
*修改标记
*修改时间：2024/12/4 15:16:13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：基础仓储单元测试
*
*****************************************************************************/
using LuBan.Orm;
using LuBan.Orm.Entities;

namespace LuBan.XTestProject
{
    /// <summary>
    /// 基础仓储单元测试
    /// </summary>
    [TestClass]
    public class BaseRepositoryUnitTest
    {
        /// <summary>
        /// 单元测试中，初始化数据库连接
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            LuBanOrm.Init();
        }



        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                var repo = new BaseRepository<DbUser>();
                var list = repo.GetList();
                var first = repo.First(q => q.IsDelete == false);
                var last = repo.Last(q => q.IsDelete == false);
                last.Remark = "test";
                repo.Update(q => new DbUser() { Remark = "Test" }, q => q.Id == last.Id);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [TestMethod]
        public void TestGetProviderReturnsNewInstanceEachCall()
        {
            var provider1 = LuBanOrm.GetProvider<DbUser>(LuBanOrmConst.MainConfigId);
            var provider2 = LuBanOrm.GetProvider<DbUser>(LuBanOrmConst.MainConfigId);
            Assert.AreNotSame(provider1, provider2, "GetProvider should return a new instance each call, not a cached one");
            Assert.AreNotSame(provider1.Provider, provider2.Provider, "SqlSugarScopeProvider should not be shared between calls");
        }
    }
}
