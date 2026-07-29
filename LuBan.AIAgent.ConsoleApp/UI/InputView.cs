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

    private void InputView_KeyDown(View.KeyEventEventArgs e)
    {
        if (e.KeyEvent.Key == Key.Enter)
        {
            if (e.KeyEvent.IsShift)
            {
                InsertText("\n");
                e.Handled = true;
            }
            else
            {
                var input = Text?.ToString()?.Trim();
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
