using Terminal.Gui;
using LuBan.Common;

namespace LuBan.AIAgent.ConsoleApp.UI;

/// <summary>
/// Header 视图，显示 LuBan Framework 名称
/// </summary>
public class HeaderView : View
{
    private readonly string[] _lines;

    public HeaderView()
    {
        Height = 6;
        CanFocus = false;
        _lines = ConsoleUtil.GetLBFName().Split('\n');
    }

    public override void Redraw(Rect bounds)
    {
        base.Redraw(bounds);
        
        for (int i = 0; i < Math.Min(_lines.Length, 6); i++)
        {
            Move(0, i);
            Driver.AddStr(_lines[i]);
        }
    }
}
