/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1
*文件名： Program.cs
*版本号： V1.0.0.0
*唯一标识：0265b347-46eb-4ab0-b21f-c52235315670
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：Program 类
*
*=================================================
*修改标记
*修改时间：2026/10/26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：最小 Agent 端点验证宿主可用性
*
*****************************************************************************/
using LuBan.AIAgent;
using LuBan.AIAgent.Hosting;
using LuBan.AIAgent.Sessions;

var builder = WebApplication.CreateBuilder(args);

// ===== 最小 Agent 示例：三行注册 =====
builder.Services.AddLuBanAgentDefaults(builder.Configuration);
builder.Services.AddLuBanAgentHost();
builder.Services.AddLuBanAgentOpenAI(builder.Configuration);

var app = builder.Build();

// ===== 最小 Agent 端点 =====
app.MapPost("/agent/minimal", async (IAgentHost host, ISessionManager sessions, string prompt, CancellationToken ct) =>
{
    var session = await sessions.CreateSessionAsync("minimal", null);
    await sessions.SetCurrentSessionAsync(session.SessionId);
    await host.InitializeAsync(session.SessionId, ct);

    var sb = new System.Text.StringBuilder();
    await foreach (var evt in host.RunStreamingAsync(new AgentRunRequest { Input = prompt, SessionId = session.SessionId }, ct))
    {
        if (evt is TextDeltaEvent delta) sb.Append(delta.Delta);
    }
    return Results.Text(sb.ToString());
});

app.Run();