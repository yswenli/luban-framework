using LuBan.AIAgent.ConsoleApp.Entities;
using LuBan.Orm;

namespace LuBan.AIAgent.ConsoleApp.Repositories;

/// <summary>
/// RAG 文件仓储
/// </summary>
public class RagFileRepository : BaseRepository<DbRagFile>
{
    public RagFileRepository(long tenantId = LuBanOrmConst.DefaultTenantId) : base(tenantId) { }
}

/// <summary>
/// RAG 切块仓储
/// </summary>
public class RagChunkRepository : BaseRepository<DbRagChunk>
{
    public RagChunkRepository(long tenantId = LuBanOrmConst.DefaultTenantId) : base(tenantId) { }
}
