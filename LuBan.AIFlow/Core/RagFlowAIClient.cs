using FileInfo = LuBan.AIFlow.Models.File.FileInfo;

namespace LuBan.AIFlow.Core;

/// <summary>
/// RagFlow AI客户端,
/// https://ragflow.io/docs/dev/http_api_reference
/// </summary>
public class RagFlowAIClient : IAIClient
{
    private readonly HttpClientProxy _httpClient;

    /// <summary>
    /// AI客户端选项
    /// </summary>
    public AIOptions Options { get; }

    /// <summary>
    /// 获取认证头部
    /// </summary>
    /// <returns>包含认证信息的头部字典</returns>
    private Dictionary<string, string> GetHeaders()
    {
        return new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {Options.ApiKey}",
            ["Content-Type"] = "application/json"
        };
    }

    /// <summary>
    /// 初始化RagFlowAIClient的新实例
    /// </summary>
    /// <param name="options">AI客户端选项</param>
    public RagFlowAIClient(AIOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));

        // 使用 HttpClientUtil 创建 HTTP 客户端
        _httpClient = HttpClientProxy.Create(options.BaseUrl ?? throw new ArgumentException("BaseUrl cannot be null", nameof(options)));
    }

    /// <summary>
    /// 处理响应错误
    /// </summary>
    /// <param name="response">通用响应</param>
    /// <param name="operation">操作名称</param>
    private void HandleResponseError(CommonResponse response, string operation)
    {
        if (!response.IsSuccess)
        {
            var errorMessage = response.Code switch
            {
                RagFlowErrorCodes.BadRequest => $"无效请求参数: {response.Message}",
                RagFlowErrorCodes.Unauthorized => "未授权访问",
                RagFlowErrorCodes.Forbidden => "访问被拒绝",
                RagFlowErrorCodes.NotFound => "资源未找到",
                RagFlowErrorCodes.InternalServerError => "服务器内部错误",
                RagFlowErrorCodes.InvalidChunkId => "无效的 Chunk ID",
                RagFlowErrorCodes.ChunkUpdateFailed => "Chunk 更新失败",
                102 => response.Message switch
                {
                    "The last content of this conversation is not from user." => "会话最后一条消息不是来自用户",
                    "The agent doesn't exist." => "Agent 不存在",
                    "Agent with title test already exists." => "Agent 标题已存在",
                    _ => response.Message
                },
                RagFlowErrorCodes.OnlyOwnerAuthorized => "只有画布所有者才能执行此操作",
                _ => $"未知错误: {response.Message}"
            };

            throw new Exception($"{operation}失败: {errorMessage}");
        }
    }

    #region Chat Completion Methods

    /// <summary>
    /// 发送聊天补全请求到RAGFlow API（带聊天ID）
    /// </summary>
    /// <param name="chatId">聊天ID</param>
    /// <param name="request">聊天补全请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天补全响应</returns>
    public async Task<dynamic> CreateChatCompletionAsync(string chatId, ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            if (request.Stream)
            {
                //启用流式响应时，使用 SSE 连接
                return await _httpClient.CreateSSEAsync("POST", $"/api/v1/chats_openai/{chatId}/chat/completions", request.ToJson(), headers, 180);
            }
            else
            {
                var response = await _httpClient.PostViewModelAsync<CommonResponse>($"/api/v1/chats_openai/{chatId}/chat/completions", request, headers);
                HandleResponseError(response, $"为聊天 {chatId} 创建补全");
                return response.Data as ChatCompletionResponse ?? throw new Exception("响应数据格式错误");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create chat completion for chat {chatId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 与聊天助手交谈
    /// </summary>
    /// <param name="chatId">聊天助手ID</param>
    /// <param name="question">问题内容</param>
    /// <param name="sessionId">会话ID（可选）</param>
    /// <param name="stream">是否使用流式响应（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天助手响应，包含回答内容和引用信息</returns>
    /// <exception cref="Exception">当请求参数无效或服务器返回错误时抛出</exception>
    public async Task<ChatAssistantResponse> ChatWithAssistantAsync(string chatId, string question, string? sessionId = null, bool stream = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(question))
            {
                throw new ArgumentException("Question is required", nameof(question));
            }

            var request = new
            {
                question,
                stream,
                session_id = sessionId
            };

            var headers = GetHeaders();
            return await _httpClient.PostViewModelAsync<ChatAssistantResponse>($"/api/v1/chats/{chatId}/completions", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to chat with assistant {chatId}: {ex.Message}", ex);
        }
    }

    #endregion

    #region Agent Management Methods

    /// <summary>
    /// 创建Agent
    /// </summary>
    /// <param name="request">创建Agent请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建结果</returns>
    public async Task<AgentInfo> CreateAgentAsync(CreateAgentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PostViewModelAsync<AgentInfo>("/api/v1/agents", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create agent: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新Agent
    /// </summary>
    /// <param name="agentId">Agent ID</param>
    /// <param name="request">更新Agent请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新结果</returns>
    public async Task<AgentInfo?> UpdateAgentAsync(string agentId, UpdateAgentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PutAsync<AgentInfo>($"/api/v1/agents/{agentId}", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update agent {agentId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除Agent
    /// </summary>
    /// <param name="agentId">Agent ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task DeleteAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            await _httpClient.DeleteJsonAsync($"/api/v1/agents/{agentId}", "", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete agent {agentId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取Agent列表
    /// </summary>
    /// <param name="query">查询参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Agent列表</returns>
    public async Task<ListAgentsResponse> ListAgentsAsync(ListAgentsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryString = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(query.Id)) queryString["id"] = query.Id;
            if (!string.IsNullOrEmpty(query.Name)) queryString["name"] = query.Name;
            if (query.Page != null) queryString["page"] = query.Page?.ToString() ?? "1";
            if (query.PageSize != null) queryString["page_size"] = query.PageSize?.ToString() ?? "10";
            if (!string.IsNullOrEmpty(query.OrderBy)) queryString["orderby"] = query.OrderBy;
            if (query.Desc != null) queryString["desc"] = query.Desc?.ToString() ?? "false";

            var headers = GetHeaders();
            return await _httpClient.GetViewModelAsync<ListAgentsResponse>($"/api/v1/agents?{queryString}", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list agents: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 创建Agent补全
    /// </summary>
    /// <param name="agentId">Agent ID</param>
    /// <param name="request">聊天补全请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天补全响应</returns>
    public async Task<ChatCompletionResponse> CreateAgentCompletionAsync(string agentId, ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            var response = await _httpClient.PostViewModelAsync<CommonResponse>($"/api/v1/agents_openai/{agentId}/chat/completions", request, headers);
            HandleResponseError(response, $"为 Agent {agentId} 创建补全");
            return response.Data as ChatCompletionResponse ?? throw new Exception("响应数据格式错误");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create agent completion for agent {agentId}: {ex.Message}", ex);
        }
    }

    #endregion

    #region Dataset Management Methods

    /// <summary>
    /// 创建数据集
    /// </summary>
    /// <param name="request">创建数据集请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建结果</returns>
    public async Task<DatasetInfo> CreateDatasetAsync(CreateDatasetRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PostViewModelAsync<DatasetInfo>("/api/v1/datasets", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create dataset: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新数据集
    /// </summary>
    /// <param name="datasetId">数据集ID</param>
    /// <param name="request">更新数据集请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新结果</returns>
    public async Task<DatasetInfo?> UpdateDatasetAsync(string datasetId, UpdateDatasetRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PutAsync<DatasetInfo>($"/api/v1/datasets/{datasetId}", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update dataset {datasetId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除数据集
    /// </summary>
    /// <param name="datasetId">数据集ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task DeleteDatasetAsync(string datasetId, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            await _httpClient.DeleteJsonAsync($"/api/v1/datasets/{datasetId}", "", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete dataset {datasetId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取数据集列表
    /// </summary>
    /// <param name="query">查询参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>数据集列表</returns>
    public async Task<ListDatasetsResponse> ListDatasetsAsync(ListDatasetsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryString = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(query.Id)) queryString["id"] = query.Id;

            var headers = GetHeaders();
            return await _httpClient.GetViewModelAsync<ListDatasetsResponse>($"/api/v1/datasets?{queryString}", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list datasets: {ex.Message}", ex);
        }
    }

    #endregion

    #region File Management Methods

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="datasetId">数据集ID</param>
    /// <param name="fileStream">文件流</param>
    /// <param name="fileName">文件名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    public async Task<FileInfo?> UploadFileAsync(string datasetId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var formData = new Dictionary<string, object>();
            var tempFilePath = Path.GetTempFileName();
            using (var fileStream2 = File.Create(tempFilePath))
            {
                await fileStream.CopyToAsync(fileStream2);
            }
            var headers = GetHeaders();
            var result = await _httpClient.UploadFileAsync<FileInfo>($"/api/v1/datasets/{datasetId}/files", headers, formData, tempFilePath, cancellationToken);
            File.Delete(tempFilePath);
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to upload file {fileName} to dataset {datasetId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新文件
    /// </summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="request">更新文件请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新结果</returns>
    public async Task<FileInfo?> UpdateFileAsync(string fileId, UpdateFileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PutAsync<FileInfo>($"/api/v1/files/{fileId}", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update file {fileId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件流</returns>
    public async Task<MemoryStream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tempFile = Path.GetTempFileName();
            var headers = GetHeaders();
            await _httpClient.DownLoadFileAsync($"/api/v1/files/{fileId}/download", tempFile, headers, 30);
            var memoryStream = new MemoryStream();
            using (var fileStream = File.OpenRead(tempFile))
            {
                await fileStream.CopyToAsync(memoryStream, cancellationToken);
            }
            File.Delete(tempFile);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download file {fileId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取文件列表
    /// </summary>
    /// <param name="datasetId">数据集ID</param>
    /// <param name="query">查询参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件列表</returns>
    public async Task<ListFilesResponse> ListFilesAsync(string datasetId, ListFilesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryString = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(query.Id)) queryString["id"] = query.Id;

            var headers = GetHeaders();
            return await _httpClient.GetViewModelAsync<ListFilesResponse>($"/api/v1/datasets/{datasetId}/files?{queryString}", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list files for dataset {datasetId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task DeleteFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            await _httpClient.DeleteJsonAsync($"/api/v1/files/{fileId}", "", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete file {fileId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 解析文件
    /// </summary>
    /// <param name="request">解析文件请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task ParseFileAsync(ParseFileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            await _httpClient.PostViewModelAsync<object>("/api/v1/files/parse", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse files: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 停止解析文件
    /// </summary>
    /// <param name="request">停止解析文件请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task StopParseFileAsync(StopParseFileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            await _httpClient.PostViewModelAsync<object>("/api/v1/files/stop-parse", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to stop parsing files: {ex.Message}", ex);
        }
    }

    #endregion

    #region Chunk Management Methods

    /// <summary>
    /// 添加分块
    /// </summary>
    /// <param name="datasetId">数据集ID</param>
    /// <param name="request">添加分块请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>添加结果</returns>
    public async Task<ChunkInfo> AddChunkAsync(string datasetId, AddChunkRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PostViewModelAsync<ChunkInfo>($"/api/v1/datasets/{datasetId}/chunks", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add chunk to dataset {datasetId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取分块列表
    /// </summary>
    /// <param name="datasetId">数据集ID</param>
    /// <param name="query">查询参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>分块列表</returns>
    public async Task<ListChunksResponse> ListChunksAsync(string datasetId, ListChunksQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryString = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(query.Id)) queryString["id"] = query.Id;

            var headers = GetHeaders();
            return await _httpClient.GetViewModelAsync<ListChunksResponse>($"/api/v1/datasets/{datasetId}/chunks?{queryString}", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list chunks for dataset {datasetId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除分块
    /// </summary>
    /// <param name="chunkId">分块ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task DeleteChunkAsync(string chunkId, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            await _httpClient.DeleteJsonAsync($"/api/v1/chunks/{chunkId}", "", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete chunk {chunkId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新分块
    /// </summary>
    /// <param name="chunkId">分块ID</param>
    /// <param name="request">更新分块请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新结果</returns>
    public async Task<ChunkInfo?> UpdateChunkAsync(string chunkId, UpdateChunkRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PutAsync<ChunkInfo>($"/api/v1/chunks/{chunkId}", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update chunk {chunkId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 检索分块
    /// </summary>
    /// <param name="datasetId">数据集ID</param>
    /// <param name="request">检索分块请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>检索结果</returns>
    public async Task<RetrieveChunksResponse> RetrieveChunksAsync(string datasetId, RetrieveChunksRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PostViewModelAsync<RetrieveChunksResponse>($"/api/v1/datasets/{datasetId}/chunks/retrieve", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to retrieve chunks for dataset {datasetId}: {ex.Message}", ex);
        }
    }

    #endregion

    #region Chat Assistant Management Methods

    /// <summary>
    /// 创建聊天助手
    /// </summary>
    /// <param name="request">创建聊天助手请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建结果</returns>
    public async Task<ChatAssistantInfo> CreateChatAssistantAsync(CreateChatAssistantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PostViewModelAsync<ChatAssistantInfo>("/api/v1/chats", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create chat assistant: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新聊天助手
    /// </summary>
    /// <param name="request">更新聊天助手请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新结果</returns>
    public async Task<ChatAssistantInfo?> UpdateChatAssistantAsync(UpdateChatAssistantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PutAsync<ChatAssistantInfo>($"/api/v1/chats/{request.ChatId}", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update chatid {request.ChatId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除聊天助手
    /// </summary>
    /// <param name="assistantId">助手ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task DeleteChatAssistantAsync(string assistantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            await _httpClient.DeleteJsonAsync($"/api/v1/chat-assistants/{assistantId}", "", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete chat assistant {assistantId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取聊天助手列表
    /// </summary>
    /// <param name="query">查询参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天助手列表</returns>
    public async Task<ListChatAssistantsResponse> ListChatAssistantsAsync(ListChatAssistantsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryString = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(query.Id)) queryString["id"] = query.Id;
            if (!string.IsNullOrEmpty(query.Name)) queryString["name"] = query.Name;
            queryString["page"] = query.Page.ToString();
            queryString["page_size"] = query.PageSize.ToString();
            if (!string.IsNullOrEmpty(query.OrderBy)) queryString["orderby"] = query.OrderBy;
            queryString["desc"] = query.Desc.ToString();

            var headers = GetHeaders();
            return await _httpClient.GetViewModelAsync<ListChatAssistantsResponse>($"/api/v1/chat-assistants?{queryString}", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list chat assistants: {ex.Message}", ex);
        }
    }

    #endregion

    #region Session Management Methods

    /// <summary>
    /// 创建会话（带聊天助手）
    /// </summary>
    /// <param name="chatId">聊天助手ID</param>
    /// <param name="request">创建会话请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建结果</returns>
    public async Task<SessionInfo> CreateSessionAsync(string chatId, CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PostViewModelAsync<SessionInfo>($"/api/v1/chats/{chatId}/sessions", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create session for chat {chatId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新会话
    /// </summary>
    /// <param name="chatId">聊天助手ID</param>
    /// <param name="sessionId">会话ID</param>
    /// <param name="request">更新会话请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新结果</returns>
    public async Task<SessionInfo?> UpdateSessionAsync(string chatId, string sessionId, UpdateSessionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PutAsync<SessionInfo>($"/api/v1/chats/{chatId}/sessions/{sessionId}", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update session {sessionId} for chat {chatId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取会话列表
    /// </summary>
    /// <param name="chatId">聊天助手ID</param>
    /// <param name="query">查询参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会话列表</returns>
    public async Task<ListSessionsResponse> ListSessionsAsync(string chatId, ListSessionsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryString = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(query.Id)) queryString["id"] = query.Id;
            if (!string.IsNullOrEmpty(query.Name)) queryString["name"] = query.Name;
            queryString["page"] = query.Page.ToString();
            queryString["page_size"] = query.PageSize.ToString();
            if (!string.IsNullOrEmpty(query.OrderBy)) queryString["orderby"] = query.OrderBy;
            queryString["desc"] = query.Desc.ToString();

            var headers = GetHeaders();
            return await _httpClient.GetViewModelAsync<ListSessionsResponse>($"/api/v1/chats/{chatId}/sessions?{queryString}", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list sessions for chat {chatId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="chatId">聊天助手ID</param>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task DeleteSessionAsync(string chatId, string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            await _httpClient.DeleteJsonAsync($"/api/v1/chats/{chatId}/sessions/{sessionId}", "", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete session {sessionId} for chat {chatId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 会话聊天
    /// </summary>
    /// <param name="chatId">聊天助手ID</param>
    /// <param name="sessionId">会话ID</param>
    /// <param name="request">会话聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会话聊天响应</returns>
    public async Task<SessionChatResponse> SessionChatAsync(string chatId, string sessionId, SessionChatRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PostViewModelAsync<SessionChatResponse>($"/api/v1/chats/{chatId}/sessions/{sessionId}/chat", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to chat with session {sessionId} for chat {chatId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 创建会话（带 Agent）
    /// </summary>
    /// <param name="agentId">Agent ID</param>
    /// <param name="request">创建会话请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建结果</returns>
    public async Task<SessionInfo> CreateAgentSessionAsync(string agentId, CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PostViewModelAsync<SessionInfo>($"/api/v1/agents/{agentId}/sessions", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create session for agent {agentId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 与 Agent 对话
    /// </summary>
    /// <param name="agentId">Agent ID</param>
    /// <param name="sessionId">会话ID</param>
    /// <param name="request">会话聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会话聊天响应</returns>
    public async Task<SessionChatResponse> AgentSessionChatAsync(string agentId, string sessionId, SessionChatRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.PostViewModelAsync<SessionChatResponse>($"/api/v1/agents/{agentId}/sessions/{sessionId}/chat", request, headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to chat with session {sessionId} for agent {agentId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取 Agent 会话列表
    /// </summary>
    /// <param name="agentId">Agent ID</param>
    /// <param name="query">查询参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会话列表</returns>
    public async Task<ListSessionsResponse> ListAgentSessionsAsync(string agentId, ListSessionsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryString = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(query.Id)) queryString["id"] = query.Id;
            if (!string.IsNullOrEmpty(query.Name)) queryString["name"] = query.Name;
            queryString["page"] = query.Page.ToString();
            queryString["page_size"] = query.PageSize.ToString();
            if (!string.IsNullOrEmpty(query.OrderBy)) queryString["orderby"] = query.OrderBy;
            queryString["desc"] = query.Desc.ToString();

            var headers = GetHeaders();
            return await _httpClient.GetViewModelAsync<ListSessionsResponse>($"/api/v1/agents/{agentId}/sessions?{queryString}", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list sessions for agent {agentId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 删除 Agent 会话
    /// </summary>
    /// <param name="agentId">Agent ID</param>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task DeleteAgentSessionAsync(string agentId, string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            await _httpClient.DeleteJsonAsync($"/api/v1/agents/{agentId}/sessions/{sessionId}", "", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete session {sessionId} for agent {agentId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取会话相关问题
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>相关问题列表</returns>
    public async Task<List<string>> GetSessionRelatedQuestionsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = GetHeaders();
            return await _httpClient.GetViewModelAsync<List<string>>($"/api/v1/sessions/{sessionId}/related_questions", headers);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get related questions for session {sessionId}: {ex.Message}", ex);
        }
    }

    #endregion
}