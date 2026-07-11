/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： TreeListUnitTest
*版本号： V1.0.0.0
*唯一标识：5b11d456-cc25-4d26-8995-d5b418276351
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/29 15:53:12
*描述：
*
*=================================================
*修改标记
*修改时间：2024/11/29 15:53:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.XTestProject;

[TestClass]
public class TreeListUnitTest
{
    [TestMethod]
    public void TestMethod1()
    {
        var list = new List<NodeInfo>()
        {
            new(){Id = 1,Name = "1", ParentId = null},
            new(){Id = 2,Name = "12", ParentId = 1},
            new(){Id = 3,Name = "123", ParentId = 2},
            new(){Id = 4,Name = "2", ParentId = 2},
            new(){Id = 5,Name = "3", ParentId = 2},
            new(){Id = 6,Name = "6", ParentId = 1},
            new(){Id = 7,Name = "7", ParentId = 6},
            new(){Id = 8,Name = "8", ParentId = 7},
            new(){Id = 9,Name = "9", ParentId = 8},
            new(){Id = 10,Name = "10", ParentId = 9},
            new(){Id = 11,Name = "11", ParentId = 10},
            new(){Id = 12,Name = "12", ParentId = 11},
            new(){Id = 13,Name = "13", ParentId = 12},
            new(){Id = 14,Name = "14", ParentId = 13},
            new(){Id = 15,Name = "15", ParentId = 14},
            new(){Id = 16,Name = "16", ParentId = 15},
            new(){Id = 17,Name = "17", ParentId = 16}
        };
        try
        {
            var treeList = list.ToTreeList(x => x.Id, x => x.Childrens, x => x.ParentId);

            Assert.IsNotNull(treeList);

        }
        catch (Exception ex)
        {
            Assert.IsNotNull(ex);
        }
    }
}

public class NodeInfo
{
    public int Id { get; set; }

    public int? ParentId { get; set; }


    public string Name { get; set; }

    public List<NodeInfo> Childrens { get; set; } = new List<NodeInfo>();
}
