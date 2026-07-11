
namespace LuBan.AIFlow.Models;

/// <summary>
/// RAGFlow 错误代码常量
/// </summary>
public static class RagFlowErrorCodes
{
    /// <summary>
    /// 成功
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// 无效请求参数
    /// </summary>
    public const int BadRequest = 400;

    /// <summary>
    /// 未授权访问
    /// </summary>
    public const int Unauthorized = 401;

    /// <summary>
    /// 访问被拒绝
    /// </summary>
    public const int Forbidden = 403;

    /// <summary>
    /// 资源未找到
    /// </summary>
    public const int NotFound = 404;

    /// <summary>
    /// 服务器内部错误
    /// </summary>
    public const int InternalServerError = 500;

    /// <summary>
    /// 无效的 Chunk ID
    /// </summary>
    public const int InvalidChunkId = 1001;

    /// <summary>
    /// Chunk 更新失败
    /// </summary>
    public const int ChunkUpdateFailed = 1002;

    /// <summary>
    /// 只有画布所有者才能执行此操作
    /// </summary>
    public const int OnlyOwnerAuthorized = 103;
}