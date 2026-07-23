/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： QingflowClientUnitTest
*版本号： V1.0.0.0
*唯一标识：5ff99c39-3531-4a48-9bcb-404d9515c9cf
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/24 9:54:12
*描述：轻流客户端单元测试
*
*=================================================
*修改标记
*修改时间：2024/12/24 9:54:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：轻流客户端单元测试
*
*****************************************************************************/
using LuBan.Common;
using LuBan.Qingflow;
using LuBan.Qingflow.Models;

using System.Diagnostics;

namespace LuBan.XTestProject;

/// <summary>
/// 轻流客户端单元测试
/// </summary>
[TestClass]
public class QingflowClientUnitTest
{
    /// <summary>
    /// 测试获取基础数据接口
    /// </summary>
    [TestMethod]
    public async Task TestMethod()
    {
        var options = OpenApiClient.Instance.Options;
        foreach (var item in options.QingflowApps)
        {
            try
            {
                if (item.AppId != "b5b5adec")
                {
                    continue;
                }
                var data = OpenApiClient.Instance.GetAppDataListAsync(new GetAppDataListInput()
                {
                    AppId = item.AppId,
                    PageIndex = 1,
                    PageSize = 10
                }).Result;

                var fileds = data.Result.Result.Select(q => q.FiledInfos.Select(qq => qq.QueTitle)).ToList();

                var dataId = data.Result.Result.First().AppDataId;

                var patients = await OpenApiClient.Instance.GetModelByIdCardAsync<BusQPatient>(new GetAppDataByIdCardInput()
                {
                    AppId = "YOUR_APP_ID",
                    PatientIdCard = "000000000000000000"
                });

                var patient = patients.FirstOrDefault();


                var data2 = OpenApiClient.Instance.GetAuditFlowListAsync(dataId.ToString()).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Assert.IsTrue(false);
            }

            Assert.IsTrue(true);
        }

    }
    /// <summary>
    /// 测试获取基础数据接口
    /// </summary>
    [TestMethod]
    public void TestMethod1()
    {
        var options = OpenApiClient.Instance.Options;
        foreach (var item in options.QingflowApps)
        {
            try
            {
                if (item.AppId != "56924fa7")
                {
                    continue;
                }
                var data = OpenApiClient.Instance.GetAppDataListAsync(new GetAppDataListInput()
                {
                    AppId = item.AppId,
                    PageIndex = 1,
                    PageSize = 10
                }).Result;

                var fileds = data.Result.Result.Select(q => q.FiledInfos.Select(qq => qq.QueTitle)).ToList();

                var dataId = data.Result.Result.First().AppDataId;
                var data2 = OpenApiClient.Instance.GetAuditFlowListAsync(dataId.ToString()).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Assert.IsTrue(false);
            }

            Assert.IsTrue(true);
        }

    }

    /// <summary>
    /// 测试根据申请人获取患者挂号信息，
    /// 申请人轻流帐号id从应用中获取传入
    /// </summary>
    [TestMethod]
    public void TestMethod2()
    {
        try
        {
            //注册登记

            var input = new Qingflow.Models.GetPatientRegistInfoInput()
            {
                AppId = "YOUR_APP_ID",//注册登记
                Applicant = "YOUR_APPLICANT_ID"//"A"
            };

            var result = OpenApiClient.Instance.GetPatientRegistInfosAsync(input).Result;

            Assert.IsTrue(result.ErrCode == 0);


            var result2 = OpenApiClient.Instance.GetPatientRegistInfosAsync<PatientFlow>(input).Result;

            Assert.IsTrue(result2 != null && result2.ErrCode == 0);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Assert.IsTrue(false);
        }
    }


    /// <summary>
    /// 测试获取患者管理名单信息
    /// </summary>
    [TestMethod]
    public void TestMethod3()
    {
        try
        {
            //注册登记

            var input = new Qingflow.Models.GetPatientRegistInfoInput()
            {
                AppId = "YOUR_APP_ID",//注册登记
                Applicant = "YOUR_APPLICANT_ID"//轻流帐号id
            };

            var result = OpenApiClient.Instance.GetPatientRegistInfosAsync(input).Result;

            Assert.IsTrue(result.ErrCode == 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Assert.IsTrue(false);
        }
    }

    /// <summary>
    /// 测试患者管理名单
    /// </summary>
    [TestMethod]
    public void TestMethod4()
    {
        try
        {
            var input = new Qingflow.Models.GetAppDataByIdCardInput()
            {
                AppId = "YOUR_APP_ID",//患者管理名单
                PatientIdCard = "000000000000000000"
            };

            var result = OpenApiClient.Instance.GetAppDataByIdCardAsync(input).Result;

            Assert.IsTrue(result.ErrCode == 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Assert.IsTrue(false);
        }
    }
    /// <summary>
    /// 测试首次申请
    /// </summary>
    [TestMethod]
    public void TestMethod5()
    {
        try
        {
            var input = new Qingflow.Models.GetAppDataByIdCardInput()
            {
                AppId = "YOUR_APP_ID",//患者管理名单
                PatientIdCard = "000000000000000000"
            };

            var result = OpenApiClient.Instance.GetAppDataByIdCardAsync(input).Result;

            Assert.IsTrue(result.ErrCode == 0);

            var firstApplyData = OpenApiClient.Instance.GetModelByIdCardAsync<PatientFlow>(new LuBan.Qingflow.Models.GetAppDataByIdCardInput()
            {
                AppId = input.AppId,
                PatientIdCard = input.PatientIdCard
            }).Result;

            Assert.IsTrue(firstApplyData != null && firstApplyData.Count > 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Assert.IsTrue(false);
        }
    }


    /// <summary>
    /// 测试获取范围数据
    /// </summary>
    [TestMethod]
    public void TestMethod6()
    {
        try
        {
            var latestAppData = OpenApiClient.Instance.GetLastestModelAsync<PatientFlow>(new GetLastestAppDataInput()
            {
                AppId = "YOUR_APP_ID",//患者管理名单,
                PageIndex = 1,
                PageSize = 200,
                FromDateTime = DateTime.Parse("2024-12-20"),
                ToDateTime = DateTime.Now,
                IsAll = true

            }).Result;

            Assert.IsTrue(latestAppData != null && latestAppData.ErrCode == 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Assert.IsTrue(false);
        }
    }

    /// <summary>
    /// 测试获取范围数据
    /// </summary>
    [TestMethod]
    public void TestMethod7()
    {
        try
        {
            var latestAppData = OpenApiClient.Instance.GetLastestAppDataAsync(new GetLastestAppDataInput()
            {
                AppId = "YOUR_APP_ID",//注册登录,
                PageIndex = 1,
                PageSize = 200,
                FromDateTime = DateTime.Parse("2024-12-30 18:00:00"),
                ToDateTime = DateTime.Now,
                IsAll = true

            }).Result;

            Assert.IsTrue(latestAppData != null && latestAppData.ErrCode == 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Assert.IsTrue(false);
        }
    }

    /// <summary>
    /// 测试获取某条数据详情
    /// </summary>
    [TestMethod]
    public void TestMethod8()
    {
        try
        {
            var appData = OpenApiClient.Instance.GetAppDataByIdAsync(186666497).Result;

            Assert.IsTrue(appData != null && appData.ErrCode == 0);

            var model = OpenApiClient.Instance.GetModelByIdAsync<PatientFlow>(186666497).Result;

            Assert.IsTrue(model != null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Assert.IsTrue(false);
        }
    }

    /// <summary>
    /// 测试获取审批流程
    /// </summary>
    [TestMethod]
    public void TestMethod9()
    {
        //将unix时间戳转换为本地DateTime
        var ticket = 1732853627000;
        var dt = DateTimeUtil.FromUnixTimeStamp(ticket);

        var dataId1 = 187082258;
        var data1 = OpenApiClient.Instance.GetAuditFlowListAsync(dataId1.ToString()).Result;

        Assert.IsTrue(data1 != null && data1.ErrCode == 0);

        var data2 = OpenApiClient.Instance.GetLatestAuditFlowAsync(dataId1.ToString(), "患者填写快递单号").Result;

        Assert.IsTrue(data2 != null);
    }


    /// <summary>
    /// 测试删除贝莫苏拜申领科研用药数据，
    /// https://cloud.gaing.cn/index/navigation/9473/tagId/2037790/app/cf8d9120/view/aa00d1f0
    /// </summary>
    [TestMethod]
    public void TestMethod10()
    {
        try
        {
            var input = new DeleteAppDataInput()
            {
                AppId = "YOUR_APP_ID",
                AppDataId = 186704860
            };

            var result = OpenApiClient.Instance.DeleteAppDataAsync(input).Result;

            Assert.IsNotNull(result);
        }
        catch (Exception)
        {
            Debugger.Break();
        }
    }


    /// <summary>
    /// 测试获取用户信息,
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/firpyap0k0fgyk7w
    /// </summary>
    [TestMethod]
    public void TestMethod11()
    {
        try
        {
            var userId = "YOUR_USER_ID";

            var result = OpenApiClient.Instance.GetUserAsync(userId).Result;

            Assert.IsNotNull(result);
        }
        catch (Exception)
        {
            Debugger.Break();
        }
    }



    /// <summary>
    /// 贝莫苏拜
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task TestMethodAsync12()
    {
        try
        {
            Stopwatch sp = Stopwatch.StartNew();

            var options = NacosConfigUtil.Read<QingflowOptions>();
            var openApiClient = new OpenApiClient(options);
            var result = await OpenApiClient.Instance.GetModelByIdCardAsync<PatientFlow>(new GetAppDataByIdCardInput()
            {
                AppId = "YOUR_APP_ID",
                IdCardQueId = 103011938,
                PatientIdCard = "000000000000000000"
            });

            var ss = sp.ElapsedMilliseconds;

            sp.Stop();

            Assert.IsNotNull(result);
        }
        catch (Exception)
        {
            Debugger.Break();
        }
    }

    #region 助景达


    /// <summary>
    /// 测试获取范围数据
    /// </summary>
    [TestMethod]
    public void TestMethod111()
    {
        try
        {
            var options = NacosConfigUtil.Read<QingflowOptions>("EntinostatOptions");
            var openApiClient = new OpenApiClient(options);

            var latestAppData = openApiClient.GetAppDataListAsync(new GetAppDataListInput()
            {
                AppId = "YOUR_APP_ID",
                IsAll = true,
                PageIndex = 1,
                PageSize = 5
            }).Result;

            Assert.IsTrue(latestAppData != null && latestAppData.ErrCode == 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Assert.IsTrue(false);
        }
    }


    /// <summary>
    /// 测试获取轻流景助达项目报表数据,
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/wyokz56ootk13hpm
    /// </summary>
    [TestMethod]
    public void TestMethod12()
    {
        try
        {
            var options = NacosConfigUtil.Read<QingflowOptions>("EntinostatOptions");
            var openApiClient = new OpenApiClient(options);
            //领药总明细
            var result = openApiClient.GetChartDataAsync(new GetChartDataListInput()
            {
                ChartKey = "YOUR_CHART_KEY",
                Filter = new ChartDataListInputFilter()
                {
                    PageNum = 1,
                    PageSize = 10,
                    //QueryKey = "天津太平振华大药房"
                },
                AccurateQuery = new List<ChartDataListInputAccurateQuery>()
                {
                     new ChartDataListInputAccurateQuery()
                     {
                          QueId=2,
                          SearchKey="2025-03-01~2025-03-31"
                     }
                }
            }).Result;

            Assert.IsNotNull(result);
        }
        catch (Exception)
        {
            Debugger.Break();
        }
    }

    /// <summary>
    /// 测试获取轻流景助达项目报表数据,
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/wyokz56ootk13hpm
    /// </summary>
    [TestMethod]
    public void TestMethod13()
    {
        try
        {
            var options = NacosConfigUtil.Read<QingflowOptions>("EntinostatOptions");
            var openApiClient = new OpenApiClient(options);
            //领药总明细
            var result = openApiClient.GetChartDataByDatetimeAsync(new ChartDataByDatetimeInput()
            {
                ChartKey = "YOUR_CHART_KEY",
                PageNum = 1,
                PageSize = 2,
                IsAll = true,
                QueId = 103015411,
                StartTime = DateTime.Parse("2025-03-01"),
                EndTime = DateTime.Parse("2025-03-31")

            }).Result;

            Assert.IsNotNull(result);
        }
        catch (Exception)
        {
            Debugger.Break();
        }
    }


    #endregion


    /// <summary>
    /// 患者流程
    /// </summary>
    public class PatientFlow : IAppData
    {
        /// <summary>
        /// 当前节点流程状态
        /// </summary>
        [PropertyTitle("当前流程状态")]
        public string FlowStatus { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [PropertyTitle("更新时间")]
        public string FlowStatusDatetimeStr { get; set; }

        /// <summary>
        /// 流程状态更新时间
        /// </summary>
        [PropertyTitle("更新时间")]
        public DateTime? FlowStatusDatetime
        {
            //get
            //{
            //    if (FlowStatusDatetimeStr.IsNullOrEmpty()) return null;
            //    if (DateTime.TryParse(FlowStatusDatetimeStr, out DateTime dt))
            //    {
            //        return dt;
            //    }
            //    return null;
            //}
            get; set;
        }

        /// <summary>
        /// 申请编号,
        /// 首次或管理中有
        /// </summary>
        [PropertyTitle("申请编号")]
        public string? ApplyCode { get; set; }

        /// <summary>
        /// 在组情况
        /// </summary>
        [PropertyTitle("流程状态")]
        public string? InGroup { get; set; }

        /// <summary>
        /// 申请轮次，随访轮次
        /// </summary>
        [PropertyTitle("申请轮次")]
        public int? Rounts { get; set; }
        /// <summary>
        /// 本次领药周期数,随访领药次数
        /// </summary>
        [PropertyTitle("本次领药周期数")]
        public int? ReceiveMedicineTimes { get; set; }

        /// <summary>
        /// 身份证
        /// </summary>
        [PropertyTitle("身份证")]
        [PropertyTitle("身份证号")]
        public string? PatientIdCard { get; set; }

        /// <summary>
        /// 身份证号（脱敏）
        /// </summary>
        [PropertyTitle("身份证号（脱敏）")]
        public string? PatientEIdCard { get; set; }
        public int? AppDataId { get; set; }
    }


    /// <summary>
    /// 测试卓然舒畅
    /// </summary>
    [TestMethod]
    public void TestMethod15()
    {
        try
        {
            //_ = OpenApiClient.Instance;
            var options = NacosConfigUtil.Read<QingflowOptions>("ZhuoRanShuChangOptions");
            var openApiClient = new OpenApiClient(options);
            //领药总明细
            var result = openApiClient.GetChartDataByDatetimeAsync(new ChartDataByDatetimeInput()
            {
                ChartKey = "YOUR_CHART_KEY",
                PageNum = 1,
                PageSize = 2,
                IsAll = true,
                QueId = 103015411,
                StartTime = DateTime.Parse("2025-03-01"),
                EndTime = DateTime.Parse("2025-03-31")
            }).Result;

            Assert.IsNotNull(result);
        }
        catch (Exception)
        {
            Debugger.Break();
        }
    }


    /// <summary>
    /// 测试卓然舒畅，获取表格数据
    /// </summary>
    [TestMethod]
    public void TestMethod16()
    {
        try
        {
            //_ = OpenApiClient.Instance;
            var options = NacosConfigUtil.Read<QingflowOptions>("ZhuoRanShuChangOptions");
            var openApiClient = new OpenApiClient(options);

            //获取表格数据
            var input = new GetTableDataListInput()
            {
                AppId = "YOUR_APP_ID",
                PageIndex = 1,
                PageSize = 100,
                QueryKey = "郑州",
                Queries =
              [
                  new AppDataQuery()
                    {
                         QueId = 103024271, //盘点月份 2025-06-01
                         QueType = 4,
                         MinValue ="2025-06-01",
                         MaxValue = "2025-07-01"
                    },
                    new AppDataQuery()
                    {
                         QueId = 103023481, //项目发药点名称
                         QueType = 19,
                         SearchKey = "郑州"
                    }
              ],
                QueTitleOfTable = "实际盘库数量"
            };
            var result = openApiClient.GetModelListForTabletAsync<ActualInventoryItem>(input, (q) => q.QueTitle == "盘点日期" && q.Values != null && q.Values.Any(v => v.Value.IsNotNullOrEmpty())).GetAwaiter().GetResult();

            Assert.IsNotNull(result);
        }
        catch (Exception)
        {
            Debugger.Break();
        }
    }

    /// <summary>
    /// 测试卓然舒畅，从轻流中获取数据和表格数据
    /// </summary>
    [TestMethod]
    public void TestMethod17()
    {
        try
        {
            //_ = OpenApiClient.Instance;
            var options = NacosConfigUtil.Read<QingflowOptions>("ZhuoRanShuChangOptions");
            var openApiClient = new OpenApiClient(options);

            var input = new GetTableDataListInput()
            {
                AppId = "YOUR_APP_ID",
                PageIndex = 1,
                PageSize = 100,
                QueryKey = "郑州",
                Queries =
               [
                   new AppDataQuery()
                    {
                         QueId = 103024271, //盘点月份 2025-06-01
                         QueType = 4,
                         MinValue ="2025-06-01",
                         MaxValue = "2025-07-01"
                    },
                    new AppDataQuery()
                    {
                         QueId = 103023481, //项目发药点名称
                         QueType = 19,
                         SearchKey = "郑州"
                    }
               ],
                QueTitleOfTable = "实际盘库数量"
            };

            //获取表格数据
            var result = openApiClient.GetModelListWithTableDataAsync<ActualInventory, ActualInventoryItem>(input, (q) => q.QueTitle == "盘点日期" && q.Values != null && q.Values.Any(v => v.Value.IsNotNullOrEmpty())).GetAwaiter().GetResult();

            Assert.IsNotNull(result);
        }
        catch (Exception)
        {
            Debugger.Break();
        }
    }
}



/// <summary>
/// 该隐轻流项目患者表
/// </summary>
public class BusQPatient : IAppData
{
    /// <summary>
    /// 项目名称
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    /// 申请人轻流id
    /// </summary>
    [PropertyTitle("申请人")]
    public string? Applicant { get; set; }
    /// <summary>
    /// 注册登记编号,
    /// 注册通过时生成
    /// </summary>
    [PropertyTitle("申请编号")]
    public string? ApplyCode { get; set; }
    /// <summary>
    /// 注册时间
    /// </summary>
    [PropertyTitle("申请时间")]
    public DateTime RegistDate { get; set; }
    /// <summary>
    /// 患者姓名
    /// </summary>
    [PropertyTitle("姓名")]
    public string PatientName { get; set; }
    /// <summary>
    /// 身份证
    /// </summary>
    [PropertyTitle("身份证")]
    [PropertyTitle("身份证号")]
    public string PatientIdCard { get; set; }

    /// <summary>
    /// 脱敏患者姓名
    /// </summary>
    [PropertyTitle("姓名（脱敏）")]
    public string PatientEName { get; set; }

    /// <summary>
    /// 脱敏身份证
    /// </summary>
    [PropertyTitle("身份证号（脱敏）")]
    public string PatientEIdCard { get; set; }
    /// <summary>
    /// 注册状态
    /// </summary>
    [PropertyTitle("当前流程状态")]
    public string RegistStatus { get; set; }

    /// <summary>
    /// 是否入组
    /// </summary>
    public bool? IsInGroup { get; set; }


    /// <summary>
    /// 轻流数据id
    /// </summary>
    public int? AppDataId { get; set; }

    /// <summary>
    /// 申请人是否错误
    /// </summary>

    public bool ApplicantIsError { get; set; } = false;


    /// <summary>
    /// 获取脱敏患者姓名
    /// </summary>
    /// <param name="patientName"></param>
    /// <returns></returns>
    public static string GetPatientEName(string patientName)
    {
        if (patientName.IsNullOrEmpty())
        {
            return patientName;
        }
        if (patientName.Length == 2)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^(.)(.*)");
            return regex.Replace(patientName, "$1*");
        }
        if (patientName.Length == 3)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^(.)(.)(.*)");
            return regex.Replace(patientName, "$1*$3");
        }
        if (patientName.Length == 4)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^(.)(.)(.)(.*)");
            return regex.Replace(patientName, "$1**$4");
        }
        if (patientName.Length > 4)
        {
            var first = patientName.Substring(0, 1);
            var last = patientName.Substring(patientName.Length - 1, 1);
            return first + "*".PadLeft(patientName.Length - 2, '*') + last;
        }
        else
        {
            var regex = new System.Text.RegularExpressions.Regex(@"(.)(.*)");
            return regex.Replace(patientName, "$1*");
        }
    }
    /// <summary>
    /// 获取脱敏患者身份证
    /// </summary>
    /// <param name="patientIdCard"></param>
    /// <returns></returns>
    public static string GetPatientEIdCard(string patientIdCard)
    {
        var regex = new System.Text.RegularExpressions.Regex(@"^(\d{6})\d{8}(\d{3}[0-9X])$");
        return regex.Replace(patientIdCard, "$1*********$2");
    }

    /// <summary>
    /// 入组时间
    /// </summary>
    [PropertyTitle("入组时间")]
    public DateTime? InGroupTime { get; set; }

    /// <summary>
    /// 申请轮次
    /// </summary>
    [PropertyTitle("申请轮次")]
    public int? ApplyRound { get; set; }

    /// <summary>
    /// 本周期允许领药次数
    /// </summary>
    [PropertyTitle("本周期允许领药次数")]
    public int? AllowReceiveMedicineTimes { get; set; }

    /// <summary>
    /// 是否为最后一次领药
    /// </summary>
    [PropertyTitle("是否为最后一次领药")]
    public bool? IsLastReceiveMedicine { get; set; }

    #region 贝莫苏拜相关字段

    /// <summary>
    /// 第二年度方案是否为2+16
    /// </summary>
    [PropertyTitle("第二年度方案是否为2+16")]
    public string? SecondYearSetting { get; set; }

    /// <summary>
    /// 贝莫苏拜后续随访中最大次数
    /// </summary>
    public int BenmelstobarMaxTimes { get; set; } = 0;

    /// <summary>
    /// 是否为特殊方案及再入组患者
    /// </summary>
    [PropertyTitle("是否为特殊方案及再入组患者")]
    public string? IsSpecialSchemePatient { get; set; }

    /// <summary>
    /// 特殊方案及再入组开始轮次
    /// </summary>
    [PropertyTitle("特殊方案及再入组开始轮次")]
    public int? SpecialSchemeStartRound { get; set; }

    /// <summary>
    /// 特殊方案及再入组领药开始次数
    /// </summary>
    [PropertyTitle("特殊方案及再入组领药开始次数")]
    public int? SpecialSchemeStartReceiveMedicineTimes { get; set; }

    #endregion


}



public class ActualInventory
{
    /// <summary>
    /// 项目发药点名称
    /// </summary>
    [PropertyTitle("项目发药点名称")]
    public string City { get; set; }
    /// <summary>
    /// 批号
    /// </summary>
    [PropertyTitle("盘点月份")]
    public string Month { get; set; }

    /// <summary>
    /// 药品名称
    /// </summary>
    [PropertyTitle("药品名称")]
    public string Name { get; set; }


}





public class ActualInventoryItem
{
    /// <summary>
    /// 批号
    /// </summary>
    [PropertyTitle("批号")]
    public string BatchNumber { get; set; }

    /// <summary>
    /// 实际库存数量
    /// </summary>
    [PropertyTitle("该批号剩余数量")]
    public int Quantity { get; set; }
}
