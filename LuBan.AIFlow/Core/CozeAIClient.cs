/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AI.Core
*文件名： CozeClient
*版本号： V1.0.0.0
*唯一标识：a9a020c0-0510-475a-b8a3-df776d7f0499
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/7/1 14:19:24
*描述：
*
*=================================================
*修改标记
*修改时间：2025/7/1 14:19:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
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
    public int code { get; set; }
    public string msg { get; set; }
    public string debug_url { get; set; }
    public string execute_id { get; set; }
    public CozeResponseDetail detail { get; set; }
}

/// <summary>
/// Coze工作流执行响应详情
/// </summary>
public class CozeResponseDetail
{
    public string logid { get; set; }
}

/// <summary>
/// Coze工作流历史记录响应
/// </summary>
public class CozeHistoryResponse
{
    public int code { get; set; }
    public string msg { get; set; }
    public List<CozeHistoryData> data { get; set; }
    public CozeResponseDetail detail { get; set; }
}

/// <summary>
/// Coze工作流历史记录数据
/// </summary>
public class CozeHistoryData
{
    public int run_mode { get; set; }
    public string output { get; set; }
    public string bot_id { get; set; }
    public string token { get; set; }
    public bool is_output_trimmed { get; set; }
    public CozeUsage usage { get; set; }
    public string error_code { get; set; }
    public string connector_id { get; set; }
    public long create_time { get; set; }
    public string debug_url { get; set; }
    public Dictionary<string, object> node_execute_status { get; set; }
    public long update_time { get; set; }
    public string logid { get; set; }
    public string connector_uid { get; set; }
    public string execute_id { get; set; }
    public string execute_status { get; set; }
    public string error_message { get; set; }
}

/// <summary>
/// Coze使用情况
/// </summary>
public class CozeUsage
{
    public int input_count { get; set; }
    public int token_count { get; set; }
    public int output_count { get; set; }
}

/// <summary>
/// Coze输出结果最外层模型
/// </summary>
public class CozeOutputResult
{
    public string Output { get; set; }
    public string node_status { get; set; }
}

/// <summary>
/// Coze输出结果内层模型
/// </summary>
public class CozeInnerOutput
{
    public string output { get; set; }
    public string reasoning_content { get; set; }
}

/// <summary>
/// Coze药店信息模型
/// </summary>
public class CozePharmacyInfo
{
    public string address_rank { get; set; }
    public string bl_address { get; set; }
    public string bl_id { get; set; }
    public string bl_name { get; set; }
    public string bl_status { get; set; }
    public string info_source { get; set; }
    public string name_rank { get; set; }
    public string o_company_name { get; set; }
    public string o_store_address { get; set; }
    public string o_store_name { get; set; }
    public string remark { get; set; }
}

