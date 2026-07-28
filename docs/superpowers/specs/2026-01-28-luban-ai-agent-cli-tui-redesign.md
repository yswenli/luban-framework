# LuBan.AIAgent.ConsoleApp TUI 界面重新设计

## 概述

使用 Terminal.Gui 库重新设计 LuBan.AIAgent.ConsoleApp 的控制台界面，实现分区布局、多行输入和现代化 TUI 体验。

## 项目依赖调整

**LuBan.Common：**
- 保留 `Spectre.Console` 引用（其他项目可能依赖）
- `ConsoleUtil.cs` 保持不变

**LuBan.AIAgent.ConsoleApp：**
- 新增 `Terminal.Gui` 依赖
- 不再使用 `ConsoleUtil` 中的 Spectre 相关方法，改用 Terminal.Gui 构建界面

## 界面布局架构

```
┌─────────────────────────────────────────────────────────────┐
│ LuBan.AIAgent.CLI                                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [Header - PrintName() 输出]                                │
│                                                             │
├──────────┬──────────────────────────────────────────────────┤
│ Session  │  [Display Area - 滚动显示区]                     │
│ 信息     │  - 用户输入                                      │
│          │  - AI 回复                                       │
│ 标题     │  - 工具调用过程                                  │
│ 消息数   │  - 思考内容                                      │
│ Token    │                                                  │
│          │                                                  │
│          ├──────────────────────────────────────────────────┤
│          │  [Input Area - 多行输入]                         │
│          │  Shift+Enter 换行，Enter 发送                    │
├──────────┴──────────────────────────────────────────────────┤
│ 模型: gpt-4o │ 连接: ✓ │ F1:帮助 F2:切换会话 F10:退出       │
└─────────────────────────────────────────────────────────────┘
```

**Terminal.Gui 控件映射：**
- `Window` - 主窗口（标题：LuBan.AIAgent.CLI）
- `Label` 或 `View` - Header 区域（PrintName 输出）
- `FrameView` - 左侧 Session 信息面板
- `TextView` - 右侧显示区（只读，支持滚动）
- `TextView` - 右侧输入区（多行编辑）
- `StatusBar` + `StatusItem` - 底部状态栏

**布局计算：**
- Header：固定高度（PrintName 输出约 6 行）
- 左侧面板：固定宽度（Console.WindowWidth / 5）
- 右侧显示区：占据剩余高度的 4/5
- 右侧输入区：固定高度（5 行）
- StatusBar：固定 1 行

## 核心组件与交互逻辑

**核心类结构：**

```
ConsoleApp/
├── UI/
│   ├── MainView.cs          # 主视图，协调所有子视图
│   ├── HeaderView.cs        # Header 区域（PrintName 输出）
│   ├── SessionPanel.cs      # 左侧 Session 信息面板
│   ├── DisplayView.cs       # 右侧显示区（对话历史）
│   ├── InputView.cs         # 右侧输入区（多行编辑）
│   └── StatusBarManager.cs  # 状态栏管理
```

**多行输入实现：**
- 使用 `TextView` 控件，设置 `Height = 5`
- 拦截 `KeyDown` 事件：
  - `Enter` + `Shift` → 插入换行
  - `Enter`（无 Shift）→ 触发发送
  - `Ctrl+C` → 取消当前操作

**显示区更新机制：**
- `DisplayView` 使用 `TextView`（只读模式）
- 提供 `AppendMessage(role, content)` 方法
- 自动滚动到最新消息
- 支持不同角色的颜色标记（用户=青色，AI=绿色，工具=黄色，思考=灰色）

**Session 信息面板刷新：**
- 提供 `Refresh(SessionInfo)` 方法
- 显示：标题、消息数、Token 数、创建时间
- 在会话切换或消息更新时调用

**状态栏动态更新：**
- 模型信息：从 `ConfigManager.SelectedModel` 读取
- 连接状态：定期 ping API 或使用缓存状态
- 快捷键提示：静态文本

**快捷键处理：**
- `F1` → 显示帮助对话框
- `F2` → 打开会话切换对话框
- `F10` 或 `Ctrl+Q` → 退出程序
- `Ctrl+L` → 清屏（清空显示区）

## 数据流与集成

**与现有服务的集成：**

```
MainView
├── 依赖注入
│   ├── ISessionManager    → SessionPanel
│   ├── ConfigManager      → StatusBarManager
│   ├── ConsoleAppService  → 命令分发
│   └── ILuBanAgentFactory → ChatCommand
```

**数据流：**

1. **用户输入 → 发送**
   ```
   InputView.Enter → MainView.OnSend() → ConsoleAppService.TryExecuteCommandAsync()
                                       → ChatCommand.RunChatLoop() → agent.RunAsync()
   ```

2. **AI 响应 → 显示**
   ```
   agent.RunAsync() → ChatCommand 解析响应
                    → MainView.AppendToDisplay()
                    → DisplayView.AppendMessage()
   ```

3. **Session 更新 → 刷新面板**
   ```
   SessionManager.AddMessageAsync() → MainView.RefreshSessionPanel()
                                   → SessionPanel.Refresh()
   ```

**线程安全：**
- Terminal.Gui 的 UI 更新必须在主线程
- 使用 `Application.MainLoop.Invoke()` 从后台线程切换到 UI 线程
- Agent 调用在后台线程执行，完成后回调 UI

**命令兼容性：**
- 保留现有 `/chat`、`/session`、`/skill` 等命令
- 在主界面直接输入文本时，默认进入对话模式
- 输入 `/` 开头的命令时，走命令分发逻辑

**错误处理：**
- API 调用失败 → 在 DisplayView 显示红色错误信息
- 网络超时 → 状态栏显示"连接中..."，超时后显示"连接失败"
- 无效输入 → 在 DisplayView 显示提示信息

## 实施要点

1. **依赖管理**：仅在 ConsoleApp 项目添加 Terminal.Gui，避免影响其他项目
2. **渐进式迁移**：先实现核心布局，再逐步完善交互功能
3. **测试策略**：手动测试界面布局和交互，确保多行输入和滚动正常工作
4. **文档更新**：更新 README.md，说明新的界面布局和快捷键
