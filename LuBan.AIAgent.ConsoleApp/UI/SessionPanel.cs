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
        Width = Dim.Percent(20);
        
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
