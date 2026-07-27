# LuBan AI Agent Console App

基于 Microsoft Agent Framework 的 AI Agent 命令行工具。

## 快速开始

### 1. 安装 Playwright 浏览器（使用浏览器工具前必须安装）

```powershell
# 安装与 Microsoft.Playwright 1.61.0 匹配的浏览器版本
npx playwright@1.61.0 install chromium
```

**注意**：浏览器版本必须与 Microsoft.Playwright 包版本匹配，当前项目使用的是 1.61.0。

### 2. 运行程序

```bash
dotnet run --project LuBan.AIAgent.ConsoleApp
```

### 3. 使用交互功能

命令行支持：
- **Tab 键自动完成** - 输入部分命令后按 Tab 自动补全
- **上/下箭头** - 浏览历史命令
- **Esc 键** - 清除当前输入
- **实时状态显示** - 执行时显示动画和进度信息

```
请输入命令: sel<Tab>    # 自动补全为 "select"
请输入命令: <上箭头>   # 显示上一条命令
```

### 实时状态显示

在执行 AI 对话时，会显示动态旋转动画和实时状态：

```
你: 帮我查一下D盘下面有哪些目录

⠋ 正在调用工具: ListDirectoryAsync
⠙ 正在思考...
⠹ 正在生成回答...
```

状态信息包括：
- 正在思考...
- 正在调用工具: {工具名}
- 工具执行完成，正在生成回答...
- 生成回答完成

### 3. 配置 Provider

```
> add-provider
选择 Provider 类型: 1
请输入 OpenAI API Key: sk-xxx
Provider 'OpenAI' 已添加并保存
  支持的模型: gpt-4.1, gpt-4.1-mini, gpt-4.1-nano, gpt-4o...
```

### 4. 选择模型

```
> select
已配置的 Provider:
  1. OpenAI

请选择 Provider 编号: 1
OpenAI 支持的模型:
  1. gpt-4.1
  2. gpt-4.1-mini
  3. gpt-4.1-nano
  4. gpt-4o
  5. gpt-4o-mini
  ...

请选择模型编号: 4
已选择模型: openai:gpt-4o
```

### 5. 使用浏览器工具

```
> browse
请输入目标网站 URL: https://www.baidu.com
指令: 导航到搜索页面
指令: 在搜索框 #kw 输入关键词 lubanframework
指令: 点击搜索按钮 #su
```

## 支持的 Provider

| Provider | 显示名称 | 支持的模型 |
|----------|---------|-----------|
| openai | OpenAI | gpt-4.1, gpt-4o, gpt-4-turbo, o1, o3-mini 等 |
| azure | Azure OpenAI | gpt-4o, gpt-4-turbo, gpt-35-turbo 等 |
| deepseek | DeepSeek | deepseek-chat, deepseek-coder, deepseek-reasoner |
| kimi | Kimi | k3, k3-256k, kimi-for-coding, kimi-for-coding-highspeed |
| glm | 智谱 GLM | glm-4-plus, glm-4-air, glm-4-flash 等 |
| qwen | 通义千问 | qwen-turbo, qwen-plus, qwen-max 等 |
| doubao | 豆包 | doubao-pro-4k, doubao-pro-32k, doubao-lite-4k 等 |
| claude | Claude | claude-3-5-sonnet, claude-3-5-haiku, claude-3-opus 等 |
| gemini | Google Gemini | gemini-2.0-flash, gemini-1.5-pro, gemini-1.5-flash 等 |
| ollama | Ollama (本地) | llama3.1, llama3.2, qwen2.5, deepseek-coder-v2 等 |

## 命令列表

| 命令 | 说明 |
|-----|-----|
| `add-provider` | 添加 AI Provider |
| `list` | 列出所有 Provider |
| `select` | 选择模型 |
| `browse` | 用自然语言操作网站 |
| `chat` | 智能对话（支持工具调用和会话保存） |
| `session` | 管理对话会话 |
| `skill` | 查看和执行 Skill（技能） |
| `rule` | 查看和管理规则 |
| `mcp` | 查看 MCP 客户端 |
| `clear` | 清除配置 |
| `exit` | 退出程序 |

## Session 系统

Session（会话）系统用于保存和管理对话历史，使用 SQLite 数据库存储。

### 数据库位置

```
%LocalAppData%\LuBan\AIAgent\ai_sessions.db
```

### 会话管理命令

```bash
# 查看当前会话和操作菜单
> session

# 创建新会话
> session new

# 列出历史会话
> session list

# 修改会话标题
> session title

# 查看会话统计
> session stats

# 清除会话消息
> session clear

# 删除会话
> session delete
```

### 会话自动保存

在 `chat` 命令中，对话会自动保存到当前会话：
- 用户消息自动保存
- AI 回复自动保存
- Token 数量自动统计

### 数据库表结构

**ai_session 表**
| 字段 | 类型 | 说明 |
|-----|------|------|
| Id | INTEGER | 主键 |
| SessionId | TEXT | 会话ID |
| UserId | TEXT | 用户ID |
| Title | TEXT | 会话标题 |
| MessageCount | INTEGER | 消息数 |
| TotalTokens | INTEGER | Token总数 |
| CreateTime | DATETIME | 创建时间 |
| UpdateTime | DATETIME | 更新时间 |

**ai_session_message 表**
| 字段 | 类型 | 说明 |
|-----|------|------|
| Id | INTEGER | 主键 |
| SessionId | TEXT | 会话ID |
| Role | TEXT | 角色（user/assistant） |
| Content | TEXT | 消息内容 |
| Tokens | INTEGER | Token数 |
| CreateTime | DATETIME | 创建时间 |

## Rule 系统

规则系统用于在工具执行前进行权限检查和参数修改。

### 查看规则

```
> rule

已配置的规则：

  ✅ path-access          - 路径访问规则
     限制文件系统访问，防止访问敏感路径
     优先级: 100
```

### 内置规则

| 规则 ID | 说明 |
|---------|------|
| `path-access` | 路径访问规则，限制文件系统访问范围 |

### 规则工作原理

1. 在工具执行前自动评估所有适用规则
2. 按优先级从高到低执行
3. 如果规则拒绝，立即停止执行
4. 规则可以修改参数

## MCP 系统

MCP (Model Context Protocol) 是一个标准协议，用于 AI 连接外部工具和数据源。

### 查看 MCP 客户端

```
> mcp

MCP (Model Context Protocol) 客户端：

  ⚪ 未连接 filesystem
     文件系统操作工具
```

### 连接 MCP 客户端

```
> mcp connect filesystem
正在连接 filesystem...
✅ 连接成功
```

### 查看可用工具

```
> mcp tools filesystem

filesystem 可用的工具：
  - read_file: 读取文件内容
  - write_file: 写入文件内容
  - list_directory: 列出目录内容
```

### 内置 MCP 客户端

| 客户端 | 说明 |
|-------|------|
| `filesystem` | 文件系统操作（读写文件、列出目录） |

## Skill 系统

Skill（技能）是预定义的 AI 能力模式，帮助用户更高效地完成特定任务。

### 内置 Skill

| Skill ID | 名称 | 分类 | 说明 |
|---------|------|------|------|
| `brainstorming` | 头脑风暴 | creative | 实现功能前探索需求和设计 |
| `code-review` | 代码审查 | development | 审查代码、发现问题、提供改进建议 |
| `documentation` | 文档生成 | productivity | 生成代码注释、README、API 文档等 |

### 使用 Skill

```
# 列出所有 Skill
> skill

# 执行 Skill（交互式）
> skill brainstorming

# 带 parameters 执行 Skill
> skill brainstorming 我想实现一个用户登录功能

# 简写
> skill code-review 检查这段代码有没有问题：public void Test() { }
```

### Skill 输出示例

```
执行 Skill: 头脑风暴

📋 需求理解：
用户想要实现一个用户登录功能，可能涉及：
- 用户身份验证
- 会话管理
- 安全机制

💡 实现方案：
1. 基于 JWT 的无状态认证
   - 优点：可扩展、跨平台
   - 缺点：Token 无法主动失效

2. 基于 Session 的有状态认证
   - 优点：安全性高、可控
   - 缺点：需要服务器存储

3. OAuth2 第三方登录
   - 优点：用户体验好
   - 缺点：依赖第三方服务

❓ 需要澄清的问题：
1. 需要支持哪些登录方式？（账号密码、手机、第三方）
2. 是否需要记住我功能？
3. 并发登录如何处理？

✅ 推荐方案：
推荐使用 JWT + Refresh Token 方案...
```

## 智能对话 (chat 命令)

`chat` 命令提供智能对话功能，AI 会自动判断是否需要使用工具来完成任务。

### 支持的工具

- 📁 **文件系统**: 读取文件、列出目录、写入文件等
- 🔧 **脚本执行**: 执行命令行脚本
- 🌐 **浏览器**: 自然语言操作网页（browse 命令专用）
- 🗄️ **数据库**: 执行 SQL 查询（需配置连接）
- 🔴 **Redis**: Redis 命令操作（需配置连接）
- 🌍 **Web 请求**: HTTP API 调用

### 示例对话

```
你: 帮我查一下D盘下面有哪些目录

[调用工具]: list_directory
  参数 path: D:\
[工具结果]: Program Files, Users, Windows, ...

AI: D盘下有以下目录：
1. Program Files
2. Users  
3. Windows
...
```

```
你: 帮我读取 C:\temp\test.txt 文件的内容

[调用工具]: read_file
  参数 path: C:\temp\test.txt
[工具结果]: 文件内容...

AI: 文件内容如下：
...
```

### 工具调用可视化

对话过程中会显示：
- **[调用工具]**: 显示工具名称和参数（青色）
- **[工具结果]**: 显示工具返回结果（绿色）
- **[思考]**: 显示 AI 的思考过程（灰色，如果模型支持）

### 危险操作确认

对于危险操作（写入文件、执行脚本等），系统会在执行前请求用户确认：

```
你: 帮我写一个文件到 C:\temp\test.txt

⚠️  危险操作请求: WriteFileAsync
参数:
  path: C:\temp\test.txt
  content: 文件内容...

是否执行此操作？(y/N): y
✓ 已确认执行

[调用工具]: WriteFileAsync
[工具结果]: 已写入文件 C:\temp\test.txt

AI: 已成功写入文件...
```

需要确认的操作包括：
- 📝 **文件系统**: 写入文件、删除文件、创建/删除目录
- 🔧 **脚本执行**: 执行 Shell、Lua、Python 脚本
- 🗄️ **数据库**: INSERT、UPDATE、DELETE 操作
- 🔴 **Redis**: SET、DELETE、FLUSHDB 操作

用户可以：
- 输入 `y` 或 `yes` 确认执行
- 输入 `n`、`no` 或直接回车取消执行

## 配置文件

配置保存在 `%LocalAppData%\LuBan\AIAgent\config.json`

## 浏览器工具说明

浏览器工具基于 Microsoft.Playwright 实现，支持以下操作：

| 工具 | 说明 |
|-----|-----|
| `NavigateAsync` | 导航到指定 URL |
| `ClickAsync` | 点击页面元素 |
| `TypeTextAsync` | 在输入框中输入文本 |
| `ScreenshotAsync` | 截取页面截图 |
| `GetContentAsync` | 获取页面内容 |
| `WaitForSelectorAsync` | 等待元素出现 |
| `GetCurrentUrlAsync` | 获取当前页面 URL |