[中文](README.md) | English

# LuBan.AIAgent

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> An AI Agent library built on Microsoft Agent Framework — giving LLMs the ability to think, plan, use tools, and execute autonomously.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.AIFlow](../LuBan.AIFlow/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why Do You Need It?

- Want LLMs to call tools to complete tasks, but MCP / Function Calling implementation details are overwhelming?
- Skill management, tool registration, and session persistence each require separate implementations — high maintenance cost?
- Switching between model providers is difficult — moving from Provider A to Provider B requires rewriting large amounts of code?
- Missing middleware mechanism — logging, policy control, and permission interception are hard to extend?

LuBan.AIAgent provides complete AI Agent infrastructure — from Agent runtime, multi-model routing, skill system, tool system, session storage to middleware pipeline — ready to use out of the box.

## Quick Preview

### Option 1: Ready to use (recommended)

`AddLuBanAgentDefaults` layers default (in-memory) implementations of session context, local memory store, and session manager on top of `AddLuBanAgent`, so a minimal host runs out of the box:

```csharp
// Register services (three lines to take off)
services.AddLuBanAgentDefaults(configuration, sp => CreateChatClient());

// Create Agent
using var scope = serviceProvider.CreateScope();
var factory = scope.ServiceProvider.GetRequiredService<ILuBanAgentFactory>();
var agent = await factory.CreateAsync(
    systemPrompt: "You are a browser automation assistant",
    toolGroups: new[] { "browser" });

// Execute task
var response = await agent.RunAsync("Open Baidu and search for LuBan Framework");
Console.WriteLine(response.Text);
```

> All defaults are registered with `TryAdd`, so **registering your own implementation first overrides them**.
> The defaults do not persist (data is lost on process exit). For production, register your own
> `ISessionManager`, `ILocalMemoryStore`, `ISessionContext`, etc.

### Option 2: Framework parts only (host implements the contracts)

```csharp
services.AddSingleton<IChatClient>(sp => CreateChatClient());
services.AddLuBanAgent(configuration);
```

With this option the host must implement `ISessionContext` (required by PathGuard and every tool group),
`ILocalMemoryStore` (required by the localmemory tool group), and other contracts; otherwise resolving
the related services throws `InvalidOperationException`.

## Tech Stack

| Component | Description |
|-----------|-------------|
| Microsoft.Agents.AI.Foundry | Agent runtime framework |
| Microsoft.Extensions.AI | Unified chat client abstraction |
| Microsoft.Playwright | Browser automation engine |
| LuBan.DI | Dependency injection integration |
| LuBan.Common | Base interfaces and utility definitions |

## Installation

```bash
dotnet add package LuBan.AIAgent
```

Install Playwright browsers (required for browser tools):

```powershell
npx playwright@1.61.0 install chromium
```

## Feature Overview

### Core Engine

| Component | Description |
|-----------|-------------|
| `LuBanAgent` | Agent instance, wraps ChatClientAgent, supports sync/streaming execution |
| `ILuBanAgentFactory` / `LuBanAgentFactory` | Agent factory, creates agents with configured tools |
| `IAppConfigReader` | Application config read-only interface (providers / skills / rules / MCP / connector declarations) |
| `IProviderRouter` | Provider routing interface |
| `TextUtils` | Text processing utilities |
| `WildcardMatcher` | Wildcard matching |
| `SkillMdParser` | SKILL.md parser |

### Component Registry Architecture

Skills, MCPs, and Rules use a unified three-tier priority registry pattern:

| Priority | Source | Behavior |
|----------|--------|----------|
| Highest | Hardcoded (DI) | Always present, can be disabled via `DisabledBuiltin` config |
| Medium | Workspace files | Add new items, same-name items are ignored |
| Lowest | config.json | Add new items, same-name items are ignored |

**Loading timing**:
- At startup: Load hardcoded components + config.json global config
- On workspace switch: Load workspace files, auto-merge

**Workspace directory structure**:
```
.luban-agent/
├── skills/          # Workspace-level Skills
│   └── my-skill/
│       └── SKILL.md
├── mcps/            # Workspace-level MCP servers
│   └── my-mcp.json
└── rules/           # Workspace-level rules
    └── my-rule.json
```

### Tool System

| Component | Description |
|-----------|-------------|
| `ILuBanToolPlugin` | Tool plugin interface, defines tool groups and provides tool functions |
| `ToolPluginRegistry` | Tool plugin registry, manages enable/disable and group filtering |
| `ToolAttribute` | Tool annotation attribute |

### Built-in Tools

| Tool Group | Group Name | Core Capabilities |
|------------|------------|-------------------|
| **Browser Tools** | `browser` | Navigate, click, type, screenshot, get content, wait for selector, get URL (Playwright-based) |
| **File System Tools** | `filesystem` | Read files, write files, edit files (exact string replacement, single/multi/replace-all), list directories, delete files, delete directories, search files (glob), content search (regex), create directories, copy files, move files, get file info; allowed directly inside the workspace, paths outside the workspace are allowed after user confirmation |
| **Script Tools** | `script` | Execute Shell (adaptive runtime), Lua (embedded sandbox, no external interpreter), Python scripts |
| **Git Tools** | `git` | 26 fine-grained tools covering status/diff/log/branch/stash/commit/merge/rebase/push, etc.; confirmed-free inside the workspace, per-call confirmation when crossing the boundary; on conflict it returns a file list for the agent to resolve manually |
| **Web Tools** | `web` | Send HTTP requests to fetch web content |
| **Retrieval Tools** | `retrieval` | Index local code/documents, semantic search |
| **Context Compaction** | `context` | Compact conversation history to free token budget (LLM-accessible, callable on demand) |
| **Local Memory Tools** | `localmemory` | Long-term memory storage, query, and management |
| **MCP External Tools** | `mcp` | Merge tools from connected MCP clients (built-in file-system client + hot-loaded external servers) into the Agent tool registry |
| **Orchestration Tools** | `orchestration` | Expose `plan_task` / `run_orchestration` so the model itself detects composite tasks and dispatches SubAgents to run a DAG (opt-in, requires `Orchestration.Enabled=true`) |
| **Wiki Knowledge Base Tools** | `wiki` | Read/write/delete wiki pages, vector search, health check (opt-in, must be named in `ToolGroups`) |
| **Log Analysis Tools** | `log` | Aggregate overview, regex search (filter by level/time/line range), and line slicing for large log files with constant memory (enabled by default, non-opt-in) |
| **Skill Consumption Tools** | `skill` | List available Skills (with keyword search), load and use Skill prompt templates; confirmation-free within the workspace root (enabled by default) |
| **Skill Management Tools** | `skillmanage` | Create/overwrite/delete workspace-level Skills; built-in Skills cannot be overwritten or deleted; write/delete operations require user confirmation (enabled by default) |
| **MCP Management Tools** | `mcpmanage` | Create/overwrite/delete workspace-level MCP configurations, test MCP connections; built-in MCPs cannot be overwritten or deleted; write/delete operations require user confirmation (enabled by default) |

> **Script execution notes**: The Shell tool auto-detects the runtime (on Windows `pwsh` > `powershell` > `cmd`; on Unix-like `bash` > `sh`), adapts argument style and quoting to the interpreter, and returns the actual `shell`/`shellPath`/`platform` in the result. The Lua tool runs on an embedded MoonSharp soft sandbox with no external `lua` interpreter required; the sandbox has no file-system or system-command access, and results are emitted via `print`.

### LLM Wiki Knowledge Base (opt-in)

`wiki` is an opt-in tool group: it is not included when `ToolGroups` is `null` (meaning "all"); it is injected only when `"wiki"` is explicitly named in `ToolGroups`.

| Tool | Description | Confirmation |
|------|-------------|--------------|
| `ReadIndexAsync` | Read `index.md` to see the existing page list | No |
| `ReadPageAsync` | Read a page body (path relative to the wiki root) | No |
| `SavePageAsync` | Write/update a page, auto-maintaining `index.md`, `log.md`, and the vector index | Yes |
| `DeletePageAsync` | Delete a page and remove its index entry and vector index | Yes |
| `SearchAsync` | Vector search within the wiki, optionally falling back to raw workspace files | No |
| `LintAsync` | Check orphan pages, dead links, unindexed pages, missing/outdated sources | No |

**Directory layout**:

```
wiki/
├── SCHEMA.md        # Maintenance schema (carries LLM authoring conventions)
├── index.md         # Page list (maintained by the service; do not edit by hand)
├── log.md           # Change log (maintained by the service; do not edit by hand)
├── overview.md      # Overview
├── sources/         # Source summaries, one page per source
├── entities/        # Entities
├── concepts/        # Concepts
└── queries/         # Saved Q&A
```

**Frontmatter convention**: every page starts with YAML frontmatter containing `title`, `type` (`source` | `entity` | `concept` | `overview` | `query`), `tags`, `sources` (source paths relative to the workspace root), `created`, and `updated`.

**Supported source formats**:

| Extension | Handling |
|-----------|----------|
| `.txt` `.log` `.ini` `.cfg` `.toml` `.properties` | Plain text, read directly |
| `.md` `.markdown` | Markdown parsing |
| `.json` `.jsonc` `.jsonl` `.ndjson` | JSON pretty-printing / per-line pretty-printing |
| `.csv` `.tsv` | Delimited table parsing |
| `.xml` `.xaml` `.svg` | Structured XML extraction |
| `.html` `.htm` | Regex tag stripping, degraded to plain text (no HtmlAgilityPack, no guarantee of full Markdown structure) |
| `.xlsx` | Fully supported (read per sheet, via MiniExcel; headers are read by column index, blank headers fall back to `Column A`, and missing headers never drop columns) |
| `.xls` | Not supported: MiniExcel has no BIFF support; the extension is no longer registered (never indexed, avoiding guaranteed failures) |
| Other unregistered extensions | Falls back to plain-text reading when the content is text; binary content (containing NUL bytes) is rejected with `NotSupportedException` so garbage never enters the context |

`|` inside a cell is escaped to `\|` and newlines become `<br>`, keeping the generated Markdown table intact.

**Configuration** (`LuBanAgent:Tools:Wiki`): `Enabled` (default `true`), `TopK` (default `8`), `IncludeRawDefault` (default `false`), `MaxResultChars` (default `8000`).

**Background indexing and explicit workspace selection**:

- `IRetrievalService.IndexDirectoryAsync(path, glob, force, IProgress<IndexProgress>? progress, cancellationToken, workspaceId)`: indexing no longer holds a method-level write lock (the lock only wraps store writes), embedding calls are serialized (internal semaphore), and progress is reported through `IndexProgress` with `IndexStage.Scanning / Embedding / Deleting / Done` — callers can run it as a background task and surface progress.
- All retrieval and wiki methods take an optional `string? workspaceId = null` to target a workspace explicitly (root resolved via `IWikiContext.WorkspaceRootFor(workspaceId)`); when omitted, `IWikiContext.WorkspaceRoot` is used.
- Semantic search now computes cosine similarity over all candidates (no sampling) and orders by descending `Score` then ascending `ChunkId`, making equal-score results reproducible.
- **Breaking changes**: all 8 `IVectorStore` methods now take `string? workspaceId = null`, and the 4th parameter of `IRetrievalService.IndexDirectoryAsync` changed to `IProgress<IndexProgress>?`. Custom implementers must update their signatures (behavior stays on the default workspace when `workspaceId` is not passed).
- **Indexing whitelist (behavior change)**: `ChunkerFactory.ShouldIndex` decides by extension only; the whitelist is the union of all registered `ICodeChunker` extensions plus `.txt/.csv/.tsv/.log/.properties/.xlsx`. `.xlsx` is extracted to Markdown via `ExcelExtractor` (MiniExcel) before chunking/embedding (the extraction is `## <sheet name>` plus markdown tables, chunked per sheet with language id `excel`); plain-text extensions without a dedicated chunker (`.txt/.csv/.tsv/.log/.properties`) fall back to the sliding window. Extensions outside the whitelist (`.docx/.pdf/.ps1/.exe`, etc.) and files without an extension are never indexed; single-file indexing (`IndexFileAsync`) is subject to the same whitelist and size limit. Whitelisted content that contains NUL bytes or is entirely blank is skipped at read time. Scanning no longer pre-reads file contents (binary detection moved to read time) and prunes excluded directories (`.git/bin/obj/node_modules/dist/packages/.vs/.idea/target`) and reparse points, avoiding long stalls on cloud/network folders caused by file hydration and antivirus scanning.
- `ChunkerFactory.EnumerateFiles(root, pattern)` is the public safe per-level enumerator (prunes excluded directories and reparse points, skips inaccessible levels without aborting, de-duplicates across patterns); it is shared by indexing, wiki page enumeration (`WikiService.EnumeratePages`), and the CLI/desktop pre-scan so every path counts identically.
- **Spreadsheet extraction limit and truncation warnings**: `ExcelExtractor.ExtractAsync(filePath, maxChars, cancellationToken)` accepts a per-file character limit; the indexing path uses `ExcelExtractor.IndexingMaxChars` (2,000,000 chars, far wider than the ingest-side `TextExtractor.MaxChars` = 200,000). On overflow the content is truncated and recorded in `IndexReport.Warnings` instead of being dropped silently. Extraction runs on the thread pool and can observe cancellation while awaiting.
- During enumeration `IndexDirectoryAsync` reports `IndexStage.Scanning` per directory (`IndexProgress.CurrentFile` is the current directory, `Total` is the number of indexable files found so far), so callers can show “Scanning: &lt;directory&gt; (N files found)…” without an extra pre-scan.

### Skill System

| Component | Description |
|-----------|-------------|
| `ISkill` | Skill interface definition, includes `PromptTemplate` property for in-conversation activation |
| `SkillBase` | Skill base class, provides logging, status updates, Agent invocation |
| `SkillRegistry` | Skill registry, manages built-in, file-based, and custom Skills |
| `SkillLoader` | Skill file loader, loads Skill definitions from SKILL.md files |
| `FileSkill` | File-based Skill adapter, wraps SKILL.md files as ISkill |
| `CustomSkill` | Custom Skill adapter, wraps CustomSkillConfig as ISkill |

### Built-in Skills

| Skill ID | Name | Category | Description |
|----------|------|----------|-------------|
| `brainstorming` | Brainstorming | creative | Explore requirements and design before implementing features |
| `code-review` | Code Review | development | Review code, find issues, provide improvement suggestions |
| `documentation` | Documentation | productivity | Generate code comments, README, API docs, etc. |
| `code-refactor` | Code Refactoring | development | Refactor code to improve quality |
| `test-generation` | Test Generation | development | Automatically generate unit tests |
| `code-explain` | Code Explanation | development | Explain complex code logic |
| `debug-assistant` | Debug Assistant | development | Assist with debugging issues |
| `git-commit` | Git Commit | productivity | Generate standardized Git commit messages |
| `find-skills` | Skill Discovery | meta | Automatically discover and recommend suitable skills |
| `agents-md-generator` | Agents.md Spec Generator | productivity | Generate workspace AGENTS.md meta-description document |

### File-based Skills

Support defining custom Skills via SKILL.md files, compatible with OpenCode format:

```markdown
---
name: my-skill
description: "Skill description"
category: custom
---

# Skill Instructions

Prompt template content here...
```

**Storage Locations** (by priority):
- Project-level: `<workspace>/.luban-agent/skills/<skill-id>/SKILL.md`
- User-level: `%LocalAppData%/LuBan/AIAgent/skills/<skill-id>/SKILL.md`

**Priority**: Hardcoded (DI) > Workspace files > config.json

### Conversational Skill/MCP Management

Create, use, and manage Skills and MCP servers through natural language conversation without manual config editing.

**Core Capabilities**:

| Feature | Tool | Description |
|---------|------|-------------|
| List Skills | `ListSkillsAsync` | List all available Skills, with optional keyword search, sorted by Id |
| Use Skill | `UseSkillAsync` | Load a specific Skill's prompt template, auto-inject into current conversation |
| Create Skill | `WriteSkillAsync` | Create or overwrite a workspace-level Skill, generating SKILL.md file |
| Delete Skill | `DeleteSkillAsync` | Delete a workspace-level Skill |
| Create MCP | `WriteMcpServerAsync` | Create or overwrite a workspace-level MCP server config, generating JSON file |
| Delete MCP | `DeleteMcpServerAsync` | Delete a workspace-level MCP config |
| Test MCP | `TestMcpServerAsync` | Test MCP server connection, without persisting the configuration |

**Usage Examples** (natural language conversation):

```
User: "List all available Skills"
AI: [calls ListSkillsAsync] There are 10 built-in Skills available...

User: "Create a Skill named code-review-v2, described as code quality review assistant"
AI: [calls WriteSkillAsync] Created Skill 'code-review-v2', available in the next conversation turn

User: "Add an MCP server named weather-api, command is node, arguments are weather-server.js"
AI: [calls WriteMcpServerAsync] Created MCP configuration, connecting...
```

**Security Protections**:

- Built-in Skills/MCPs cannot be overwritten or deleted
- Skill IDs must match `^[a-z0-9][a-z0-9_-]{0,63}$` format
- Single SKILL.md file size limit: 256KB
- Write/delete operations require user confirmation (recorded as plan items in Plan mode)
- Changes take effect in the next conversation turn (delayed reload mechanism)

**Storage Locations**:

- Skill: `<workspace>/.luban-agent/skills/<skill-id>/SKILL.md`
- MCP: `<workspace>/.luban-agent/mcps/<mcp-name>.json`

**Configuration Options**:

```json
{
  "LuBanAgent": {
    "Tools": {
      "Skill": {
        "Enabled": true,
        "CatalogEnabled": true,
        "CatalogMaxEntries": 50
      },
      "SkillManage": {
        "Enabled": true,
        "MaxSkillFileBytes": 262144
      },
      "McpManage": {
        "Enabled": true
      }
    },
    "Mcp": {
      "ConnectTimeoutSeconds": 10
    }
  }
}
```

**Auto-Connection**: On Agent startup, all external MCPs are auto-connected (skipping built-in `filesystem`), and the Agent is automatically rebuilt at the end of a turn when configurations are modified, making new configs effective immediately.

### Rule System

| Component | Description |
|-----------|-------------|
| `IRule` | Rule interface, defines execution conditions and behavior |
| `RuleBase` | Rule base class |
| `RuleEngine` | Rule engine, evaluates rules by priority |
| `ContextInjectBuilder` | Context injection builder, assembles rule engine `Inject` text into context injectable into chat/system prompts |
| `PathAccessRule` | Built-in path access rule, restricts file system access scope |

### MCP System

| Component | Description |
|-----------|-------------|
| `IMCPClient` | MCP client interface, interacts with MCP servers |
| `StdioMCPClient` | stdio JSON-RPC based external MCP client |
| `MCPRegistry` | MCP registry, manages built-in and external clients (workspace files / config.json / in-process, three-tier merge) |
| `MCPClientFactory` | Builds external clients from `McpServerConfig.Transport` (the stdio / http / sse dispatch lives in one place) |
| `MCPToolPlugin` | MCP tool plugin, exposes MCP tools to Agent |
| `FileSystemMCPClient` | Built-in file system MCP client |

### Session System

| Component | Description |
|-----------|-------------|
| `ISessionManager` | Session manager interface, supports create/switch/clear |
| `SessionChatHistoryProvider` | Session history provider, auto-persists conversation history |
| `SessionOptions` | Session config, supports compression thresholds |

### Attachment System

| Component | Description |
|-----------|-------------|
| `IAttachmentProcessor` | Attachment processor interface: reports supported types/MIME types and generates thumbnails on demand |
| `DefaultAttachmentProcessor` | Default implementation: images (PNG/JPEG/GIF/WebP/BMP; TIFF/HEIC require conversion first) and text files; decodes and resizes images, reads text content |
| `AttachmentInfo` | Attachment metadata (file name, MIME, size, kind, source path) |
| `AttachmentKind` | Attachment kind enum (Image / Text) |
| `ProcessedAttachment` | Processing result (metadata + text content / thumbnail path + pixel size) |
| `AttachmentMessageBuilder` | Assembles attachments into a `ChatMessage` (images → base64 `DataContent`; text ≤50KB inlined; >50KB injects only a path hint) |
| `AttachmentRecord` | Session persistence record (paths and metadata only, no base64) for history replay |

### Rule Interception

| Component | Description |
|-----------|-------------|
| `RuleCheckedAIFunction` | Rule-checking decorator, intercepts tool calls |
| `CustomRule` | Custom rule adapter, supports wildcard matching |

### Security & Confirmation

| Component | Description |
|-----------|-------------|
| `ToolConfirmationService` | Tool execution confirmation service: allows operations directly inside the workspace, allows paths outside the workspace after user confirmation, and requires confirmation before dangerous operations |
| `ToolConfirmationContext` | Confirmation context: holds the three-tier authorization (this time / this turn / this session) keyed by grant root; `AllowThisTurn`/`IsAllowedThisTurn`, `AllowThisSession`/`IsAllowedThisSession` |
| `ConfirmResult` | User confirmation result: `Allow` (this time) / `AllowAll` (all this turn) / `AllowSession` (all this session) / `Deny` |
| `PathGuard` | Path security guard, provides a fast path for workspace roots (no longer a hard gate for out-of-workspace access) |
| `RuleEngine` | Rule engine, performs permission checks and parameter modification before tool execution |

### Multi-Agent Orchestration

Main Agent parses composite tasks → decomposes into DAG task graph → dispatches SubAgents for serial/parallel execution.

| Component | Description |
|-----------|-------------|
| `IOrchestrator` / `Orchestrator` | Orchestrator entry, chains planning, scheduling, and result aggregation |
| `ITaskPlanner` | Task planner interface, converts natural language tasks to TaskGraph |
| `LlmTaskPlanner` | LLM-based planner, generates DAG via prompt engineering |
| `GraphPlanStore` | Holds task graphs (created by `plan_task`, consumed by `run_orchestration`) with TTL and capacity eviction |
| `IOrchestrationProgressSink` | Orchestration progress outlet, no-op by default; hosts may register an implementation (e.g. live TUI rendering) |
| `DagScheduler` | DAG scheduler, layer-based parallel execution via topological sort |
| `SubAgentFactory` | SubAgent factory, wraps LuBanAgentFactory for child agent creation |
| `SubAgentRoleRegistry` | SubAgent role registry, manages built-in and custom roles |
| `SubAgentRole` | SubAgent role definition, includes name, system prompt template, default tool groups |
| `ContextStore` | Cross-node context store, isolated by graph ID, thread-safe |
| `TaskGraph` / `TaskNode` | DAG data models, support dependency declaration, placeholder references, critical nodes, role assignment; `TaskGraph.SharedContext` carries workspace memory/rule context |
| `SubAgentSpec` | SubAgent specification (prompt, tool groups, workspace root, etc.); its `SharedContext` is appended to the sub-agent system prompt |
| `OrchestrationToolPlugin` | Tool plugin, exposes orchestration capability to main Agent |
| `ReflectionResult` / `ReplanContext` | Dynamic replanning models, LLM analyzes failures and generates fix graph after critical node failure |

### Plugin System

A plugin is an **application-level** install unit (`plugins/<id>/` directory plus a `plugin.json` manifest). The host loads it at runtime through a collectible `AssemblyLoadContext`, and on disable/uninstall it breaks every reference in a fixed order and verifies them with GC weak-reference assertions.

| Component | Description |
|------|------|
| `IPlugin` | Plugin contract: `Id` / `Name` / `Version` / `Description` / `InitializeAsync` / `ShutdownAsync` / `ContributeSkills` |
| `PluginBase` | Plugin base class (namespace `LuBan.AIAgent.Plugins.Base`), providing `Host` / `SafeLog` / `PostToUi` (wrapped in an invalidation token, plugins must never post to the UI thread directly) and `OnInitializeAsync` / `OnShutdownAsync` hooks |
| `IPluginHost` | Host capability contract: register/unregister sidebar entries and panels, register/unregister Skills and MCP servers, `PostToUi`, `GetConfig`, plugin logging |
| `PluginManager` | Singleton manager: `ScanAsync` / `RestoreEnabledAsync` / `GetPlugins` / `EnableAsync` / `DisableAsync` / `InstallAsync` / `UpgradeAsync` / `UninstallAsync`, plus the `Changed` event |
| `PluginLoader` / `ManifestValidator` | Manifest reading and validation: required fields, id/version regexes, entry assembly existence, `minHostVersion` / `targetApp`, duplicates, `transport`, dependency self-reference; `ValidatePermissions` performs the permission/credential coverage check on its own (used by the pre-load gate, separate from the whole-manifest validation that yields `Error`) |
| `PluginDependencyResolver` | Dependency topology and cycle detection (validates existence only, never auto-enables) |
| `PluginLocation` | Directory convention: `<AppContext.BaseDirectory>/plugins/<id>/{plugin.json, lib/, assets/}` |
| `IPluginStateRepository` / `PluginStateRepository` | State persistence contract plus the default JSON-file implementation (`id` / `version` / `state` / `installedAt` / `lastError` / `requiresRestart`) |
| `BuiltinPluginCatalog` | Built-in plugin catalog that registers in-process `IPlugin` instances shipped with the host (must be registered as Singleton) |
| `PluginHostScope` / `PluginDescriptor` / `PluginChangedEventArgs` | Plugin identity backfill and contribution bookkeeping, read-only descriptor for manager UIs, state-change event payload |

Directory semantics: **plugins exist at application level only** (`<AppContext.BaseDirectory>/plugins/<plugin-id>/`); the Skills / MCP servers a plugin **contributes** land at workspace level (`.luban-agent/skills`, `.luban-agent/mcps`) and are explicitly deleted on uninstall.

### Plugin Permissions & Credential Security

Plugins are third-party code, so the host constrains their access with **directory whitelists + default deny** (PRD §3.7): **at declaration time** every permission scope in the manifest must fall inside the whitelist directory, otherwise the plugin is marked `Disabled` before loading; **at runtime** the real egress points (network / process / credential / Git) are intercepted again according to the declaration.

| Component | Description |
|------|------|
| `PluginSecurityOptions` | Whitelist policy (`NetworkWhitelist` / `ProcessWhitelist` / `GitWhitelist` / `FilesystemWhitelist` / `CredentialCatalog` / `EnforceAtRuntime` / `AuditDenials` / `MaxAuditRecords`), overridable from the `LuBanAgent:Plugins:Security` configuration node (collection entries are **additive**); the default network whitelist is `codeup.aliyun.com` / `flow.aliyun.com` / `*.aliyuncs.com`, the process whitelist is `dotnet` / `git`, **the Git capability whitelist is the groups `clone_push` (`clone` / `push`) / `commit` (`add` / `commit`) / `inspect` (`rev-parse` / `status` / `diff` / `branch` / `config` / `checkout`)**, and the credential catalog is `connector:yunxiao:token` (the legacy spelling `yunxiao_pat` still normalizes) |
| `PluginPermissionEvaluator` | The decision engine: `ValidateDeclaration` performs the declaration-time coverage check (concrete out-of-range scopes go into `ManifestValidationResult.Errors`; a wildcard `*` goes into `Warnings`); `IsNetworkAllowed` / `IsProcessAllowed` / `IsCredentialAllowed` / `IsGitAllowed` / `IsFilesystemAllowed` perform read-only runtime boundary decisions (a wildcard declaration is bounded by the host whitelist; they never throw) |
| `PluginScopeContext` | The permission environment (`AsyncLocal`): `Enter(pluginId, permissions)` declares "which plugin identity the current call belongs to", and it nests; `Current == null` means a host-originated call and is always allowed |
| `PluginPermissionGuard` | The runtime egress guard: `RequireNetwork` / `RequireProcess` / `RequireCredential` / `RequireGit` throw `PluginPermissionDeniedException` on a boundary violation and append to the bounded `RecentDenials` audit; `PluginPermissionGuard.Default` is the fallback instance for non-DI scenarios |
| `ICredentialStore` / `ConfigurationCredentialStore` | Credential read/write contract plus the default read-only "configuration node `LuBanAgent:Credentials:<key>` + environment-variable fallback" implementation (`SetAsync` throws `NotSupportedException`); desktop hosts should register their own writable implementation to replace it. A canonical key contains colons (`connector:yunxiao:token`), which is a three-level nested node in `IConfiguration`, so `Keys` flattens paths back to full keys — do not rely on short child names |
| `CredentialKeys` | Credential key catalog and normalization: connector tokens are uniformly `connector:<instanceName>:token` (e.g. `connector:yunxiao:token`, see `ForConnectorToken`), and the legacy spellings `codeup:pat` / `codeup_pat` / `yunxiao:pat` / `yunxiao_pat` all normalize to the same canonical key; organization ID → `yunxiao_organization_id`; environment-variable mapping `LUBAN_YUNXIAO_PAT` / `LUBAN_YUNXIAO_ORGANIZATION_ID`; `GetLegacyKeyAliases` supports legacy-key fallback reads (the Q15 generalization) |

Key points:

- **The whitelist uses "coverage" semantics, not "containment"**: every **concrete scope** a plugin declares must be covered by some whitelist rule. A wildcard `*` (including the bare-dimension array shorthand, e.g. `["filesystem", "network"]`) means "declare this dimension; the concrete scope is decided by the host whitelist" — it is accepted at declaration time with a warning and bounded by the whitelist at runtime, so it cannot bypass the whitelist. `*.aliyuncs.com` covers `openapi-rdc.aliyuncs.com` but not the bare domain `aliyuncs.com`.
- **Git capabilities are declared by "group" and decided by "operation name"**: each whitelist entry is a capability group, and declaring the group grants all of its sub-operations — `clone_push` → `clone` / `push` (remote read/write), `commit` → `add` / `commit` (local staging and committing), `inspect` → `rev-parse` / `status` / `diff` / `branch` / `config` / `checkout` (local read-only inspection and branch switching). A concrete operation name may also be declared directly (e.g. `git:push`, which is also valid once covered by `clone_push`). The host decides on the **operation name**, and a plugin that shells out to git itself passes the subcommand name (e.g. `add`) — a plugin declaring only `clone_push` is denied when it runs a local `add` / `commit`.
- **`Disabled` vs `Error`**: a non-compliant permission/credential declaration means "host policy forbids enabling" → `Disabled`; a manifest format problem (missing `id`, invalid `id`, missing entry assembly, ...) means "the plugin itself is unusable" → `Error` at the activation stage. Manager UIs can therefore still list malformed plugins and surface a diagnosable error.
- Setting `PluginSecurityOptions.EnforceAtRuntime = false` audits without blocking, purely for compatibility debugging; production should keep it `true`.
- Credentials **never** land in the DB as plain text; the framework only ships the "configuration + environment variable" fallback, and the actual storage baseline is the host's choice (desktop hosts reuse their LLM ApiKey baseline).

### Connector System (Connectors)

A connector bundles "external-system I/O + permission gate + MCP exposure" into one configurable unit: **the abstraction and built-in implementations live in the framework**, and **the host only assembles them from configuration** — no name-based `switch` (PRD §3.10).

| Component | Description |
|-----------|-------------|
| `IConnector` | Connector contract: `Key` / `Name` / `DisplayName` / `McpExposures` / `InitializeAsync`, implements `IAsyncDisposable` |
| `ConnectorMcpExposure` | An MCP surface exposed by a connector: `Name` / `Description` / `CreateClient(services, scope)` (the in-process client only **borrows** the connector; `Dispose` never disposes the connector) |
| `ConnectorBuildContext` / `ConnectorSettings` | Factory inputs (instance name / provider key / services / settings / `IConfiguration`) and the settings bag |
| `AddConnector<T>` / `AddBuiltinConnectors` | Registers connector **factories** (built-ins are registered, not enabled); `AddLuBanAgent` already calls this, minimal hosts may call it explicitly |
| `ConnectorRegistry` (scoped) | Resolver: `LoadFromWorkspace` / `GetStates` (4-state catalog) / `ApplyEnabledAsync` / `CreateMcpClient(mcpName, scope)` / `UnregisterAll` |
| `ConnectorFactoryRegistry` / `ConnectorInstanceHost` | Factory table plus the host-level **singleton** instance cache (connectors are owned by DI; session-scoped MCP clients are derived by name) |
| `ConnectorConfig` | A `config.json` `connectors[]` item: `name` / `provider` / `enabled` / `settings` |

**Three-level availability (configuration-driven)**:

| Level | Source | Effect |
|-------|--------|--------|
| Declared | `config.json:connectors` (via `IAppConfigReader.Connectors`) | Whether the provider is **available** (undeclared / unregistered = `Unavailable`, warn and skip, never fail-fast) |
| Enabled | `.luban-agent/connectors/<name>.json` | Whether the connector is enabled in this **workspace** (enabling an undeclared connector counts as not installed) |
| Exposed | Plugin manifest `mcpServers: ["<name>"]` | Resolves the connector's MCP surface by name and injects the plugin permission scope |

```json
// config.json (host per-user file)
{ "connectors": [ { "name": "yunxiao", "provider": "aliyunDevOps", "enabled": false } ] }
```

```json
// appsettings.json (non-secret parameter overrides; the PAT never lands here)
{ "LuBanAgent": { "Connectors": { "Yunxiao": { "OrganizationId": "your-org-id" } } } }
```

> Credential keys are generalized per instance name to `connector:<instanceName>:token` (see `CredentialKeys`); the PAT flows only through `ICredentialStore` and is **never** written in plain text into `.luban-agent/mcps/*.json`.

### Aliyun DevOps Connector (Codeup / Yunxiao)

Code repository and pipeline access for Aliyun DevOps (formerly Codeup / Flow) lives in `Connectors/AliyunDevOps/`; it implements `IConnector` (`ProviderKey = "aliyunDevOps"`, default instance name `yunxiao`), registers its factory through `AddBuiltinConnectors` and is assembled from configuration, and every outbound call passes through `PluginPermissionGuard`.

| Component | Description |
|-----------|-------------|
| `YunxiaoApiOptions` / `YunxiaoConnector` | Organization ID, domain, timeout, and the shared `HttpClient` egress |
| `CodeupClient` | Repository I/O: `SearchRepositoriesAsync` / `GetRepositoryAsync` / `CreateRepositoryAsync` / `UpdateRepositoryAsync` (creation supports `namespaceId` for a code group and `templateProject` for template sync), `ListBranchesAsync` / `ListTemplateRepositoriesAsync` (template-library lookup and runtime pin resolution), `ListNamespacesAsync` / `CreateNamespaceAsync` (find-or-create code groups), `CloneToLocalAsync` / `PushAsync` (both return `GitCommandResult`) |
| `GitCommandResult` | Local Git command result (`Success` / `ExitCode` / `StandardOutput` / `StandardError` / `TimedOut`): on failure `FailureSummary()` surfaces the raw git error summary (URL-embedded credentials masked via `SecretMasker`), replacing the previous `bool`-only return |
| `FlowClient` | Pipeline I/O (Aliyun Flow) |
| `IYunxiaoCredentialProvider` / `PatCredential` | PAT and organization ID credential source (the PAT is read from `connector:yunxiao:token`, with legacy-key `yunxiao_pat` compatibility; the organization ID from `yunxiao_organization_id`), no plain-text DB storage |
| `YunxiaoMCPClient` / `YunxiaoToolDefinitions` | Wraps repository / pipeline capabilities as **10** MCP tools (Codeup: `codeup_search_repos` / `codeup_clone` / `codeup_create_repo` / `codeup_push` / `codeup_list_branches` / `codeup_create_group`; Flow: `flow_list_pipelines` / `flow_clone_pipeline` / `flow_update_pipeline` / `flow_run_pipeline`) for conversational use |

**Response shapes**: a successful Yunxiao OpenAPI response is a **bare object / bare array** (no `{ result: ... }` envelope) and the list total is in the `x-total` response header; business errors use `{ code, errorCode, errorDescription, errorMessage, ... }` (`errorCode` is a string) and are mapped to `YunxiaoApiException` by `YunxiaoErrorMapper`.

## Usage Guide

### 1. Configuration & Registration

```json
{
  "LuBanAgent": {
    "DefaultModel": "openai:gpt-4o",
    "SystemPrompt": "You are a helpful assistant.",
    "MaxToolLoopIterations": 10,
    "Session": {
      "CompactTargetMessages": 20,
      "CompactThreshold": 10
    }
  }
}
```

```csharp
// Register services
services.AddSingleton<IAppConfigReader>(myConfigManager);
services.AddSingleton<IProviderRouter>(myProviderRouter);
services.AddLuBanAgent(configuration);   // already calls AddBuiltinConnectors: registers built-in connector factories (registered, not enabled)
```

> Whether a connector is available is declared by `IAppConfigReader.Connectors` (`config.json:connectors`), and whether it is enabled is decided by the workspace file `.luban-agent/connectors/<name>.json` — see the "Connector System" section.

### 2. Multi-Model Routing

```csharp
// Use provider:model format to route to different models
// IProviderRouter auto-dispatches based on the provider prefix in ModelId
var agent = await factory.CreateAsync(modelName: "qwen:qwen-plus");

// Switch providers by changing the prefix only
var agent2 = await factory.CreateAsync(modelName: "openai:gpt-4o");
```

### 3. Tool Registration & Usage

```csharp
// Specify tool groups when creating Agent
var agent = await factory.CreateAsync(
    toolGroups: new[] { "browser", "filesystem" });

// Agent auto-selects and invokes tools
var response = await agent.RunAsync("List all .cs files under src directory and count lines of code");

// Streaming execution
await foreach (var update in agent.RunStreamingAsync("Help me analyze this code"))
{
    Console.Write(update.Text);
}
```

### 4. Skill Management

```csharp
// Get Skill registry
var skillRegistry = serviceProvider.GetRequiredService<SkillRegistry>();

// List all Skills
var skills = skillRegistry.GetAll();

// Execute a Skill
var context = new SkillContext
{
    Agent = agent,
    UpdateStatus = status => Console.WriteLine($"Status: {status}")
};
var result = await skillRegistry.Get("brainstorming")
    .ExecuteAsync(context, "I want to implement a user login feature");
```

### 5. Custom Tool Plugin

```csharp
public class MyToolPlugin : ILuBanToolPlugin
{
    public string GroupName => "my-tools";
    public string? Description => "Custom toolset";

    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        return new List<AIFunction> { /* ... */ };
    }

    public bool IsEnabled(LuBanAgentOptions options) => true;
}

// Register
services.AddSingleton<ILuBanToolPlugin, MyToolPlugin>();
```

### 6. Custom Skill

**Method 1: File-based Skill (Recommended)**

Create `SKILL.md` files in project-level or user-level directories:

```bash
# Project-level directory
<workspace>/.luban-agent/skills/my-skill/SKILL.md

# User-level directory
%LocalAppData%/LuBan/AIAgent/skills/my-skill/SKILL.md
```

SKILL.md format:

```markdown
---
name: my-translator
description: "Translate text to English"
category: custom
---

# Translation Assistant

Please translate the user's content to English.

## Requirements
- Maintain the tone and style of the original text
- Use idiomatic English expressions
```

**Method 2: Code-defined Skill**

```csharp
public class MyCustomSkill : SkillBase
{
    public override string Id => "my-custom-skill";
    public override string Name => "My Custom Skill";
    public override string Description => "Custom Skill example";
    public override string Category => "custom";
    public override string? PromptTemplate => "Custom prompt template...";

    public override async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        UpdateStatus(context, "Processing...");
        var result = await CallAgentAsync(context, input);
        return SkillResult.Ok(result ?? "");
    }
}

// Register
services.AddSingleton<ISkill, MyCustomSkill>();
```

### 7. Custom Rule

```csharp
public class MyRule : RuleBase
{
    public override string Id => "my-rule";
    public override string Name => "My Rule";
    public override int Priority => 50;

    public override bool IsApplicable(RuleContext context)
        => context.ActionType == "file-write";

    public override Task<RuleResult> ExecuteAsync(RuleContext context)
    {
        var path = context.Arguments.GetValueOrDefault("path")?.ToString();
        if (path?.Contains("secret") == true)
            return Task.FromResult(RuleResult.DenyResult("Access to paths containing 'secret' is forbidden"));
        return Task.FromResult(RuleResult.AllowResult());
    }
}

// Register
services.AddSingleton<IRule, MyRule>();
```

### 8. External Plugin Loading

```json
{
  "LuBanAgent": {
    "ExternalPlugins": ["MyCompany.AgentPlugins", "ThirdParty.Tools"]
  }
}
```

Specify assembly names via `ExternalPlugins` configuration — the framework auto-scans and registers types implementing `ILuBanToolPlugin`.

### 9. Multi-Agent Task Orchestration

```json
{
  "LuBanAgent": {
    "Orchestration": {
      "Enabled": true,
      "PlannerReasoningEffort": "none",
      "MaxNodes": 10,
      "MaxParallelism": 5,
      "DefaultNodeTimeoutSeconds": 0,
      "MaxReplanAttempts": 3,
      "ReflectionTimeoutSeconds": 0,
      "DefaultToolGroups": []
    }
  }
}
```

- `Enabled`: When enabled, two orchestration tools are exposed to the model — `plan_task` (the model itself decides a task is composite and produces a task graph) and `run_orchestration` (executes the graph by the `graphId` returned from `plan_task`). Whether to orchestrate is entirely the model's decision; the framework no longer applies heuristics or automatic detection.
- Planning always uses the LLM planner. When `PlannerModel` is `null` it inherits the main model; set `PlannerReasoningEffort` to `none` to disable reasoning-model thinking output and significantly shorten planning time.

**SubAgent Role System**: The planner can assign a role (`analyst`/`researcher`/`coder`/`writer`) to each node. Roles provide specialized system prompts and default tool groups. 4 built-in roles are included, with support for custom roles via workspace extensions.

#### Workspace Orchestration Extensions

On entering an `/agi` workspace, the following directories are loaded automatically:

- `.luban-agent/roles/*.json`: custom SubAgent roles; same-name entries override built-in roles. Format: `{ "name": "...", "systemPromptTemplate": "... {prompt} ...", "defaultToolGroups": [...] }`.

#### Multi-Model Routing

When an `IProviderRouter` is registered, `TaskNode.ModelName` (format `provider:model`) and `OrchestrationOptions.PlannerModel` are routed to the corresponding provider. Routing failures fall back to the default model with a warning. Behavior is unchanged without a router.

#### Orchestration Decision

Whether to orchestrate is entirely the model's decision: the framework exposes the `plan_task` and `run_orchestration` tools, and the model itself decides whether an input is a composite task and then builds/executes the graph. The framework no longer applies heuristic pre-filtering or automatic detection.

**Dynamic Replanning**: When critical node failures cause overall status `failed`, the orchestrator automatically triggers reflection:
1. **Reflect**: LLM analyzes failed nodes and their direct dependencies' outputs to determine if fixable
2. **Replan**: LLM generates fix nodes (with `fix_{attempt}_` prefix); dependencies pointing to already-succeeded nodes are resolved and inlined into the prompt text, so no reference is left to a node outside the fix graph
3. **Retry**: Executes fix graph, up to `MaxReplanAttempts` times (default: 3)

```csharp
// Invoke orchestrator directly
var orchestrator = serviceProvider.GetRequiredService<IOrchestrator>();
var result = await orchestrator.RunAsync("Research LuBan framework and generate a comparison report");

Console.WriteLine($"Overall status: {result.OverallStatus}");
Console.WriteLine($"Replanning attempts: {result.ReplanningAttempts}");
Console.WriteLine($"Final output:\n{result.FinalOutput}");

// Execute with a progress callback (keeps the final result): planning/node-level events
// are reported in real time, while the return value remains the full orchestration result.
var result2 = await orchestrator.RunAsync(
    graph,
    onProgress: p => Console.WriteLine($"{p.EventType}: {p.Message}"),
    cancellationToken: default);
```

Orchestration is triggered by the model calling the `plan_task` / `run_orchestration` tools, and
execution happens inside the tool call, so progress is no longer streamed with the conversation.
The framework broadcasts `OrchestrationProgress` (`EventType` / `NodeId` / `Message` / `NodeResult` /
`Activity` / `ElapsedMs`) through `IOrchestrationProgressSink` (a no-op implementation is registered by
default); hosts may register a custom sink (e.g. the CLI's `TuiOrchestrationProgressSink`) to render
progress line by line during planning and node execution.
Progress event types include: `PlanningStarted`, `PlanningCompleted`, `NodeStarted`, `NodeCompleted`,
`NodeFailed`, `NodeSkipped` (a node skipped because a critical predecessor failed, reported per node),
`ReflectionStarted`, and `NodeActivity` (per-node thinking/tool-call details).

**Orchestration Flow**:

1. **Planning**: `ITaskPlanner` decomposes natural language task into DAG (template-first, LLM fallback)
2. **Validation**: `TaskGraph.Validate` checks for acyclicity, dependency existence, no duplicate IDs
3. **Scheduling**: `DagScheduler` executes layers via Kahn topological sort, parallel within same layer
4. **Context Passing**: `{dep:xxx}` placeholders in node prompts are replaced with predecessor outputs by `ContextStore`
5. **Error Handling**: Critical node failure skips successors; non-critical failure continues execution
6. **Result Aggregation**: Terminal nodes (no successors) outputs are aggregated into `FinalOutput`

**Memory Context Injection**: The orchestration entry (`Orchestrator.ExecuteGraphAsync`) uses `ContextInjectBuilder` to build
workspace long-term memory and rule context, writes it once into `TaskGraph.SharedContext` (filled once; replan fix graphs inherit
the same copy), and `SubAgentFactory` then appends it to each SubAgent's system prompt, so sub-agents share the same workspace
memory as the main agent conversation. The regular chat path injects recall results directly as System messages via
`SessionChatHistoryProvider`; both paths share the same `ContextInjectBuilder`.

**Key Concepts**:

- **Critical Node** (`IsCritical = true`): Failure blocks successor execution, overall status is `failed`
- **Non-Critical Node**: Failure allows successors to continue, overall status is `partial`
- **Placeholder**: `{dep:node-id}` references predecessor output, auto-replaced at runtime
- **Parallelism**: `MaxParallelism` limits max parallel nodes per layer, 0 means unlimited
- **Shared Context** (`SharedContext`): Workspace memory/rule context, built by the orchestration entry and injected into all sub-agents

### 10. Error Handling and Automatic Retry

All LLM/API calls are classified by the framework (structured first: HTTP status + `error.code`). On final failure an `AgentApiException` is thrown; hosts only need to render `Error.FriendlyMessage`:

| Category | Typical error | Auto retry |
|---|---|---|
| Auth / permission / balance | 401 / 402 / 403 | No — abort and show guidance |
| Model not found | 404 / 410 | No — abort with a checklist |
| Bad request / context length / unsupported capability | 400 / 413 / 422 | No — abort with guidance |
| Content moderation | `content_filter` | No — abort and suggest rewording |
| Rate limit | 429 `rate_limit_exceeded` | Yes (3 attempts by default, 1s/2s exponential backoff with jitter, honors `Retry-After`, single-wait cap 30s) |
| Server / network / timeout | 5xx / DNS/TLS / timeout | Yes (same as above) |
| Quota exhausted | 429 `insufficient_quota` | No — abort; the server-provided reset time is included in the message |
| Stream interruption | Connection drops after content was emitted | No — keep partial output and report the interruption |

Configuration (`LuBanAgent` section in `appsettings.json`):

```json
"ApiRetry": {
  "Enabled": true,
  "MaxAttempts": 3,
  "BaseDelayMs": 1000,
  "BackoffFactor": 2,
  "MaxDelayMs": 30000
}
```

Retry progress callback (set by the host at startup; invoked on the calling thread):

```csharp
options.Value.OnApiRetry = notice =>
    Console.WriteLine($"{notice.Category} retrying in {notice.Delay.TotalSeconds:F0}s (attempt {notice.Attempt})");
```

Notes:

- Streaming calls are retried only **before the first token**; once content has been emitted a failure is not retried to avoid duplicated output.
- The resilience middleware sits inside `FunctionInvokingChatClient`, so each tool round retries its own API call independently.
- The OpenAI SDK's built-in retries (`ClientRetryPolicy.Default`, 3 extra attempts) are disabled on the host side so the configured retry budget and progress notifications are authoritative.
- Recognizable API failures during orchestration planning are no longer silently downgraded to a normal chat; they surface with their classification.
- Session summarization failures degrade to "skip compaction" instead of breaking the main conversation.

### 11. Tool Confirmation and Abort Semantics

- **Deny aborts the turn**: when the user chooses "Deny" in a tool confirmation, the host immediately aborts the current turn (partial output is kept). Both CLI and Codex show "已拒绝，本轮终止"; CLI marks the denied tool block "已被用户拒绝", while Codex marks the confirmation card "已拒绝" and the tool card with the framework-neutral result "工具调用被拒绝或已取消".
- **Unbounded waits**: tool confirmations and workspace authorization wait indefinitely for user input; there is no timeout. Esc cancels the turn at any time ("任务已取消" in CLI, "已取消" in Codex).
- **In-workspace operations are confirmed-free**: file-system tools whose target path falls inside a workspace root (or a root authorized this turn/session) are allowed directly without confirmation.
- **Out-of-workspace paths are confirmed, not blocked**: paths outside the workspace are no longer hard-blocked; instead a confirmation is shown with "Allow (this time) / Allow All This Turn / Allow All This Session / Deny". Authorization is remembered per grant root (parent directory for files, the directory itself for directories); when the grant root resolves to a drive root (e.g. `D:\`), it is not remembered, to avoid granting a whole drive in one step.
- **Grant tiers**: `Allow` grants this call only; `AllowAll` records the grant for this turn; `AllowSession` records it for this session (process lifetime). `Reset()` clears the turn set at the start of each turn, while the session set survives across turns; `ResetSession()` explicitly clears the session set.
- **Read-only fast path (Plan only)**: in Plan mode, read-only tools (`ReadFileAsync`/`ListDirectoryAsync`/`GetWorkspaceOverviewAsync`/`SearchFilesAsync`/`GrepAsync`/`GetFileInfoAsync`) are allowed directly within authorized scope so the Agent can read context to produce a plan; out-of-scope read-only calls are recorded as planned actions instead of being allowed.
- **Deletion tools always confirm**: `AlwaysConfirmTools` (default `DeleteFileAsync`/`DeleteDirectoryAsync`/`DeletePageAsync`) must confirm regardless of whether the path is inside the workspace, and "Allow All This Turn/Session" is not remembered for them — every deletion confirms again.
- **Sub-agent delegated confirmation**: orchestration sub-agents never prompt the user. Tool calls that would need manual confirmation are decided by the main code against the turn/session allowed set — allowed when present, otherwise a sub-agent-specific denial (not a user denial).
- **Turn/session allow-lists**: `ToolConfirmationContext.AllowedThisTurn` is no longer public; hosts call `AllowThisTurn(toolName)`/`AllowThisSession(toolName)` and the framework checks `IsAllowedThisTurn(toolName)`/`IsAllowedThisSession(toolName)`.
- **Unbounded sub-agent waits**: `Orchestration.DefaultNodeTimeoutSeconds` and `ReflectionTimeoutSeconds` default to 0 (unbounded); set them above 0 to enforce limits, and `TimeoutSeconds` can still override per node.
- **Planning no longer hangs**: `Orchestration.PlannerReasoningEffort` controls the reasoning effort of planning/reflection calls. Reasoning models (e.g. glm-5) emit large amounts of reasoning content during planning by default, which can take over a minute per call; set it to `"none"` to disable reasoning (`null`, the default, sends nothing and stays compatible with non-reasoning models that do not support `reasoning_effort`).

### 12. Plugin System

A plugin is a triple of manifest (`plugin.json`) + assemblies (`lib/`) + static assets (`assets/`), installed into `<AppContext.BaseDirectory>/plugins/<plugin-id>/`, loaded by the host at runtime, and disableable/uninstallable.

**Write your first plugin (built-in, shipped with the host)**

```csharp
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Plugins.Base;   // PluginBase lives in a sub-namespace; add the using explicitly

public sealed class HelloPlugin : PluginBase
{
    public override string Id => "com.tjc.hello";
    public override string Name => "Hello Plugin";
    public override string Version => "1.0.0";

    protected override Task OnInitializeAsync(IPluginHost host, CancellationToken cancellationToken)
    {
        host.RegisterSidebarEntry(new PluginSidebarEntry
        {
            Id = "hello-entry",
            Label = "Hello",
            PanelType = "hello-panel"
        });

        host.RegisterMcpServer("hello-mcp");

        return Task.CompletedTask;
    }

    public override IEnumerable<SkillDefinition> ContributeSkills()
    {
        yield return new SkillDefinition { Id = "hello-skill", Name = "Hello Skill" };
    }
}
```

Register it in DI — **`AddSingleton` is mandatory** (the `PluginManager` constructor chain may only contain singleton services; hosts that enable `ValidateScopes` will throw otherwise):

```csharp
services.AddSingleton<IPlugin, HelloPlugin>();
services.AddLuBanAgent(configuration);      // registers PluginManager / PluginLoader / state repository
services.AddSingleton<IPluginHost, MyPluginHost>();   // host-supplied: wires into real UI and registries
```

**Write a directory-based plugin (`plugin.json`)**

```json
{
  "id": "com.tjc.demo",
  "name": "Demo Plugin",
  "version": "1.0.0",
  "author": "TJC",
  "description": "Sample plugin",
  "entryAssembly": "Tjc.Demo.Plugin.dll",
  "targetApp": "codex",
  "minHostVersion": "2026.10.15",
  "uiContributions": [{ "id": "demo-entry", "label": "Demo", "panelType": "demo-panel" }],
  "skills": [{ "id": "demo-skill", "name": "Demo Skill" }],
  "mcpServers": [{ "name": "demo-mcp", "transport": "inproc", "enabled": true }],
  "permissions": { "filesystem": ["workspace"], "network": ["codeup.aliyun.com", "*.aliyuncs.com"], "credential": ["yunxiao_pat"] },
  "dependencies": ["com.tjc.base"]
}
```

Manifest conventions:

| Field | Convention |
|------|------|
| `id` | Required, `^[a-zA-Z][a-zA-Z0-9._\-]{1,127}$` |
| `name` / `version` / `entryAssembly` | Required; version looks like `1.0.0` or `1.0.0-beta.1` |
| `skills[].id` | Must not contain dots (matches the Skill registry convention) |
| `mcpServers[].transport` | Only `inproc` is supported for now (in-process MCP client) |
| `permissions` | Six dimensions `filesystem` / `network` / `git` / `database` / `process` / `credential`; the array shorthand `["filesystem:workspace"]` is accepted (a bare dimension such as `"network"` equals the wildcard `"*"` for that dimension), an omitted dimension is equivalent to undeclared, and `"permissions": null` normalizes to empty permissions. A **concrete scope** must be **covered** by the host whitelist, and a boundary violation means `Disabled`; a wildcard `*` passes declaration time and is bounded by the whitelist at runtime (see "How a permission declaration takes effect" below) |
| `dependencies` | Only verifies that the dependency plugin exists and is enabled; self-reference is rejected |

A JSON Schema for `plugin.json` ships with the package (`Plugins/Schemas/plugin.schema.json`) so editors can complete and validate it.

**How a permission declaration takes effect**

The `permissions` in a manifest is not a notification — it is an **application reviewed against the host whitelist** (PRD §3.7: directory whitelist + default deny):

1. **Declaration time**: `EnableAsync` first calls `PluginLoader.ValidatePermissions` (plugin-level plus skill-level) and checks every **concrete scope** against `PluginSecurityOptions`; any violation marks the plugin **`Disabled`** (with the offending scope in `LastError`). A wildcard `*` (including the bare-dimension array shorthand, e.g. `["filesystem", "network"]`) is not a violation: it is recorded as a warning and bounded by the whitelist at runtime. `*.aliyuncs.com` covers `openapi-rdc.aliyuncs.com` but not the bare domain `aliyuncs.com`.
2. **Runtime**: framework connectors (`CodeupClient` / `FlowClient` / `YunxiaoMCPClient`, ...) consult `PluginPermissionGuard` right before any network / process / credential / Git call. The host wraps plugin calls with `PluginScopeContext.Enter(pluginId, permissions)`; plugins can call `PluginBase.EnterPermissionScope()` themselves in callbacks the host cannot wrap (UI events). **Undeclared means denied**; a dimension declared as wildcard `*` is bounded by the host whitelist; an empty permission environment denotes a host-originated call and is always allowed.
3. **Credential access**: plugins cannot read credentials directly; they declare `credential` and the host fetches on their behalf through `ICredentialStore`. With `EnforceAtRuntime = false` the guard only audits instead of blocking (compatibility debugging only; production should keep it `true`).

**Lifecycle and unload semantics**

- States: `Installed` → `Enabled` → `Disabled` (with `Error` as the exception branch), persisted across restarts: `ScanAsync` restores the memory (plugins last left `Enabled` are marked for startup), and `RestoreEnabledAsync` actually enables them.
- Disable/uninstall always follows **7 steps**: ① `ShutdownAsync` → ② unregister UI contributions → ③ unregister Skills / MCP servers (**must happen before the ALC unload**, otherwise registry-held instances become lingering strong references and unload inevitably fails) → ④ delete the workspace file-based Skills / MCP servers → ⑤ `AssemblyLoadContext.Unload()` → ⑥ several rounds of `GC.Collect` plus weak-reference verification → ⑦ on failure, **degrade** to `Disabled` with `RequiresRestart = true`, tell the user to restart, and write the reference chain to the diagnostic log.
- Entries a plugin **registers itself** during `InitializeAsync` (rather than declaring in the manifest) are tracked too, and are unregistered precisely on unload, preventing "plugin uninstalled but entry still visible" ghost states.
- `UpgradeAsync` (hot replacement): runs the full unload flow on the old version and **only copies the new package when unload verification passed**, then re-enables it; otherwise the upgrade is aborted (otherwise every upgrade would leak another assembly).
- Skills / MCP servers registered through `IPluginHost` land as **workspace-level** files, so uninstall must delete them explicitly; otherwise the host's `LoadFromWorkspace()` will simply load them back.
- **Plugins must not use the reflection-based JSON serializer**: `JsonSerializer.Deserialize<T>` / `Serialize<T>` (with `T` being a plugin-owned type) caches reflection metadata (`JsonTypeInfo` / member accessors) keyed by that plugin type **inside the default ALC**, pinning the plugin ALC forever so the step-⑥ weak-reference verification always fails. Use `System.Text.Json` **source generation** (`JsonSerializerContext` + `[JsonSerializable]`) or the DOM APIs (`JsonDocument` / `Utf8JsonWriter`) instead; plugins should also avoid creating their own `JsonSerializerOptions` (measured: the reflection path pins the ALC, while source generation and DOM both collect).

## Supported AI Providers

| Provider | Display Name | Supported Models |
|----------|-------------|-----------------|
| openai | OpenAI | gpt-4.1, gpt-4o, gpt-4-turbo, o1, o3-mini, etc. |
| azure | Azure OpenAI | gpt-4o, gpt-4-turbo, gpt-35-turbo, etc. |
| deepseek | DeepSeek | deepseek-chat, deepseek-coder, deepseek-reasoner |
| kimi | Kimi | k3, k3-256k, kimi-for-coding, kimi-for-coding-highspeed |
| glm | Zhipu GLM | glm-4-plus, glm-4-air, glm-4-flash, etc. |
| qwen | Qwen | qwen-turbo, qwen-plus, qwen-max, etc. |
| doubao | Doubao | doubao-pro-4k, doubao-pro-32k, doubao-lite-4k, etc. |
| claude | Claude | claude-3-5-sonnet, claude-3-5-haiku, claude-3-opus, etc. |
| gemini | Google Gemini | gemini-2.0-flash, gemini-1.5-pro, gemini-1.5-flash, etc. |
| ollama | Ollama (Local) | llama3.1, llama3.2, qwen2.5, deepseek-coder-v2, etc. |
| ernie | Baidu ERNIE | ernie-4.0-turbo-8k, ernie-4.0-8k, etc. |
| minimax | MiniMax | abab6.5s-chat, abab6.5-chat, etc. |
| hunyuan | Tencent Hunyuan | hunyuan-pro, hunyuan-standard, etc. |
| mimo | Xiaomi MiMo | mimo-v1, mimo-v1-32k, etc. |
| xai | xAI Grok | grok-2, grok-2-mini, grok-beta |
| qianfan | Baidu Qianfan | ernie-4.0-8k, ernie-speed-128k, etc. |
| tencent-ti | Tencent TI Platform | hunyuan-pro, hunyuan-standard, etc. |
| huawei-pangu | Huawei Pangu | pangu-7b, pangu-13b, pangu-52b |
| bedrock | AWS Bedrock | anthropic.claude-3-sonnet, etc. |
| openrouter | OpenRouter | openai/gpt-4o, anthropic/claude-3.5-sonnet, etc. |

## Project Structure

```
LuBan.AIAgent/
├── Abstractions/
│   ├── ILuBanToolPlugin.cs            # Tool plugin interface
│   ├── ToolPluginRegistry.cs          # Plugin registry
│   ├── ToolConfirmationService.cs     # Tool execution confirmation service
│   ├── ToolAttribute.cs               # Tool attribute
│   ├── ToolResult.cs                  # Tool result
│   └── IIdentifiable.cs               # Identifiable component interface
├── Configuration/
│   ├── IAppConfigReader.cs            # Application config read-only interface
│   ├── IProviderRouter.cs             # Provider routing interface
│   ├── CustomSkillConfig.cs           # Custom skill configuration
│   ├── CustomRuleConfig.cs            # Custom rule configuration
│   ├── McpServerConfig.cs             # External MCP server config
│   ├── ConnectorConfig.cs             # Connector declaration config (config.json connectors[])
│   ├── LuBanAgentOptions.cs           # Agent configuration options
│   ├── BrowserToolOptions.cs          # Browser tool options
│   ├── FileSystemToolOptions.cs       # File system tool options
│   ├── ScriptToolOptions.cs           # Script tool options
│   ├── WebToolOptions.cs              # Web tool options
│   ├── GitToolOptions.cs              # Git tool options
│   ├── RetrievalToolOptions.cs        # Retrieval tool options
│   ├── LocalMemoryOptions.cs          # Local memory options
│   ├── ModelEndpointOptions.cs        # Model endpoint options
│   ├── OrchestrationOptions.cs        # Orchestration options
│   ├── WikiToolOptions.cs             # Wiki tool options
│   └── ToolGroupOptions.cs            # Tool group configuration
├── Infrastructure/
│   ├── PlaywrightSession.cs           # Playwright session management
│   ├── ProcessRunner.cs               # Process executor
│   ├── LuaScriptRunner.cs             # Embedded MoonSharp Lua sandbox executor
│   ├── ShellEnvironmentDetector.cs    # Shell runtime detection and command builder
│   └── PathGuard.cs                   # Path security guard
├── Tools/
│   ├── Browser/BrowserToolPlugin.cs   # Browser tools
│   ├── FileSystem/FileSystemToolPlugin.cs  # File system tools
│   ├── FileSystem/EditOperation.cs    # File edit operation model
│   ├── Script/ScriptToolPlugin.cs     # Script execution tools
│   ├── Git/                           # Git version control tools
│   │   └── GitToolPlugin.cs           # Git tool plugin and tool group
│   ├── Web/WebToolPlugin.cs           # Web tools
│   ├── Context/
│   │   └── CompactContextToolPlugin.cs # Context compaction tools
│   ├── LocalMemory/LocalMemoryToolPlugin.cs  # Local memory tools
│   ├── Retrieval/RetrievalToolPlugin.cs # Semantic retrieval tools
│   ├── Wiki/                          # LLM Wiki knowledge base tools
│   │   ├── WikiToolPlugin.cs          # wiki tool plugin (opt-in)
│   │   └── WikiToolGroup.cs           # wiki tool implementation
│   └── Orchestration/                 # Orchestration tools
│       ├── OrchestrationToolPlugin.cs # Orchestration tool plugin
│       └── OrchestrationToolGroup.cs  # Orchestration tool group
├── Skills/
│   ├── ISkill.cs                      # Skill interface (with PromptTemplate)
│   ├── SkillBase.cs                   # Skill base class
│   ├── SkillRegistry.cs               # Skill registry (merges multiple sources)
│   ├── SkillLoader.cs                 # SKILL.md file loader
│   ├── SkillMdParser.cs               # SKILL.md parser
│   ├── FileSkill.cs                   # File-based Skill adapter
│   ├── CustomSkill.cs                 # Custom Skill adapter
│   └── BuiltIn/
│       ├── BrainstormingSkill.cs      # Brainstorming
│       ├── CodeReviewSkill.cs         # Code review
│       ├── DocumentationSkill.cs      # Documentation generation
│       ├── CodeRefactorSkill.cs       # Code refactoring
│       ├── TestGenerationSkill.cs     # Test generation
│       ├── CodeExplainSkill.cs        # Code explanation
│       ├── DebugAssistantSkill.cs     # Debug assistant
│       ├── GitCommitSkill.cs          # Git commit
│       └── FindSkillsSkill.cs         # Skill discovery
├── Rules/
│   ├── IRule.cs                       # Rule interface
│   ├── RuleBase.cs                    # Rule base class
│   ├── RuleEngine.cs                  # Rule engine
│   ├── RuleCheckedAIFunction.cs       # Rule-checking decorator
│   ├── CustomRule.cs                  # Custom rule adapter
│   └── BuiltIn/
│       └── PathAccessRule.cs          # Path access rule
├── MCP/
│   ├── IMCPClient.cs                  # MCP client interface
│   ├── MCPClientBase.cs               # Client base class
│   ├── StdioMCPClient.cs              # stdio JSON-RPC client
│   ├── HttpMCPClient.cs               # http / sse client
│   ├── MCPClientFactory.cs            # External client factory (stdio/http/sse dispatch in one place)
│   ├── MCPRegistry.cs                 # MCP registry
│   ├── MCPToolPlugin.cs               # MCP tool plugin
│   └── BuiltIn/
│       └── FileSystemMCPClient.cs     # File system MCP client
├── Connectors/                        # Connector system (abstraction in the framework, assembled from config, no name switch)
│   ├── IConnector.cs                  # Connector contract + MCP exposure + factory inputs/settings
│   ├── ConnectorFactoryRegistry.cs    # Connector factory table + host-level singleton instance cache
│   ├── ConnectorRegistry.cs           # Scoped resolver: declaration/enablement/4-state catalog/MCP derivation
│   ├── ConnectorServiceCollectionExtensions.cs # AddConnector / AddBuiltinConnectors
│   └── AliyunDevOps/                  # Aliyun DevOps connector (ProviderKey: aliyunDevOps, instance name yunxiao)
│       ├── YunxiaoConnector.cs        # IConnector implementation + MCP exposure
│       ├── YunxiaoApiOptions.cs       # Organization ID / domain / timeout
│       ├── YunxiaoServiceCollectionExtensions.cs # Connector DI registration extension
│       ├── IYunxiaoCredentialProvider.cs / PatCredential.cs # Credential source
│       ├── CodeupClient.cs            # Repository I/O
│       ├── FlowClient.cs              # Pipeline I/O
│       ├── YunxiaoErrorMapper.cs / YunxiaoModels.cs # Error mapping and data models
│       └── Mcp/                       # In-process MCP client and tool definitions
│           ├── YunxiaoMCPClient.cs    # Yunxiao MCP client (borrows the connector, never disposes it)
│           ├── YunxiaoToolDefinitions.cs # Tool schemas (code-defined)
│           └── MCPRegistryExtensions.cs  # Register/unregister semantic aliases (pass-through)
├── Attachments/
│   ├── IAttachmentProcessor.cs        # Attachment processor interface
│   ├── DefaultAttachmentProcessor.cs  # Default implementation (images/text)
│   ├── AttachmentInfo.cs              # Attachment metadata
│   ├── AttachmentKind.cs              # Attachment kind enum
│   ├── ProcessedAttachment.cs         # Processing result
│   └── AttachmentMessageBuilder.cs    # Attachment → ChatMessage builder
├── Sessions/
│   ├── ISessionManager.cs             # Session manager interface
│   ├── AttachmentRecord.cs            # Attachment persistence record
│   └── SessionChatHistoryProvider.cs  # Session history provider
├── Retrieval/
│   ├── IRetrievalService.cs           # Semantic retrieval interface
│   ├── RetrievalService.cs            # Retrieval service
│   ├── IndexProgress.cs               # Indexing progress model (stage/completed/total/current file)
│   └── Chunkers/                     # Code chunkers
├── Wiki/                              # LLM Wiki knowledge base service
│   ├── IWikiService.cs                # Wiki service interface
│   ├── IWikiContext.cs                # Workspace context interface
│   ├── WikiService.cs                 # Wiki service implementation (index/log/index maintenance)
│   ├── WikiDefaults.cs                # Default schema text
│   ├── WikiFrontmatter.cs             # YAML frontmatter parse/render
│   ├── WikiPageSerializer.cs          # Page serializer
│   ├── WikiSlug.cs                    # File-name slug generation
│   ├── Models.cs                      # Page/index/lint models
│   └── Extractors/                    # Source extractors (txt/md/json/csv/xml/html/xlsx)
├── LocalMemory/
│   ├── ILocalMemoryService.cs         # Local memory service interface
│   ├── ILocalMemoryStore.cs           # Local memory store interface
│   ├── IWorkspaceContextProvider.cs   # Workspace context provider interface
│   ├── LocalMemoryService.cs          # Local memory service
│   ├── MemoryCategories.cs            # Memory categories
│   └── MemoryEntry.cs                 # Memory entry model
├── Utils/Text/
│   ├── TextUtils.cs                   # Text processing utilities
│   ├── NGramExtractor.cs              # N-gram extractor
│   └── WildcardMatcher.cs             # Wildcard matching
├── Orchestration/                     # Multi-Agent orchestration subsystem
│   ├── IOrchestrator.cs               # Orchestrator interface
│   ├── Orchestrator.cs                # Orchestrator default implementation
│   ├── DagScheduler.cs                # DAG scheduler (topological layer parallel)
│   ├── SubAgentFactory.cs             # SubAgent factory
│   ├── SubAgentRoleRegistry.cs        # SubAgent role registry
│   ├── ContextStore.cs                # Cross-node context store
│   ├── Models/                        # Data models
│   │   ├── TaskGraph.cs               # Task graph
│   │   ├── TaskNode.cs                # Task node
│   │   ├── TaskNodeStatus.cs          # Node status enum
│   │   ├── SubAgentSpec.cs            # SubAgent specification
│   │   ├── SubAgentRole.cs            # SubAgent role definition
│   │   ├── NodeResult.cs              # Node result
│   │   ├── OrchestrationResult.cs     # Orchestration result
│   │   ├── OrchestrationProgress.cs   # Progress event
│   │   ├── ProgressEventType.cs       # Progress event type
│   │   └── ReflectionResult.cs        # Reflection result and replan context
│   ├── Planner/                       # Task planners
│   │   ├── ITaskPlanner.cs            # Planner interface
│   │   └── LlmTaskPlanner.cs          # LLM planner
│   ├── GraphPlanStore.cs              # Task graph store
│   ├── IOrchestrationProgressSink.cs  # Orchestration progress outlet
│   └── Exceptions/                    # Exception definitions
│       ├── TaskPlanningException.cs   # Planning exception
│       └── NodeExecutionException.cs  # Node execution exception
├── Plugins/                           # Plugin system (application-level plugins: manifest + collectible ALC lifecycle)
│   ├── Base/
│   │   ├── PluginBase.cs              # Plugin base class (Host/SafeLog/PostToUi + init/shutdown hooks)
│   │   └── BuiltinPluginCatalog.cs    # Built-in plugin catalog (shipped with the host)
│   ├── Contracts/                     # Contracts: IPlugin / IPluginHost / IPluginPanel / state repository
│   │   ├── IPlugin.cs                 # Plugin contract
│   │   ├── IPluginHost.cs             # Host capability contract (sidebar/panel/Skill/MCP/logging/UI callback)
│   │   ├── IPluginPanel.cs            # Plugin panel contract (UI-agnostic)
│   │   ├── IPluginStateRepository.cs  # State persistence contract
│   │   ├── PluginManifest.cs          # plugin.json manifest model
│   │   ├── PluginPermissions.cs       # Permission declarations (6 dimensions + array/object JSON converter)
│   │   ├── PluginSidebarEntry.cs      # Sidebar entry declaration
│   │   ├── PluginCommandDescriptor.cs # Command declaration
│   │   ├── SkillDefinition.cs         # Skill declaration contributed by a plugin
│   │   └── PluginState.cs             # Lifecycle state enum (4 states)
│   ├── Loading/
│   │   ├── PluginLoader.cs            # Loading entry (manifest + entry assembly + IPlugin instance)
│   │   ├── PluginFactory.cs           # IPlugin instantiation via reflection (with IL2067 suppression)
│   │   ├── PluginLocation.cs          # Directory convention and resolution (application level only)
│   │   ├── ManifestValidator.cs       # Manifest validation
│   │   └── PluginDependencyResolver.cs# Dependency topology and cycle detection
│   ├── Lifecycle/
│   │   ├── PluginManager.cs           # Scan/enable/disable/install/upgrade/uninstall orchestration + 7-step unload and GC verification
│   │   ├── PluginLoadContext.cs       # Collectible AssemblyLoadContext (lib/ first, falls back to the root)
│   │   ├── PluginHostScope.cs         # Host capability proxy + contribution bookkeeping
│   │   └── PluginStateRepository.cs   # Default JSON state repository
│   ├── Security/
│   │   ├── PluginSecurityOptions.cs   # Permission whitelist policy (network/process/Git/filesystem/credentials)
│   │   ├── PluginPermissionEvaluator.cs # Declaration-time coverage check + runtime boundary decisions
│   │   ├── PluginPermissionGuard.cs   # Runtime egress guard + bounded violation audit
│   │   ├── PluginScopeContext.cs      # Permission environment (AsyncLocal) propagation
│   │   ├── CredentialKeys.cs          # Credential key normalization and environment-variable mapping
│   │   ├── ICredentialStore.cs        # Credential storage contract
│   │   └── ConfigurationCredentialStore.cs # Default read-only "configuration + environment variable" store
│   └── Schemas/
│       └── plugin.schema.json         # JSON Schema for plugin.json (shipped in the package)
├── LuBanAgent.cs                      # Agent instance
├── LuBanAgentFactory.cs               # Agent factory
├── LuBanAgentExtensions.cs            # DI extension methods
├── SanitizingChatClient.cs            # Chat client sanitizer
├── AIFunctionFactoryHelper.cs         # AI function factory helper
└── ILuBanAgentFactory.cs              # Agent factory interface
```

## Tips

- Model routing uses `provider:model` format; configure providers via `IAppConfigReader` / host implementation
- **12 built-in tool groups** cover browser automation, file operations, script execution, Git version control, web requests, semantic retrieval, context compaction, local memory, MCP, multi-agent orchestration, the LLM Wiki knowledge base (opt-in), and large log analysis
- `ToolConfirmationService` requires user confirmation for out-of-workspace path access and for sensitive operations (write, delete, execute); in-workspace operations are allowed directly
- Three-tier grant memory (this time / this turn / this session) keyed by grant root; deletion tools (`AlwaysConfirmTools`) are never remembered and always confirm
- `FileSystemToolOptions.AllowedRoots` together with workspace roots forms the trusted scope; operations inside it need no confirmation
- **Session history auto-persistence** with compression (SummarizingChatReducer), context never lost
- **Custom Skill/Rule/MCP persistence**, configs saved locally and auto-loaded on restart
- **File-based Skills**: define custom Skills via SKILL.md files, compatible with OpenCode format, automatically loaded from project-level/user-level directories
- **Rule interception** checks before tool execution, supports deny/allow/modify
- **MCP tool integration**, external MCP server tools exposed to Agent
- External tool plugin assemblies can be hot-loaded via `ExternalPlugins` configuration
- **Plugin system**: application-level plugins (`plugins/<id>/` + `plugin.json`) use a collectible ALC and support enable/disable/upgrade/uninstall; uninstall breaks references in a fixed 7-step order and verifies them with GC weak references, degrading to `Disabled` with a restart hint when verification fails
- Combine with LuBan.AIFlow to connect to RagFlow / Dify / Coze and other AI platforms
- **Multi-Agent Orchestration**: composite tasks decomposed into DAG, SubAgents execute serially/parallelly with skip-on-failure, timeout, and context passing
- **Attachment Support**: `IAttachmentProcessor` handles images and text files uniformly; images are resized locally and sent as base64 `DataContent`, large text injects only a path hint, and sessions persist paths/metadata only

## License

MIT
