# Agent 对话接口（/api/agent）

三个动作：`chat`（非流式）、`chatstream`（SSE）、`confirm`（工具确认回执）。

- 全部需要 JWT 登录（`[Authorize]`），返回壳沿用框架的 `SuccessResult` / `ErrorResult`。
- 会话归属由服务端 `WebAgentSessionRegistry` 维护：每个用户「最后一次」会话；可显式传 `sessionId`。
- 三个动作均标 `[NoApiLog]`：全局 API 日志会记录请求/响应正文（人设提示词、带签名的附件 URL），且 SSE 不能被缓冲。

## 1. 端点一览

| 方法 | 路径 | 鉴权 | 说明 |
|---|---|---|---|
| POST | `/api/agent/chat` | JWT | 一轮对话，**不装配任何工具**，返回正文 + 思考 + 附件摘要 |
| POST | `/api/agent/chatstream` | JWT | 一轮对话，SSE 事件流，装配工具 |
| POST | `/api/agent/confirm` | JWT | 回传 `tool.confirm` 的确认结果 |

### 请求体（`chat` / `chatstream` 共用）

| 字段 | 必填 | 说明 |
|---|---|---|
| `prompt` | 是 | 用户本轮输入；空白 → 400 `prompt 不能为空` |
| `system` | 否 | 前端人设/规则；`null`/空白 → 用后端 Profile 默认提示词。**不持久化，只作用于本轮** |
| `attachments` | 否 | 附件来源数组；单项可以是 `http(s)://` URL、本机绝对路径（Windows 盘符 / UNC / Linux `/`）、`file://`、相对路径（先查用户工作区，再查站点内容根）；数量上限见配置，超限 → 400 |
| `sessionId` | 否 | 指定会话；不传 → 复用该用户最后一次，没有就新建 |

### 确认请求体（`confirm`）

| 字段 | 说明 |
|---|---|
| `sessionId` | 必填，必须属于当前登录用户 |
| `confirmationId` | 必填，取自 `tool.confirm` 帧的 `confirmationId` |
| `result` | `allow` / `deny` / `allowAll` / `allowSession`（映射 `ConfirmResult`） |

响应 `data`：`{ "resolved": true }`。

## 2. `POST /api/agent/chat`

响应 `data`：

```json
{
  "sessionId": "b1f2c3...",
  "content": "合同主要风险有三点……",
  "thinking": "先看违约责任条款……",
  "attachments": {
    "added": [
      { "source": "https://cdn.example.com/contract-1.png", "fileName": "contract-1.png", "mediaType": "image/png", "fileSize": 81234 }
    ],
    "skipped": [
      { "source": "https://cdn.example.com/notes.txt", "reason": "下载超时（30s）" }
    ]
  },
  "error": null
}
```

行为要点：

- 建 Agent 时 `EnableTools=false`：**本轮不注册任何工具**，模型看不到任何工具；系统提示词也不追加 Skill 目录与工具示例段。
- 部分成功（已产出正文后又出现 `ErrorEvent`）：**200**，`content` 保留已产出部分，`error` 填错误文案。
- 一个字符都没产出就失败：**502** + 错误信息，不带部分内容。
- 文本附件 >50KB 在非流式下直接跳过（`attachments.skipped`，reason 含「50KB」），因为大文本走的是「让模型用 `ReadFileAsync` 自己读」的指引，而本接口没有工具。

错误语义（三条）：

| 场景 | 状态码 |
|---|---|
| `prompt` 空白 | 400 |
| `sessionId` 传了但会话不存在（重启/伪造/已清理） | 404 `会话不存在或已过期`，不静默新建 |
| 无正文的模型异常 | 502 |

## 3. `POST /api/agent/chatstream`

响应 `Content-Type: text/event-stream`，每帧 `event: <type>` + `data: <单行 JSON>`：

```
event: session
data: {"sessionId":"b1f2c3...","created":false}

event: think.delta
data: {"delta":"先看违约责任","llmRound":1}

: ping

event: text.delta
data: {"delta":"合同主要风险有三点","llmRound":2}

event: done
data: {"sessionId":"b1f2c3...","elapsedMs":8421}

```

约定：

- 首帧一定是 `session`（含最终 `sessionId`，`created` 表示本次是否新建）。
- 正常结束、或发生不可恢复错误（先 `error` 再 `done`），最后都发 `done`；客户端断连导致取消时不发（连接已断）。没有「停止生成」接口，`done` 也不带 `cancelled`。
- 心跳：空闲超过 `Api:SseHeartbeatSeconds`（默认 15s）发一行注释 `: ping`，不算事件，前端 `EventSource` 会忽略。
- 前置校验（401 / 403 / 404 / 400 / 503）在切换 `Content-Type` 前按普通 JSON 返回，不会得到半个空 SSE。
- 权限模式固定 `ToolPermissionMode.Default`（`AcceptEdits` / `Bypass` 不开放给 Web）。

### 事件名与载荷（`LuBan.AIAgent.Protocol.AgentEventProtocol`）

| 事件名 | 载荷（camelCase） | 来源 |
|---|---|---|
| `session` | `{ sessionId, created }` | 控制器（传输层） |
| `think.delta` | `{ delta, llmRound }` | `ThinkingDeltaEvent` |
| `text.delta` | `{ delta, llmRound }` | `TextDeltaEvent` |
| `tool.started` | `{ callId, name, arguments, llmRound }` | `ToolCallStartedEvent` |
| `tool.completed` | `{ callId, resultSummary, llmRound }` | `ToolCallCompletedEvent` |
| `tool.failed` | `{ callId, error, llmRound }` | `ToolCallFailedEvent` |
| `tool.confirm` | `{ confirmationId, toolName, arguments, timeoutSeconds }` | `ToolConfirmationRequestedEvent` |
| `attachment.added` | `{ fileName, mediaType, fileSize, source }` | `AttachmentAddedEvent` |
| `attachment.skipped` | `{ source, reason }` | `AttachmentSkippedEvent` |
| `error` | `{ message }` | `ErrorEvent` |
| `done` | `{ sessionId, elapsedMs }` | 控制器（传输层） |

`OrchestrationProgressEvent` 不映射（Web 不启用自动编排），遇到时忽略。

## 4. `POST /api/agent/confirm`

- `result` 只能是 `allow` / `deny` / `allowAll` / `allowSession`，否则 400。
- `confirmationId` 不存在 / 已超时 / 已应答 → 404 `确认请求不存在或已过期`。
- `sessionId` 不属于当前用户 → 403 `无权访问该会话`；会话不存在 → 404。
- 重复提交只有第一次生效，后续 404。
- 本接口**不新建会话**，避免前端拿垃圾 `sessionId` 刷出一堆空会话。

## 5. 必须知道的边界

1. **`chat` 完全没有工具。** 不要说「让它自己读文件」再去调 `chat`：模型看不到 `ReadFileAsync`，只会告诉你它做不到。需要工具请用 `chatstream`。
2. **`Default` 权限模式下工作区内操作不问。** 工作区（`aiagent/users/<userId>`）内的读写按 `ToolConfirmationService.IsAuthorizedPath` 直接放行；只有删除类工具、越界路径、无路径语义的脚本才触发 `tool.confirm`。前端不能假设「写文件一定会弹确认」。
3. **附件来源不做白名单，也不拦私网。** URL 会被直接下载（体积 25MB、超时 30s 是唯二兜底），本地路径会被就地读取。**调用方必须是可信前端**；不要把这个接口开放给不受控的第三方。

## 6. 部署假设

- **单实例或 sticky session**：确认登记簿（`IToolConfirmationBroker`）与 SSE 连接都在进程内。多副本时需要共享登记簿（如 Redis）并把同一会话的请求路由到同一副本，本次未实现。
- 会话归属（`WebAgentSessionRegistry`）与 `ISessionManager` 默认实现都是进程内单例，重启后旧 `sessionId` 一律 404。

## 7. 相关配置（`appsettings.json`）

```jsonc
"LuBanAgent": {
  "WorkspaceRoot": "aiagent",
  // 每个登录用户一个工作区：<WorkspaceRoot>/users/<userId>
  "IsolateWorkspaceByUser": true,
  "Attachments": {
    "Enabled": true,
    "MaxCount": 5,
    "MaxFileBytes": 26214400,   // 25MB，单文件硬上限
    "TimeoutSeconds": 30,       // 单个 URL 下载超时
    "RootDirectory": ".luban-agent/attachments",
    "ContentRoot": "wwwroot"    // 相对路径第二基准；由 LuBan.Web.Core 归一化为绝对路径
  },
  "Confirmation": {
    "TimeoutSeconds": 60,       // 超时按拒绝处理
    "BrokerEnabled": true
  }
},
"Api": {
  "SseHeartbeatSeconds": 15     // ≤0 关闭心跳
}
```

## 8. 验收记录（设计 §11 的 18 条）

| # | 验收点 | 结果 | 备注 |
|---|---|---|---|
| 1 | `chat` 带 `system` 生效 | ☐ | |
| 2 | `chat` 三类附件（URL / 绝对路径 / 相对路径）均 added | ☐ | 检查 `aiagent/users/<userId>/.luban-agent/attachments/...` |
| 3 | 404 URL / 不存在路径 / `.pdf` 均 skipped 且正文正常 | ☐ | |
| 4 | 不传 `sessionId` 两次 → 第二次 `created=false` | ☐ | |
| 5 | `chatstream` 帧序 `session` → `*.delta` → `done`，长任务有 `: ping` | ☐ | |
| 6 | 触发 `tool.confirm` → `confirm` allow → `tool.completed` | ☐ | 用 `DeleteFileAsync`，工作区内写文件不弹确认 |
| 7 | 60s 不答 → `tool.failed`（拒绝），模型继续，`done` | ☐ | |
| 8 | A 用户传 B 的 `sessionId` → 403；确认别人 `confirmationId` → 403/404 | ☐ | |
| 9 | 附件数超 `MaxCount` → 400 且不产生 Agent 调用 | ☐ | |
| 10 | Agent 未启用 → 两个接口 503，`chatstream` 返回 JSON | ☐ | |
| 11 | 客户端中途断开 → 本轮取消，未答确认自动拒绝，无残留 pending | ☐ | |
| 12 | 不存在的 `sessionId` → 404；不传再调 → 200 新会话 | ☐ | |
| 13 | 同会话并发两次 `chatstream`（不同 `system`）→ 均正常结束 | ☐ | |
| 14 | 日志里搜不到 `system` 全文与附件来源 | ☐ | 对应 `[NoApiLog]` |
| 15 | 用户 A 的 `ListDirectory` 只看到 `users/<A>`；读 `users/<B>` 被拒或要求确认 | ☐ | |
| 16 | CLI 不开 `IsolateWorkspaceByUser` 行为不变 | ☐ | |
| 17 | `chat` 下模型看不到任何工具（提示词无 Skill 目录 / 工具段，`tools.Count == 0`） | ☐ | 反目标级要求 |
| 18 | 60KB 文本：`chat` → skipped；`chatstream` → `ReadFileAsync` 指引 | ☐ | 50KB 以下内联 |