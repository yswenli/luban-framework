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

    public override void Redraw(Rect bounds)
    {
        base.Redraw(bounds);
        
        var name = ConsoleUtil.GetLBFName();
        var lines = name.Split('\n');
        
        for (int i = 0; i < Math.Min(lines.Length, 6); i++)
        {
            Move(0, i);
            Driver.AddStr(lines[i]);
        }
    }
}
