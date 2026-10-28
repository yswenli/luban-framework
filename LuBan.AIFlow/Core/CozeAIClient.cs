/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIFlow.Core
*文件名： CozeAIClient
*版本号： V1.0.0.0
*唯一标识：a9a020c0-0510-475a-b8a3-df776d7f0499
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/7/1 14:19:24
*描述：CozeClient
*
*=================================================
*修改标记
*修改时间：2025/7/1 14:19:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：CozeClient
*
*****************************************************************************/
namespace LuBan.AIFlow.Core;

/// <summary>
/// CozeClient
/// </summary>
public class CozeAIClient : IAIClient
{
    private const string RUN_WORKFLOW_ENDPOINT = "/workflow/run";
    private const string GET_WORKFLOW_HISTORY_ENDPOINT = "/workflows/:workflow_id/run_histories/:execute_id";

    /// <summary>
    /// AI客户端选项
    /// </summary>
    public AIOptions Options { get; }

    private readonly HttpClientProxy _httpClientProxy;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">AI客户端选项</param>
    public CozeAIClient(AIOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClientProxy = HttpClientProxy.Create(options.BaseUrl ?? "https://api.coze.cn/v1", useLog: true);
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public CozeAIClient() : this(NacosConfigUtil.Read<AIOptions>() ?? throw new Exception("找不到AI配置"))
    {
    }

    /// <summary>
    /// 执行工作流
    /// </summary>
    /// <param name="workflowId">工作流ID</param>
    /// <param name="input">输入参数</param>
    /// <param name="isAsync">是否异步执行</param>
    /// <returns>工作流执行结果</returns>
    public CozeResponse RunWorkflow(string workflowId, string input, bool isAsync = true)
    {
        var requestData = new
        {
            parameters = new
            {
                input = input
            },
            workflow_id = workflowId,
            is_async = isAsync
        };

        var jsonData = SerializeUtil.Serialize(requestData);
        var headers = new Dictionary<string, string> {
            { "Authorization", $"Bearer {Options.ApiKey}" }
        };
        return _httpClientProxy.PostJson<CozeResponse>(RUN_WORKFLOW_ENDPOINT, jsonData, headers);
    }
    /// <summary>
    /// 查询工作流历史记录
    /// </summary>
    /// <param name="workflow_id"></param>
    /// <param name="execute_id"></param>
    /// <returns></returns>
    public CozeHistoryResponse GetWorkflowHistory(string workflow_id, string execute_id)
    {
        var headers = new Dictionary<string, string> {
            { "Authorization", $"Bearer {Options.ApiKey}" }
        };
        return _httpClientProxy.Get<CozeHistoryResponse>(GET_WORKFLOW_HISTORY_ENDPOINT.Replace(":workflow_id", workflow_id).Replace(":execute_id", execute_id), headers);
    }

    /// <summary>
    /// 解析CozeHistoryResponse中的output内容为药店信息列表
    /// </summary>
    /// <param name="historyResponse">工作流历史记录响应</param>
    /// <returns>药店信息列表</returns>
    public List<CozePharmacyInfo> ParsePharmacyInfo(CozeHistoryResponse historyResponse)
    {
        if (historyResponse?.data == null || historyResponse.data.Count == 0)
        {
            return new List<CozePharmacyInfo>();
        }

        try
        {
            var output = historyResponse.data[0].output;
            if (string.IsNullOrEmpty(output))
            {
                return new List<CozePharmacyInfo>();
            }

            // 解析最外层Output结果
            var cozeOutputResult = SerializeUtil.Deserialize<CozeOutputResult>(output);
            if (string.IsNullOrEmpty(cozeOutputResult?.Output))
            {
                return new List<CozePharmacyInfo>();
            }

            // 解析内层output结果
            var cozeInnerOutput = SerializeUtil.Deserialize<CozeInnerOutput>(cozeOutputResult.Output);
            if (string.IsNullOrEmpty(cozeInnerOutput?.output))
            {
                return new List<CozePharmacyInfo>();
            }

            // 解析药店信息列表
            return SerializeUtil.Deserialize<List<CozePharmacyInfo>>(cozeInnerOutput.output) ?? new List<CozePharmacyInfo>();
        }
        catch (Exception ex)
        {
            throw new Exception("解析Coze输出结果失败: " + ex.Message);
        }
    }
}

/// <summary>
/// Coze工作流执行响应
/// </summary>
public class CozeResponse
{
    /// <summary>
    /// Coze 接口状态码，0 表示成功（接口字段 code）。
    /// </summary>
    public int code { get; set; }
    /// <summary>
    /// 接口返回的状态信息（接口字段 msg）。
    /// </summary>
    public string msg { get; set; }
    /// <summary>
    /// 工作流调试页面地址（接口字段 debug_url）。
    /// </summary>
    public string debug_url { get; set; }
    /// <summary>
    /// 本次工作流执行记录标识，可用于查询执行历史（接口字段 execute_id）。
    /// </summary>
    public string execute_id { get; set; }
    /// <summary>
    /// 响应详情，承载日志标识等信息（接口字段 detail）。
    /// </summary>
    public CozeResponseDetail detail { get; set; }
}

/// <summary>
/// Coze工作流执行响应详情
/// </summary>
public class CozeResponseDetail
{
    /// <summary>
    /// 请求日志标识，用于问题排查（接口字段 logid）。
    /// </summary>
    public string logid { get; set; }
}

/// <summary>
/// Coze工作流历史记录响应
/// </summary>
public class CozeHistoryResponse
{
    /// <summary>
    /// Coze 接口状态码，0 表示成功（接口字段 code）。
    /// </summary>
    public int code { get; set; }
    /// <summary>
    /// 接口返回的状态信息（接口字段 msg）。
    /// </summary>
    public string msg { get; set; }
    /// <summary>
    /// 工作流执行历史记录集合（接口字段 data）。
    /// </summary>
    public List<CozeHistoryData> data { get; set; }
    /// <summary>
    /// 响应详情，承载日志标识等信息（接口字段 detail）。
    /// </summary>
    public CozeResponseDetail detail { get; set; }
}

/// <summary>
/// Coze工作流历史记录数据
/// </summary>
public class CozeHistoryData
{
    /// <summary>
    /// 工作流运行模式，区分调试运行与线上运行（接口字段 run_mode）。
    /// </summary>
    public int run_mode { get; set; }
    /// <summary>
    /// 工作流的输出内容，通常是 JSON 字符串（接口字段 output）。
    /// </summary>
    public string output { get; set; }
    /// <summary>
    /// 发起本次执行所使用的 Bot 标识（接口字段 bot_id）。
    /// </summary>
    public string bot_id { get; set; }
    /// <summary>
    /// 本次执行消耗的 Token 数（接口字段 token）。
    /// </summary>
    public string token { get; set; }
    /// <summary>
    /// 输出内容是否被截断（接口字段 is_output_trimmed）。
    /// </summary>
    public bool is_output_trimmed { get; set; }
    /// <summary>
    /// 本次执行的 Token 用量统计（接口字段 usage）。
    /// </summary>
    public CozeUsage usage { get; set; }
    /// <summary>
    /// 执行失败时的错误码（接口字段 error_code）。
    /// </summary>
    public string error_code { get; set; }
    /// <summary>
    /// 执行所用的连接器标识（接口字段 connector_id）。
    /// </summary>
    public string connector_id { get; set; }
    /// <summary>
    /// 执行记录的创建时间，Unix 时间戳秒（接口字段 create_time）。
    /// </summary>
    public long create_time { get; set; }
    /// <summary>
    /// 该次执行的调试页面地址（接口字段 debug_url）。
    /// </summary>
    public string debug_url { get; set; }
    /// <summary>
    /// 各节点执行状态映射，键为节点标识（接口字段 node_execute_status）。
    /// </summary>
    public Dictionary<string, object> node_execute_status { get; set; }
    /// <summary>
    /// 执行记录的最后更新时间，Unix 时间戳秒（接口字段 update_time）。
    /// </summary>
    public long update_time { get; set; }
    /// <summary>
    /// 该次执行的日志标识（接口字段 logid）。
    /// </summary>
    public string logid { get; set; }
    /// <summary>
    /// 连接器所属用户标识（接口字段 connector_uid）。
    /// </summary>
    public string connector_uid { get; set; }
    /// <summary>
    /// 本次执行记录标识（接口字段 execute_id）。
    /// </summary>
    public string execute_id { get; set; }
    /// <summary>
    /// 执行状态，用于判断工作流是否已完成（接口字段 execute_status）。
    /// </summary>
    public string execute_status { get; set; }
    /// <summary>
    /// 执行失败时的错误描述（接口字段 error_message）。
    /// </summary>
    public string error_message { get; set; }
}

/// <summary>
/// Coze使用情况
/// </summary>
public class CozeUsage
{
    /// <summary>
    /// 输入侧消耗的 Token 数（接口字段 input_count）。
    /// </summary>
    public int input_count { get; set; }
    /// <summary>
    /// 本次执行累计消耗的 Token 总数（接口字段 token_count）。
    /// </summary>
    public int token_count { get; set; }
    /// <summary>
    /// 输出侧消耗的 Token 数（接口字段 output_count）。
    /// </summary>
    public int output_count { get; set; }
}

/// <summary>
/// Coze输出结果最外层模型
/// </summary>
public class CozeOutputResult
{
    /// <summary>
    /// 工作流输出的 JSON 字符串，需再次反序列化（接口字段 Output）。
    /// </summary>
    public string Output { get; set; }
    /// <summary>
    /// 输出所对应节点的执行状态（接口字段 node_status）。
    /// </summary>
    public string node_status { get; set; }
}

/// <summary>
/// Coze输出结果内层模型
/// </summary>
public class CozeInnerOutput
{
    /// <summary>
    /// 内层输出的 JSON 字符串，最终为药店信息数组（接口字段 output）。
    /// </summary>
    public string output { get; set; }
    /// <summary>
    /// 模型推理过程的内容（接口字段 reasoning_content）。
    /// </summary>
    public string reasoning_content { get; set; }
}

/// <summary>
/// Coze药店信息模型
/// </summary>
public class CozePharmacyInfo
{
    /// <summary>
    /// 地址相似度排名（接口字段 address_rank）。
    /// </summary>
    public string address_rank { get; set; }
    /// <summary>
    /// bl 来源的门店地址（接口字段 bl_address）。
    /// </summary>
    public string bl_address { get; set; }
    /// <summary>
    /// bl 来源的门店标识（接口字段 bl_id）。
    /// </summary>
    public string bl_id { get; set; }
    /// <summary>
    /// bl 来源的门店名称（接口字段 bl_name）。
    /// </summary>
    public string bl_name { get; set; }
    /// <summary>
    /// bl 来源的门店状态（接口字段 bl_status）。
    /// </summary>
    public string bl_status { get; set; }
    /// <summary>
    /// 该条药店信息的来源（接口字段 info_source）。
    /// </summary>
    public string info_source { get; set; }
    /// <summary>
    /// 名称相似度排名（接口字段 name_rank）。
    /// </summary>
    public string name_rank { get; set; }
    /// <summary>
    /// 原始公司名称（接口字段 o_company_name）。
    /// </summary>
    public string o_company_name { get; set; }
    /// <summary>
    /// 原始门店地址（接口字段 o_store_address）。
    /// </summary>
    public string o_store_address { get; set; }
    /// <summary>
    /// 原始门店名称（接口字段 o_store_name）。
    /// </summary>
    public string o_store_name { get; set; }
    /// <summary>
    /// 备注信息（接口字段 remark）。
    /// </summary>
    public string remark { get; set; }
}

