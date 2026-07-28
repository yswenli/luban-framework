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
