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
        
        ScrollTo(Lines - 1, true);
    }

    public new void Clear()
    {
        Text = "";
        SetNeedsDisplay();
    }
    
    private void MoveToEnd()
    {
        Move(0, Lines);
    }
}
