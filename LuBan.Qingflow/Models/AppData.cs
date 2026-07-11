/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow.Models
*文件名： AppData
*版本号： V1.0.0.0
*唯一标识：d0ac0d22-4559-4c50-bf1d-5509818817c2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/23 17:57:15
*描述：
*
*=================================================
*修改标记
*修改时间：2024/12/23 17:57:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Qingflow.Models;

/// <summary>
/// 应用数据
/// </summary>
public class AppData : IAppData
{
    /// <summary>
    /// 数据的id
    /// </summary>
    [JsonProperty(PropertyName = "applyId")]
    public int? AppDataId { get; set; }
    /// <summary>
    /// 字段信息列表
    /// </summary>
    [JsonProperty(PropertyName = "answers")]
    public List<FiledInfo> FiledInfos { get; set; }
}

/// <summary>
/// 字段信息
/// </summary>
public class FiledInfo
{
    /// <summary>
    /// 字段Id
    /// </summary>
    [JsonProperty(PropertyName = "queId")]
    public int QueId { get; set; }
    /// <summary>
    /// 字段标题
    /// </summary>
    [JsonProperty(PropertyName = "queTitle")]
    public string QueTitle { get; set; }
    /// <summary>
    /// 字段类型。queType可见文档1、描述文字；2、单行文字；3、多行文字；4、日期时间；5、成员字段；6、邮箱；7、手机；8、数字；9、链接；10、单项选择；11、下拉选择；12、多项选择；13、上传附件；14、起止时间；15、图片选择；16、富文本；17、定位字段；18、表格；19、数据关联；20、Q-Linker；21、地址字段；22、部门字段；
    /// </summary>
    [JsonProperty(PropertyName = "queType")]
    public EnumQueType QueType { get; set; }
    /// <summary>
    /// 字段中的值
    /// </summary>
    [JsonProperty(PropertyName = "values")]
    public List<ValueInfo> Values { get; set; }

    /// <summary>
    /// tableValues
    /// </summary>
    [JsonProperty(PropertyName = "tableValues")]
    public List<TableInfo> TableValues { get; set; }
}

/// <summary>
/// 字段中的值
/// </summary>
public class ValueInfo
{
    /// <summary>
    /// 值
    /// </summary>
    [JsonProperty(PropertyName = "dataValue")]
    public string DataValue { get; set; }
    /// <summary>
    /// email
    /// </summary>
    [JsonProperty(PropertyName = "email")]
    public string? Email { get; set; }

    /// <summary>
    /// 选择类型，为optId，成员类型，为uid；成员字段 ，当前用户: -1；地址字段，1-4：省、市、区、详细地址
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public int? Id { get; set; }

    /// <summary>
    /// 字段Id
    /// </summary>
    [JsonProperty(PropertyName = "queId")]
    public int QueId { get; set; }
    /// <summary>
    /// 值
    /// </summary>
    [JsonProperty(PropertyName = "value")]
    public string Value { get; set; }
}

/// <summary>
/// TableInfo
/// </summary>
public class TableInfo : List<FiledInfo>
{

}

/// <summary>
/// 数据当前所处状态。1：草稿、2:流程中（已经有用户处理过）、3：已通过（有流程）、4:已拒绝、5:待完善（退回申请人）、6:已通过（无流程）、7:流程中（没有用户处理过）
/// </summary>
public enum EnumFlowType
{
    Default = 0,
    /// <summary>
    /// 草稿 
    /// </summary>
    Draft = 1,
    /// <summary>
    /// 流程中,已经有用户处理过
    /// </summary>
    InProcess = 2,
    /// <summary>
    /// 已通过
    /// </summary>
    Completed = 3,
    /// <summary>
    /// 已拒绝
    /// </summary>
    Refuse = 4,
    /// <summary>
    /// 待完善,退回申请人
    /// </summary>
    Reture = 5,
    /// <summary>
    /// 已通过,无流程
    /// </summary>
    Passed = 6,
    /// <summary>
    /// 流程中,没有用户处理过
    /// </summary>
    Pending = 7
}
/// <summary>
/// 流程信息
/// </summary>
public class AuditFlowInfo
{
    /// <summary>
    /// 数据当前所处状态。1：草稿、2:流程中（已经有用户处理过）、3：已通过（有流程）、4:已拒绝、5:待完善（退回申请人）、6:已通过（无流程）、7:流程中（没有用户处理过）
    /// </summary>
    [JsonProperty(PropertyName = "applyStatus")]
    public EnumFlowType ApplyStatus { get; set; }
    /// <summary>
    /// 流程日志
    /// </summary>
    [JsonProperty(PropertyName = "auditRecords")]
    public List<AuditRecord> AuditRecords { get; set; }
}
/// <summary>
/// ApplyRecord
/// </summary>
public class AuditRecord
{
    /// <summary>
    /// 审批节点id
    /// </summary>
    [JsonProperty(PropertyName = "auditNodeId")]
    public int? AuditNodeId { get; set; }
    /// <summary>
    /// 审批节点名称
    /// </summary>
    [JsonProperty(PropertyName = "auditNodeName")]
    public string? AuditNodeName { get; set; }
    /// <summary>
    /// 审批时间unit时间戳
    /// </summary>
    [JsonProperty(PropertyName = "auditTime")]
    public long? AuditTime { get; set; }
    /// <summary>
    /// 审批人
    /// </summary>
    [JsonProperty(PropertyName = "auditUser")]
    public AuditUser? AuditUser { get; set; }
}

/// <summary>
/// 获得应用数据列表请求值
/// </summary>
public class GetAppDataListInput
{
    /// <summary>
    /// 应用Id
    /// </summary>
    [JsonIgnore]
    public string AppId { get; set; }
    /// <summary>
    /// 页码
    /// </summary>
    [JsonProperty(PropertyName = "pageNum")]
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 获取全部数据：8；（不传默认为type=8）,
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/wz17n68r0ayoa5sk?singleDoc#EbkfH
    /// </summary>
    [JsonProperty(PropertyName = "type")]
    public int Type { get; set; } = 8;
    /// <summary>
    /// 每页数据条数，注意pageSize最大可填200
    /// </summary>
    [JsonProperty(PropertyName = "pageSize")]
    public int PageSize { get; set; } = 10;
    /// <summary>
    /// 针对字段的查询条件
    /// </summary>
    [JsonProperty(PropertyName = "queries")]
    public List<AppDataQuery>? Queries { get; set; }

    /// <summary>
    /// 模糊搜索全部字段数据的key值
    /// </summary>
    [JsonProperty(PropertyName = "queryKey", NullValueHandling = NullValueHandling.Ignore)]
    public string? QueryKey { get; set; }


    /// <summary>
    /// 申请的数据id列表
    /// </summary>
    [JsonProperty(PropertyName = "applyIds", NullValueHandling = NullValueHandling.Ignore)]
    public long[]? AppDataIds { get; set; }

    /// <summary>
    /// 是否是全部数据
    /// </summary>
    [JsonIgnore]
    public bool IsAll { get; set; } = false;
}

/// <summary>
/// 表格数据列表请求值
/// </summary>
public class GetTableDataListInput : GetAppDataListInput
{
    /// <summary>
    /// 标题，表格数据的标题用于在结果中搜索表格数据
    /// </summary>
    [JsonIgnore]
    public string QueTitleOfTable { get; set; }
}

/// <summary>
/// 针对字段的查询条件
/// </summary>
public class AppDataQuery
{
    /// <summary>
    /// 字段Id
    /// </summary>
    [JsonProperty(PropertyName = "queId")]
    public int QueId { get; set; }

    /// <summary>
    /// 若是数字类型，此处需要赋值，且值为8
    /// </summary>
    public int? QueType { get; set; }

    /// <summary>
    /// 搜索关键字，搜索为模糊搜索
    /// </summary>
    [JsonProperty(PropertyName = "searchKey")]
    public string SearchKey { get; set; }
    /// <summary>
    /// 数字模块中，是搜索结果中最小值，日期类型，就是最早日期
    /// </summary>
    [JsonProperty(PropertyName = "minValue")]
    public string? MinValue { get; set; }
    /// <summary>
    /// 数字模块中，是搜索结果中最大值，日期类型，就是最晚日期
    /// </summary>
    [JsonProperty(PropertyName = "maxValue")]
    public string? MaxValue { get; set; }

    /// <summary>
    /// 成员字段中，搜索答案中包含这些userId的申请
    /// </summary>
    [JsonProperty(PropertyName = "searchUserIds")]
    public List<string>? SearchUserIds { get; set; }
}
/// <summary>
/// 字段类型
/// </summary>
public enum EnumQueType
{
    Default = 0,
    /// <summary>
    /// 描述文字
    /// </summary>
    Description = 1,
    /// <summary>
    /// 单行文字
    /// </summary>
    Line = 2,
    /// <summary>
    /// 多行文字
    /// </summary>
    Text = 3,
    /// <summary>
    /// 日期时间
    /// </summary>
    DateTime = 4,
    /// <summary>
    /// 成员字段
    /// </summary>
    Member = 5,
    /// <summary>
    /// 邮箱
    /// </summary>
    Email = 6,
    /// <summary>
    /// 手机
    /// </summary>
    Mobile = 7,
    /// <summary>
    /// 数字
    /// </summary>
    Number = 8,
    /// <summary>
    /// 链接
    /// </summary>
    Link = 9,
    /// <summary>
    /// 单项选择
    /// </summary>
    Radio = 10,
    /// <summary>
    /// 下拉选择
    /// </summary>
    Select = 11,
    /// <summary>
    /// 多项选择
    /// </summary>
    Checkbox = 12,
    /// <summary>
    /// 上传附件
    /// </summary>
    Attachment = 13,
    /// <summary>
    /// 起止时间
    /// </summary>
    BeginEndDateTime = 14,
    /// <summary>
    /// 图片选择
    /// </summary>
    Image = 15,
    /// <summary>
    /// 富文本
    /// </summary>
    RichText = 16,
    /// <summary>
    /// 定位字段
    /// </summary>
    Location = 17,
    /// <summary>
    /// 表格
    /// </summary>
    Table = 18,
    /// <summary>
    /// 数据关联
    /// </summary>
    Reference = 19,
    /// <summary>
    /// Q-Linker
    /// </summary>
    QLink = 20,
    /// <summary>
    /// 地址
    /// </summary>
    Addrress = 21,
    /// <summary>
    /// 部门
    /// </summary>
    Department = 22,
    /// <summary>
    /// ocr字段
    /// </summary>
    OCR = 23,
    /// <summary>
    /// 段落
    /// </summary>
    Paragraph = 24,
    /// <summary>
    /// 引用
    /// </summary>
    Quote = 25,
    /// <summary>
    /// 代码块
    /// </summary>
    Code = 26
}

/// <summary>
/// 根据申请人获取患者登记信息
/// </summary>
public class GetPatientRegistInfoInput
{
    /// <summary>
    /// 应用Id
    /// </summary>
    public string AppId { get; set; }
    /// <summary>
    /// 申请人,此处需要传入轻流帐号，例如：79f3ae7d2490
    /// </summary>
    public string Applicant { get; set; }
}
/// <summary>
/// 根据患者身份证获取患者相关应用的信息，比如随访、申请、管理名单等等
/// </summary>
public class GetAppDataByIdCardInput
{
    /// <summary>
    /// 应用Id
    /// </summary>
    public string AppId { get; set; }


    /// <summary>
    /// 身份证号字段id
    /// </summary>
    public int IdCardQueId { get; set; } = 103011938;

    /// <summary>
    /// 身份证号
    /// </summary>
    public string PatientIdCard { get; set; }

    /// <summary>
    /// 页数
    /// </summary>
    public int PageIndex { get; set; } = 1;
    /// <summary>
    /// 每页条数
    /// </summary>
    public int PageSize { get; set; } = 200;
}
/// <summary>
/// 获取指定字段数据，例如：姓名、性别、年龄、身份证号等等
/// </summary>
public class GetCustomerDataByIdCardInput : GetAppDataByIdCardInput
{
    /// <summary>
    /// 指定字段，例如：姓名、性别、年龄、身份证号等等
    /// </summary>
    public List<string> QueTitles { get; set; }
}

/// <summary>
/// 获取最新的应用数据
/// </summary>
public class GetLastestAppDataInput
{
    /// <summary>
    /// 应用Id
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    /// 页数
    /// </summary>
    public int PageIndex { get; set; }
    /// <summary>
    /// 每页条数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime FromDateTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime ToDateTime { get; set; }

    /// <summary>
    /// 是否获取所有数据
    /// </summary>
    public bool IsAll { get; set; } = false;
}

/// <summary>
/// 请求信息
/// </summary>
public class RequestInfo
{
    /// <summary>
    /// 请求ID
    /// </summary>
    [JsonProperty(PropertyName = "requestId")]
    public string RequestId { get; set; }
}

/// <summary>
/// 删除应用数据
/// </summary>
public class DeleteAppDataInput
{
    /// <summary>
    /// 应用Id
    /// </summary>
    [Required(ErrorMessage = "轻流应用id不能为空")]
    public string AppId { get; set; }
    /// <summary>
    /// 数据id
    /// </summary>
    [Required(ErrorMessage = "数据id(applyId)不能为空")]
    public long AppDataId { get; set; }
}


/// <summary>
/// 基础报表请求参数，
/// https://exiao.yuque.com/ixwxsb/cqfg2y/wyokz56ootk13hpm
/// </summary>
public class GetChartDataListInput
{
    /// <summary>
    /// 报表key
    /// </summary>
    [JsonIgnore]
    public string ChartKey { get; set; }
    /// <summary>
    /// 针对字段的查询条件
    /// </summary>
    [JsonProperty(PropertyName = "filter")]
    public ChartDataListInputFilter Filter { get; set; }
    /// <summary>
    /// 数据表设置了查询条件时，查询条件的值
    /// </summary>
    [JsonProperty(PropertyName = "accurateQuery")]
    public List<ChartDataListInputAccurateQuery> AccurateQuery { get; set; }
}

/// <summary>
/// 字段查询条件
/// </summary>
public class ChartDataListInputQuery
{
    /// <summary>
    /// 字段ID，specialQueId可见数据结构
    /// </summary>
    [JsonProperty(PropertyName = "queId")]
    public int QueId { get; set; }

    /// <summary>
    /// 搜索的关键词,选择类型传递optionId
    /// </summary>
    [JsonProperty(PropertyName = "searchKey")]
    public string SearchKey { get; set; }
}


public class ChartDataListInputFilter
{
    /// <summary>
    /// 每页数据条数
    /// </summary>
    [JsonProperty(PropertyName = "pageSize")]
    public int PageSize { get; set; } = 200;//注意pageSize最大可填200

    /// <summary>
    /// 请求页码
    /// </summary>
    [JsonProperty(PropertyName = "pageNum")]
    public int PageNum { get; set; }

    /// <summary>
    /// 针对字段的查询条件
    /// </summary>
    [JsonProperty(PropertyName = "queries")]
    public List<ChartDataListInputQuery> Queries { get; set; }

    /// <summary>
    /// 模糊搜索全部字段数据的key值
    /// </summary>
    [JsonProperty(PropertyName = "queryKey")]
    public string QueryKey { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    [JsonProperty(PropertyName = "type")]
    public int Type { get; set; } = 13;


}


public class ChartDataListInputAccurateQuery
{
    /// <summary>
    /// 字段ID，特殊queId可查看
    /// </summary>
    [JsonProperty(PropertyName = "queId")]
    public int QueId { get; set; }
    /// <summary>
    /// 搜索的关键词（选择类型字段传递optionId）
    /// </summary>
    [JsonProperty(PropertyName = "searchKey")]
    public string SearchKey { get; set; }
}
