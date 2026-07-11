/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： ReportUnitTest
*版本号： V1.0.0.0
*唯一标识：9fd70d43-e5de-4749-b590-bb5c5b5f0753
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/2/24 15:23:08
*描述：
*
*=================================================
*修改标记
*修改时间：2025/2/24 15:23:08
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using LuBan.Reporting;
using LuBan.Reporting.Attrs;

namespace LuBan.UnitTestProject
{


    /// <summary>
    /// ReportUnitTest
    /// </summary>
    [TestClass]
    public class ReportUnitTest
    {

        /// <summary>
        /// TestMethod1 测试Report和相关类导出文件
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                var data = new List<Hospital>();
                data.Add(new Hospital() { Name = "医院1", Address = "地址1,aaa'bbb,ccc,[dddd],\"eeee\"", Phone = "电话1", CreateTime = DateTime.Now, IsImportant = true, HospitalType = EnumHospitalType.Level1 });
                data.Add(new Hospital() { Name = "医院2", Address = "地址2", Phone = "电话2", CreateTime = DateTime.Now, IsImportant = false, Score = 3, HospitalType = EnumHospitalType.Other });

                var report = new Report<Hospital>(data);
                var filePath = $"{Path.Combine(PathUtil.CurrentPath, "downloads", $"医院信息-{DateTime.Now:yyyyMMddHHmmss}.xlsx")}";
                report.Export(filePath);

                var filePath2 = $"{Path.Combine(PathUtil.CurrentPath, "downloads", $"医院信息-{DateTime.Now:yyyyMMddHHmmss}.csv")}";
                report.Export(filePath2);

                Assert.IsTrue(File.Exists(filePath));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }

    /// <summary>
    /// 定义医院类
    /// </summary>
    public class Hospital
    {
        [ReportDescription("是否重要", 3, "重要,不重要")]
        public bool IsImportant { get; set; }

        [ReportDescription("医院名称", 1)]
        public string Name { get; set; }


        [ReportDescription("医院地址", 6)]
        public string Address { get; set; }


        [ReportDescription("医院电话", 4)]
        public string Phone { get; set; }


        [ReportDescription("创建时间", 7, datetimeFormat: "yyyy-MM-dd")]
        public DateTime CreateTime { get; set; }

        [ReportDescription("分数", 5)]
        public int? Score { get; set; }

        [ReportDescription("医院类型", 2, enumValues: "级别一,级别二,级别三,其他")]
        public EnumHospitalType HospitalType { get; set; }
    }

    public enum EnumHospitalType : int
    {
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Other = 4
    }
}
