using Terminal.Gui;
using LuBan.AIAgent;
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

    private async void OnSendMessage(string input)
    {
        if (input.StartsWith('/'))
        {
            _displayView.AppendMessage("user", input);
        }
        else
        {
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
