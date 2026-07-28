using Terminal.Gui;
using LuBan.AIAgent;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.ConsoleApp.Services;
using LuBan.AIAgent.Services;
using LuBan.AIAgent.Sessions;
using LuBan.Common;
using Microsoft.Extensions.AI;
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
    private readonly ConsoleAppService _consoleAppService;
    
    private LuBanAgent? _agent;

    public MainView(
        ISessionManager sessionManager,
        ConfigManager configManager,
        IServiceProvider serviceProvider,
        ConsoleAppService consoleAppService)
    {
        Title = "LuBan.AIAgent.CLI";
        
        _sessionManager = sessionManager;
        _configManager = configManager;
        _serviceProvider = serviceProvider;
        _consoleAppService = consoleAppService;

        _headerView = new HeaderView { Y = 0, Height = 6 };
        Add(_headerView);

        _sessionPanel = new SessionPanel { Y = Pos.Bottom(_headerView) };
        Add(_sessionPanel);

        _displayView = new DisplayView
        {
            X = Pos.Right(_sessionPanel),
            Y = Pos.Bottom(_headerView),
            Width = Dim.Fill(),
            Height = Dim.Fill(5)
        };
        Add(_displayView);

        _inputView = new InputView
        {
            X = Pos.Right(_sessionPanel),
            Y = Pos.Bottom(_displayView),
            Width = Dim.Fill()
        };
        _inputView.OnSend += OnSendMessage;
        Add(_inputView);

        var statusBar = new StatusBar();
        _statusBarManager = new StatusBarManager(statusBar);
        Add(statusBar);

        _statusBarManager.UpdateModel(configManager.SelectedModel ?? "未选择");
        _statusBarManager.UpdateConnection(true);

        _sessionPanel.Refresh(sessionManager.CurrentSession);

        KeyDown += MainView_KeyDown;
        
        InitializeAgentAsync();
    }

    private async void InitializeAgentAsync()
    {
        try
        {
            var agentFactory = _serviceProvider.GetRequiredService<ILuBanAgentFactory>();
            _agent = await agentFactory.CreateAsync(
                modelName: _configManager.SelectedModel,
                systemPrompt: @"你是一个智能助手，拥有以下工具能力：

1. **文件系统操作**：可以读取文件、写入文件、列出目录内容
2. **脚本执行**：可以执行 Shell、Python、Lua 等脚本
3. **浏览器自动化**：可以打开网页、点击元素、输入文本、截图
4. **数据库操作**：可以执行 SQL 查询
5. **Redis 操作**：可以执行 Redis 命令
6. **Web 请求**：可以发送 HTTP 请求

当用户的请求涉及上述操作时，**必须使用相应的工具**来完成，不要说'我无法访问'或'我没有这个能力'。

请立即使用工具来帮助用户完成任务。");

            ToolConfirmationService.ConfirmationCallback = (toolName, args) =>
            {
                var confirmed = false;
                Application.MainLoop.Invoke(() =>
                {
                    var message = $"危险操作请求: {toolName}\n\n参数:\n{ToolConfirmationService.FormatArguments(args, 500)}\n\n是否执行此操作？";
                    confirmed = MessageBox.Query("确认", message, "是", "否") == 0;
                });
                
                // 等待用户响应
                Thread.Sleep(100);
                return confirmed;
            };
        }
        catch (Exception ex)
        {
            Application.MainLoop.Invoke(() =>
            {
                _displayView.AppendMessage("error", $"初始化 Agent 失败: {ex.Message}");
            });
        }
    }

    private void MainView_KeyDown(View.KeyEventEventArgs e)
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
            case Key.Q | Key.CtrlMask:
                Application.RequestStop();
                e.Handled = true;
                break;
            case Key.L | Key.CtrlMask:
                _displayView.Clear();
                e.Handled = true;
                break;
        }
    }

    private void OnSendMessage(string input)
    {
        _ = ProcessInputAsync(input);
    }

    private async Task ProcessInputAsync(string input)
    {
        try
        {
            if (input.StartsWith('/'))
            {
                _displayView.AppendMessage("user", input);
                await ProcessCommandAsync(input);
            }
            else
            {
                _displayView.AppendMessage("user", input);
                await ProcessChatMessageAsync(input);
            }
        }
        catch (Exception ex)
        {
            _displayView.AppendMessage("error", $"错误: {ex.Message}");
        }
    }

    private async Task ProcessCommandAsync(string input)
    {
        try
        {
            var handled = await _consoleAppService.TryExecuteCommandAsync(input);
            if (!handled)
            {
                _displayView.AppendMessage("error", $"未知命令: {input}");
            }
        }
        catch (Exception ex)
        {
            _displayView.AppendMessage("error", $"命令执行失败: {ex.Message}");
        }
    }

    private async Task ProcessChatMessageAsync(string input)
    {
        if (_agent == null)
        {
            _displayView.AppendMessage("error", "Agent 未初始化，请稍候重试");
            return;
        }

        try
        {
            _statusBarManager.UpdateConnection(true);
            
            var currentSession = _sessionManager.CurrentSession;
            if (currentSession == null)
            {
                currentSession = await _sessionManager.CreateSessionAsync(userId: "default", title: "新对话");
                _sessionPanel.Refresh(currentSession);
            }

            await _sessionManager.AddMessageAsync(currentSession.SessionId, "user", input);

            var response = await _agent.RunAsync(input);
            
            var toolCalls = new List<string>();
            var thinkingContents = new List<string>();

            if (response.Messages != null)
            {
                foreach (var message in response.Messages)
                {
                    if (message.Role == ChatRole.Assistant && message.Contents != null)
                    {
                        foreach (var content in message.Contents)
                        {
                            if (content is FunctionCallContent functionCall)
                            {
                                var toolInfo = $"调用工具: {functionCall.Name}";
                                toolCalls.Add(toolInfo);
                            }
                        }

                        var textContents = message.Contents
                            .OfType<TextContent>()
                            .Where(t => !string.IsNullOrWhiteSpace(t.Text))
                            .ToList();

                        foreach (var text in textContents)
                        {
                            var isThinking = false;
                            if (text.AdditionalProperties != null)
                            {
                                foreach (var key in text.AdditionalProperties.Keys)
                                {
                                    if (key.Contains("thinking", StringComparison.OrdinalIgnoreCase) ||
                                        key.Contains("thought", StringComparison.OrdinalIgnoreCase) ||
                                        key.Contains("reasoning", StringComparison.OrdinalIgnoreCase))
                                    {
                                        isThinking = true;
                                        break;
                                    }
                                }
                            }

                            if (isThinking)
                            {
                                thinkingContents.Add(text.Text!);
                            }
                        }
                    }
                }
            }

            if (toolCalls.Count > 0)
            {
                _displayView.AppendMessage("tool", string.Join("\n", toolCalls));
            }

            if (thinkingContents.Count > 0)
            {
                _displayView.AppendMessage("thinking", string.Join("\n", thinkingContents));
            }

            if (!string.IsNullOrEmpty(response.Text))
            {
                _displayView.AppendMessage("assistant", response.Text);
                await _sessionManager.AddMessageAsync(currentSession.SessionId, "assistant", response.Text);
            }

            _sessionPanel.Refresh(_sessionManager.CurrentSession);
        }
        catch (Exception ex)
        {
            _statusBarManager.UpdateConnection(false);
            _displayView.AppendMessage("error", $"错误: {ex.Message}");
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
