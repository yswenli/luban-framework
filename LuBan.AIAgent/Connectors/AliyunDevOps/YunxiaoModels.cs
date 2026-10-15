/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Connectors.AliyunDevOps
*文件名： YunxiaoModels
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：云效 API 请求/响应模型
*
*****************************************************************************/

using System.Text.Json.Serialization;

namespace LuBan.AIAgent.Connectors.AliyunDevOps;

#region 通用响应

/// <summary>
/// 云效 API 通用响应包装
/// </summary>
public class YunxiaoApiResponse<T>
{
    /// <summary>
    /// 错误码（0 表示成功）
    /// </summary>
    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    [JsonPropertyName("result")]
    public T? Result { get; set; }

    /// <summary>
    /// 请求 ID
    /// </summary>
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }
}

/// <summary>
/// 分页结果
/// </summary>
public class YunxiaoPagedResult<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    [JsonPropertyName("result")]
    public List<T>? Items { get; set; }

    /// <summary>
    /// 总条数
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// 每页条数
    /// </summary>
    [JsonPropertyName("perPage")]
    public int PerPage { get; set; }
}

#endregion

#region 代码仓库（Codeup）

/// <summary>
/// 云效代码仓库
/// </summary>
public class YunxiaoRepository
{
    /// <summary>
    /// 仓库 ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 仓库路径（路径标识）
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>
    /// HTTP 克隆地址
    /// </summary>
    [JsonPropertyName("httpUrlToRepo")]
    public string? HttpUrlToRepo { get; set; }

    /// <summary>
    /// SSH 克隆地址
    /// </summary>
    [JsonPropertyName("sshUrlToRepo")]
    public string? SSHUrlToRepo { get; set; }

    /// <summary>
    /// 可见性：private / public
    /// </summary>
    [JsonPropertyName("visibilityLevel")]
    public int VisibilityLevel { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 默认分支
    /// </summary>
    [JsonPropertyName("defaultBranch")]
    public string? DefaultBranch { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    [JsonPropertyName("lastActivityAt")]
    public string? LastActivityAt { get; set; }
}

/// <summary>
/// 创建仓库请求
/// </summary>
public class YunxiaoCreateRepositoryRequest
{
    /// <summary>
    /// 仓库名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 仓库路径（路径标识）
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>
    /// 可见性：private（私有）/ internal（组织内公开，默认）/ public（公开）
    /// </summary>
    [JsonPropertyName("visibility")]
    public string? Visibility { get; set; }

    /// <summary>
    /// 可见性级别（数字，0-私有 / 10-组织内公开 / 20-公开；与 Visibility 二选一）
    /// </summary>
    [JsonPropertyName("visibilityLevel")]
    public int? VisibilityLevel { get; set; }

    /// <summary>
    /// 组织下的分组/命名空间 ID（不传则建在组织根下）
    /// </summary>
    [JsonPropertyName("namespaceId")]
    public string? NamespaceId { get; set; }

    /// <summary>
    /// path 含多级分组时是否自动创建父路径
    /// </summary>
    [JsonPropertyName("createParentPath")]
    public bool? CreateParentPath { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 模板项目（从模板仓创建时使用）
    /// </summary>
    [JsonPropertyName("templateProject")]
    public YunxiaoTemplateProject? TemplateProject { get; set; }
}

/// <summary>
/// 模板项目参数（从模板仓创建仓库）
/// </summary>
public class YunxiaoTemplateProject
{
    /// <summary>
    /// 模板仓 ID（如 TJCES-Service/Admin/Mobile 对应的仓库 ID）
    /// </summary>
    [JsonPropertyName("templateProjectId")]
    public string? TemplateProjectId { get; set; }

    /// <summary>
    /// 模板类型：1 自定义模板 / 2 系统预置模板
    /// </summary>
    [JsonPropertyName("templateType")]
    public int TemplateType { get; set; } = 1;

    /// <summary>
    /// 是否同步模板全部分支（建议 true，否则新仓可能无分支）
    /// </summary>
    [JsonPropertyName("syncAllBranches")]
    public bool SyncAllBranches { get; set; } = true;
}

/// <summary>
/// 更新仓库请求
/// </summary>
public class YunxiaoUpdateRepositoryRequest
{
    /// <summary>
    /// 仓库名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 可见性
    /// </summary>
    [JsonPropertyName("visibilityLevel")]
    public int? VisibilityLevel { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 默认分支
    /// </summary>
    [JsonPropertyName("defaultBranch")]
    public string? DefaultBranch { get; set; }
}

#endregion

#region 流水线（Flow）

/// <summary>
/// 云效流水线
/// </summary>
public class YunxiaoPipeline
{
    /// <summary>
    /// 流水线 ID
    /// </summary>
    [JsonPropertyName("pipelineId")]
    public string? PipelineId { get; set; }

    /// <summary>
    /// 流水线名称
    /// </summary>
    [JsonPropertyName("pipelineName")]
    public string? PipelineName { get; set; }

    /// <summary>
    /// 流水线配置
    /// </summary>
    [JsonPropertyName("pipelineConfig")]
    public YunxiaoPipelineConfig? PipelineConfig { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    [JsonPropertyName("modifyTime")]
    public string? ModifyTime { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>
/// 流水线配置
/// </summary>
public class YunxiaoPipelineConfig
{
    /// <summary>
    /// 流水线 YAML 内容
    /// </summary>
    [JsonPropertyName("flow")]
    public string? Flow { get; set; }
}

/// <summary>
/// 创建流水线请求
/// </summary>
public class YunxiaoCreatePipelineRequest
{
    /// <summary>
    /// 流水线名称
    /// </summary>
    [JsonPropertyName("pipelineName")]
    public string? PipelineName { get; set; }

    /// <summary>
    /// 流水线 YAML 内容
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

/// <summary>
/// 更新流水线请求
/// </summary>
public class YunxiaoUpdatePipelineRequest
{
    /// <summary>
    /// 流水线名称
    /// </summary>
    [JsonPropertyName("pipelineName")]
    public string? PipelineName { get; set; }

    /// <summary>
    /// 流水线 YAML 内容
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

/// <summary>
/// 运行流水线请求
/// </summary>
public class YunxiaoRunPipelineRequest
{
    /// <summary>
    /// 分支或标签
    /// </summary>
    [JsonPropertyName("branch")]
    public string? Branch { get; set; }

    /// <summary>
    /// 环境变量
    /// </summary>
    [JsonPropertyName("env")]
    public Dictionary<string, string>? Environment { get; set; }
}

/// <summary>
/// 克隆流水线结果
/// </summary>
public class YunxiaoPipelineCloneResult
{
    /// <summary>
    /// 新流水线 ID
    /// </summary>
    public string? PipelineId { get; set; }

    /// <summary>
    /// 新流水线名称
    /// </summary>
    public string? PipelineName { get; set; }

    /// <summary>
    /// 是否已禁用部署阶段
    /// </summary>
    public bool DeployStageDisabled { get; set; }

    /// <summary>
    /// 待人工绑定清单（服务连接 / 主机组 / 构建集群 / 制品仓库等资源字面量）。
    /// 云效 OpenAPI 无「复制流水线」接口，克隆实为 YAML 文本复制，
    /// 官方不做资源重绑定，这些资源需用户在云效确认后再启用部署阶段（PRD §6 #14）。
    /// </summary>
    public List<string> PendingBindings { get; set; } = new();
}

/// <summary>
/// 流水线运行结果
/// </summary>
public class YunxiaoPipelineRun
{
    /// <summary>
    /// 运行 ID
    /// </summary>
    [JsonPropertyName("runId")]
    public string? RunId { get; set; }

    /// <summary>
    /// 流水线 ID
    /// </summary>
    [JsonPropertyName("pipelineId")]
    public string? PipelineId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// 触发者
    /// </summary>
    [JsonPropertyName("triggeredBy")]
    public string? TriggeredBy { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }
}

#endregion