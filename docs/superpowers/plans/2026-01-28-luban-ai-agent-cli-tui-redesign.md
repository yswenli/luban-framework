# LuBan.AIAgent.ConsoleApp TUI 界面重新设计实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 使用 Terminal.Gui 库重新设计 LuBan.AIAgent.ConsoleApp 的控制台界面，实现分区布局、多行输入和现代化 TUI 体验。

**Architecture:** 采用 Terminal.Gui 框架构建 TUI 界面，将界面分为 Header、SessionPanel、DisplayView、InputView 和 StatusBar 五个区域。使用依赖注入集成现有服务，通过 Application.MainLoop.Invoke() 确保线程安全。

**Tech Stack:** Terminal.Gui, .NET 8, C#

---

## 文件结构

```
LuBan.AIAgent.ConsoleApp/
├── UI/
│   ├── MainView.cs          # 主视图，协调所有子视图
│   ├── HeaderView.cs        # Header 区域（GetLBFName() 输出）
│   ├── SessionPanel.cs      # 左侧 Session 信息面板
│   ├── DisplayView.cs       # 右侧显示区（对话历史）
│   ├── InputView.cs         # 右侧输入区（多行编辑）
│   └── StatusBarManager.cs  # 状态栏管理
├── Commands/                # 保留现有命令，修改 ChatCommand 适配新 UI
├── Services/                # 保留现有服务
└── Program.cs               # 修改为 Terminal.Gui 初始化流程
```

---

### Task 1: 添加 Terminal.Gui 依赖

**Files:**
- Modify: `LuBan.AIAgent.ConsoleApp/LuBan.AIAgent.ConsoleApp.csproj`

- [ ] **Step 1: 添加 Terminal.Gui 包引用**

编辑 `LuBan.AIAgent.ConsoleApp/LuBan.AIAgent.ConsoleApp.csproj`，在 `<ItemGroup>` 中添加：

```xml
<PackageReference Include="Terminal.Gui" Version="1.17.1" />
```

- [ ] **Step 2: 恢复依赖**

```bash
dotnet restore LuBan.AIAgent.ConsoleApp/LuBan.AIAgent.ConsoleApp.csproj
```

Expected: 成功恢复依赖，无错误

- [ ] **Step 3: 提交**

```bash
git add LuBan.AIAgent.ConsoleApp/LuBan.AIAgent.ConsoleApp.csproj
git commit -m "feat: add Terminal.Gui dependency"
```

---

### Task 2: 创建 HeaderView 组件

**Files:**
- Create: `LuBan.AIAgent.ConsoleApp/UI/HeaderView.cs`

- [ ] **Step 1: 创建 HeaderView 类**

```csharp
using Terminal.Gui;
using LuBan.Common;

namespace LuBan.AIAgent.ConsoleApp.UI;

/// <summary>
/// Header 视图，显示 LuBan Framework 名称
/// </summary>
public class HeaderView : View
{
    public HeaderView()
    {
        Height = 6;
        CanFocus = false;
    }

    public override void Redraw(Rectangle bounds)
    {
        base.Redraw(bounds);
        
        var name = ConsoleUtil.GetLBFName();
        var lines = name.Split('\n');
        
        for (int i = 0; i < Math.Min(lines.Length, 6); i++)
        {
            Move(0, i);
            Driver.AddStr(lines[i], ColorScheme.Focus);
        }
    }
}
```

- [ ] **Step 2: 提交**

```bash
git add LuBan.AIAgent.ConsoleApp/UI/HeaderView.cs
git commit -m "feat: add HeaderView component"
```

---

### Task 3: 创建 SessionPanel 组件

**Files:**
- Create: `LuBan.AIAgent.ConsoleApp/UI/SessionPanel.cs`

- [ ] **Step 1: 创建 SessionPanel 类**

```csharp
using Terminal.Gui;
using LuBan.AIAgent.Sessions;

namespace LuBan.AIAgent.ConsoleApp.UI;

/// <summary>
/// 左侧 Session 信息面板
/// </summary>
public class SessionPanel : FrameView
{
    private readonly Label _titleLabel;
    private readonly Label _messageCountLabel;
    private readonly Label _tokenCountLabel;
    private readonly Label _createTimeLabel;

    public SessionPanel()
    {
        Title = "Session 信息";
        Width = Dim.Fill() / 5;
        
        _titleLabel = new Label("标题: 未命名") { Y = 1 };
        _messageCountLabel = new Label("消息数: 0") { Y = 3 };
        _tokenCountLabel = new Label("Token: 0") { Y = 4 };
        _createTimeLabel = new Label("创建: -") { Y = 5 };
        
        Add(_titleLabel, _messageCountLabel, _tokenCountLabel, _createTimeLabel);
    }

    public void Refresh(SessionInfo? session)
    {
        if (session == null)
        {
            _titleLabel.Text = "标题: 未命名";
            _messageCountLabel.Text = "消息数: 0";
            _tokenCountLabel.Text = "Token: 0";
            _createTimeLabel.Text = "创建: -";
        }
        else
        {
            _titleLabel.Text = $"标题: {session.Title ?? "未命名"}";
            _messageCountLabel.Text = $"消息数: {session.MessageCount}";
            _tokenCountLabel.Text = $"Token: {session.TotalTokens}";
            _createTimeLabel.Text = $"创建: {session.CreatedAt:MM-dd HH:mm}";
        }
        
        SetNeedsDisplay();
    }
}
```

- [ ] **Step 2: 提交**

```bash
git add LuBan.AIAgent.ConsoleApp/UI/SessionPanel.cs
git commit -m "feat: add SessionPanel component"
```

---

### Task 4: 创建 DisplayView 组件

**Files:**
- Create: `LuBan.AIAgent.ConsoleApp/UI/DisplayView.cs`

- [ ] **Step 1: 创建 DisplayView 类**

```csharp
using Terminal.Gui;

namespace LuBan.AIAgent.ConsoleApp.UI;

/// <summary>
/// 右侧显示区，显示对话历史
/// </summary>
public class DisplayView : TextView
{
    public DisplayView()
    {
        ReadOnly = true;
        WordWrap = true;
        AllowsTab = false;
    }

    public void AppendMessage(string role, string content)
    {
        var color = role switch
        {
            "user" => Color.Cyan,
            "assistant" => Color.Green,
            "tool" => Color.Yellow,
            "thinking" => Color.Gray,
            _ => Color.White
        };
        
        var prefix = role switch
        {
            "user" => "你: ",
            "assistant" => "AI: ",
            "tool" => "工具: ",
            "thinking" => "💭 ",
            _ => ""
        };
        
        var message = $"{prefix}{content}\n\n";
        
        MoveToEnd();
        InsertText(message);
        
        // 自动滚动到底部
        ScrollTo(0, LineCount - 1);
    }

    public void Clear()
    {
        Text = "";
        SetNeedsDisplay();
    }
    
    private void MoveToEnd()
    {
        Move(0, LineCount);
    }
}
```

- [ ] **Step 2: 提交**

```bash
git add LuBan.AIAgent.ConsoleApp/UI/DisplayView.cs
git commit -m "feat: add DisplayView component"
```

---

### Task 5: 创建 InputView 组件

**Files:**
- Create: `LuBan.AIAgent.ConsoleApp/UI/InputView.cs`

- [ ] **Step 1: 创建 InputView 类**

```csharp
using Terminal.Gui;

namespace LuBan.AIAgent.ConsoleApp.UI;

/// <summary>
/// 右侧输入区，支持多行输入
/// </summary>
public class InputView : TextView
{
    public event Action<string>? OnSend;
    
    public InputView()
    {
        Height = 5;
        WordWrap = true;
        AllowsTab = false;
        
        KeyDown += InputView_KeyDown;
    }

    private void InputView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyEvent.Key == Key.Enter)
        {
            if (e.KeyEvent.IsShift)
            {
                // Shift+Enter: 插入换行
                InsertText("\n");
                e.Handled = true;
            }
            else
            {
                // Enter: 发送
                var input = Text?.Trim();
                if (!string.IsNullOrEmpty(input))
                {
                    OnSend?.Invoke(input);
                    Text = "";
                    SetNeedsDisplay();
                }
                e.Handled = true;
            }
        }
    }
}
```

- [ ] **Step 2: 提交**

```bash
git add LuBan.AIAgent.ConsoleApp/UI/InputView.cs
git commit -m "feat: add InputView component with multi-line support"
```

---

### Task 6: 创建 StatusBarManager 组件

**Files:**
- Create: `LuBan.AIAgent.ConsoleApp/UI/StatusBarManager.cs`

- [ ] **Step 1: 创建 StatusBarManager 类**

```csharp
using Terminal.Gui;

namespace LuBan.AIAgent.ConsoleApp.UI;

/// <summary>
/// 状态栏管理器
/// </summary>
public class StatusBarManager
{
    private readonly StatusBar _statusBar;
    private readonly StatusItem _modelItem;
    private readonly StatusItem _connectionItem;

    public StatusBarManager(StatusBar statusBar)
    {
        _statusBar = statusBar;
        
        _modelItem = new StatusItem(Key.Unknown, "模型: -", null);
        _connectionItem = new StatusItem(Key.Unknown, "连接: -", null);
        
        _statusBar.Items = new[]
        {
            _modelItem,
            _connectionItem,
            new StatusItem(Key.F1, "F1:帮助", null),
            new StatusItem(Key.F2, "F2:切换会话", null),
            new StatusItem(Key.F10, "F10:退出", null)
        };
    }

    public void UpdateModel(string modelName)
    {
        _modelItem.Title = $"模型: {modelName}";
        _statusBar.SetNeedsDisplay();
    }

    public void UpdateConnection(bool connected)
    {
        _connectionItem.Title = connected ? "连接: ✓" : "连接: ✗";
        _statusBar.SetNeedsDisplay();
    }
}
```

- [ ] **Step 2: 提交**

```bash
git add LuBan.AIAgent.ConsoleApp/UI/StatusBarManager.cs
git commit -m "feat: add StatusBarManager component"
```

---

### Task 7: 创建 MainView 主视图

**Files:**
- Create: `LuBan.AIAgent.ConsoleApp/UI/MainView.cs`

- [ ] **Step 1: 创建 MainView 类**

```csharp
using Terminal.Gui;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Sessions;
using LuBan.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.AIAgent.ConsoleApp.UI;

/// <summary>
/// 主视图，协调所有子视图
/// </summary>
public class MainView : Window
{
    private readonly HeaderView _headerView;
    private readonly SessionPanel _sessionPanel;
    private readonly DisplayView _displayView;
    private readonly InputView _inputView;
    private readonly StatusBarManager _statusBarManager;
    
    private readonly ISessionManager _sessionManager;
    private readonly ConfigManager _configManager;
    private readonly IServiceProvider _serviceProvider;

    public MainView(
        ISessionManager sessionManager,
        ConfigManager configManager,
        IServiceProvider serviceProvider)
    {
        Title = "LuBan.AIAgent.CLI";
        
        _sessionManager = sessionManager;
        _configManager = configManager;
        _serviceProvider = serviceProvider;

        // Header
        _headerView = new HeaderView { Y = 0, Height = 6 };
        Add(_headerView);

        // Session Panel (左侧)
        _sessionPanel = new SessionPanel { Y = Pos.Bottom(_headerView) };
        Add(_sessionPanel);

        // Display View (右侧上部)
        _displayView = new DisplayView
        {
            X = Pos.Right(_sessionPanel),
            Y = Pos.Bottom(_headerView),
            Width = Dim.Fill(),
            Height = Dim.Fill(5) // 减去 InputView 高度
        };
        Add(_displayView);

        // Input View (右侧下部)
        _inputView = new InputView
        {
            X = Pos.Right(_sessionPanel),
            Y = Pos.Bottom(_displayView),
            Width = Dim.Fill()
        };
        _inputView.OnSend += OnSendMessage;
        Add(_inputView);

        // StatusBar
        var statusBar = new StatusBar();
        _statusBarManager = new StatusBarManager(statusBar);
        Add(statusBar);

        // 初始化状态栏
        _statusBarManager.UpdateModel(configManager.SelectedModel ?? "未选择");
        _statusBarManager.UpdateConnection(true);

        // 刷新 Session 面板
        _sessionPanel.Refresh(sessionManager.CurrentSession);

        // 快捷键
        KeyDown += MainView_KeyDown;
    }

    private void MainView_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyEvent.Key)
        {
            case Key.F1:
                ShowHelp();
                e.Handled = true;
                break;
            case Key.F2:
                ShowSessionSwitch();
                e.Handled = true;
                break;
            case Key.F10:
            case Key.ControlMask | Key.Q:
                Application.RequestStop();
                e.Handled = true;
                break;
            case Key.ControlMask | Key.L:
                _displayView.Clear();
                e.Handled = true;
                break;
        }
    }

    private async void OnSendMessage(string input)
    {
        if (input.StartsWith('/'))
        {
            // 命令模式
            _displayView.AppendMessage("user", input);
            // TODO: 调用命令分发逻辑
        }
        else
        {
            // 对话模式
            _displayView.AppendMessage("user", input);
            await ProcessChatMessage(input);
        }
    }

    private async Task ProcessChatMessage(string input)
    {
        try
        {
            _statusBarManager.UpdateConnection(true);
            
            var agentFactory = _serviceProvider.GetRequiredService<ILuBanAgentFactory>();
            var agent = await agentFactory.CreateAsync(
                modelName: _configManager.SelectedModel,
                systemPrompt: "你是一个智能助手。");

            var response = await agent.RunAsync(input);
            
            Application.MainLoop.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(response.Text))
                {
                    _displayView.AppendMessage("assistant", response.Text);
                }
                
                // 刷新 Session 面板
                _sessionPanel.Refresh(_sessionManager.CurrentSession);
            });
        }
        catch (Exception ex)
        {
            Application.MainLoop.Invoke(() =>
            {
                _statusBarManager.UpdateConnection(false);
                _displayView.AppendMessage("error", $"错误: {ex.Message}");
            });
        }
    }

    private void ShowHelp()
    {
        var help = @"快捷键:
F1 - 显示帮助
F2 - 切换会话
F10 / Ctrl+Q - 退出
Ctrl+L - 清屏

输入:
Enter - 发送消息
Shift+Enter - 换行
/命令 - 执行命令";

        MessageBox.Query("帮助", help, "确定");
    }

    private void ShowSessionSwitch()
    {
        MessageBox.Query("提示", "会话切换功能待实现", "确定");
    }
}
```

- [ ] **Step 2: 提交**

```bash
git add LuBan.AIAgent.ConsoleApp/UI/MainView.cs
git commit -m "feat: add MainView with layout and event handling"
```

---

### Task 8: 修改 Program.cs 使用 Terminal.Gui

**Files:**
- Modify: `LuBan.AIAgent.ConsoleApp/Program.cs`

- [ ] **Step 1: 修改 Main 方法**

```csharp
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.ConsoleApp.Infrastructure;
using LuBan.AIAgent.ConsoleApp.Retrieval;
using LuBan.AIAgent.ConsoleApp.Services;
using LuBan.AIAgent.ConsoleApp.UI;
using LuBan.AIAgent.Retrieval;
using LuBan.AIAgent.Sessions;
using LuBan.Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Terminal.Gui;

namespace LuBan.AIAgent.ConsoleApp;

/// <summary>
/// 程序入口
/// </summary>
class Program
{
    /// <summary>
    /// 程序主入口
    /// </summary>
    static async Task Main(string[] args)
    {
        // 初始化数据库
        DatabaseInitializer.Initialize();

        // 构建配置
        var configuration = BuildConfiguration(args);
        var (embedder, modelManager) = await PrepareRetrievalAsync(configuration);

        // 构建服务
        using var serviceProvider = BuildServiceProvider(configuration, embedder, modelManager);

        // 初始化 Terminal.Gui
        Application.Init();
        
        try
        {
            // 创建主视图
            var sessionManager = serviceProvider.GetRequiredService<ISessionManager>();
            var configManager = serviceProvider.GetRequiredService<ConfigManager>();
            
            var mainView = new MainView(sessionManager, configManager, serviceProvider);
            
            // 运行主循环
            Application.Run(mainView);
        }
        finally
        {
            // 清理
            Application.Shutdown();
        }
    }

    private static IConfiguration BuildConfiguration(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
        return builder.Build();
    }

    private static async Task<(OnnxEmbeddingGenerator? embedder, ModelManager? modelManager)> PrepareRetrievalAsync(IConfiguration configuration)
    {
        var retrieval = configuration.GetSection("LuBanAgent:Tools:Retrieval").Get<RetrievalToolOptions>() ?? new RetrievalToolOptions();
        if (!retrieval.Enabled) return (null, null);
        var spec = EmbeddingModelCatalog.Find(retrieval.ModelId);
        if (spec == null)
        {
            Console.WriteLine($"未知的嵌入模型：{retrieval.ModelId}，检索功能已禁用");
            return (null, null);
        }
        var mm = new ModelManager(spec);
        if (mm.IsModelReady()) return (new OnnxEmbeddingGenerator(mm.ModelDirectory, spec), mm);
        
        // 在 Terminal.Gui 初始化前使用传统方式显示进度
        Console.WriteLine("准备嵌入模型...");
        var ok = await mm.EnsureModelAsync(
            (progress) => Console.Write($"\r进度: {progress:P0}"),
            CancellationToken.None);
        
        if (!ok || !mm.IsModelReady())
        {
            Console.WriteLine();
            Console.WriteLine($"嵌入模型 {spec.ModelId} 未就绪，检索功能已禁用");
            return (null, null);
        }
        return (new OnnxEmbeddingGenerator(mm.ModelDirectory, spec), mm);
    }

    private static ServiceProvider BuildServiceProvider(IConfiguration configuration, OnnxEmbeddingGenerator? embedder, ModelManager? modelManager)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        var configPath = ConfigManager.GetDefaultConfigPath();
        var configManager = new ConfigManager(configPath);
        configManager.Load();
        services.AddSingleton(configManager);

        services.AddScoped<IChatClient>(sp =>
        {
            var cm = sp.GetRequiredService<ConfigManager>();
            return cm.CreateChatClient();
        });

        services.AddLuBanAgent(configuration);

        services.AddSingleton<ISessionManager, SessionManager>();

        if (embedder != null)
        {
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(embedder);
            services.AddSingleton<IVectorStore, SqliteVectorStore>();
            services.AddSingleton<IRetrievalService>(sp => new RetrievalService(
                sp.GetRequiredService<IVectorStore>(),
                sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(),
                sp.GetRequiredService<IOptions<LuBanAgentOptions>>()));
            if (modelManager != null) services.AddSingleton(modelManager);
        }

        services.AddSingleton<ConsoleAppService>();
        return services.BuildServiceProvider();
    }
}
```

- [ ] **Step 2: 编译验证**

```bash
dotnet build LuBan.AIAgent.ConsoleApp/LuBan.AIAgent.ConsoleApp.csproj
```

Expected: 编译成功，无错误

- [ ] **Step 3: 提交**

```bash
git add LuBan.AIAgent.ConsoleApp/Program.cs
git commit -m "feat: migrate to Terminal.Gui TUI framework"
```

---

### Task 9: 测试和验证

- [ ] **Step 1: 运行程序**

```bash
dotnet run --project LuBan.AIAgent.ConsoleApp/LuBan.AIAgent.ConsoleApp.csproj
```

- [ ] **Step 2: 验证界面布局**

检查：
- Header 显示 LuBan Framework 名称
- 左侧 Session 面板显示
- 右侧 DisplayView 和 InputView 显示
- 底部 StatusBar 显示

- [ ] **Step 3: 验证多行输入**

检查：
- Enter 发送消息
- Shift+Enter 换行
- 消息显示在 DisplayView 中

- [ ] **Step 4: 验证快捷键**

检查：
- F1 显示帮助
- F10 或 Ctrl+Q 退出
- Ctrl+L 清屏

- [ ] **Step 5: 提交最终版本**

```bash
git add .
git commit -m "feat: complete TUI redesign with Terminal.Gui"
```

---

## 实施注意事项

1. **Terminal.Gui 版本**: 使用 1.17.1 稳定版
2. **线程安全**: 所有 UI 更新必须通过 `Application.MainLoop.Invoke()` 切换到主线程
3. **依赖注入**: 保持与现有服务的集成方式
4. **渐进式实施**: 先实现核心布局，再完善交互功能
5. **测试**: 手动测试界面布局和交互，确保多行输入和滚动正常工作
