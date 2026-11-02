/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.EventBus.Core
*文件名： ConsoleWriteInfoHandler.cs
*版本号： V1.0.0.0
*唯一标识：db12a430-0d6f-4493-9ff2-4337f592053d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ConsoleWriteInfoHandler 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ConsoleWriteInfoHandler 类
*
*****************************************************************************/

namespace LuBan.EventBus.Core;

/// <summary>
/// ConsoleWriteInfoHandler 处理器类
/// </summary>

/// <summary>
/// 把控制台输出事件按事件指定的颜色与时间格式打印到控制台的处理器。
/// </summary>
public class ConsoleWriteInfoHandler : IEventHandler<ConsoleWriteInfo>
{
    /// <summary>
    /// 按事件中的颜色与时间开关输出文本：Windows 使用控制台前景色，其它平台使用 ANSI 颜色。
    /// </summary>
    /// <param name="eventData">包含待输出文本、颜色与时间格式的控制台输出事件。</param>
    public Task HandleAsync(ConsoleWriteInfo eventData)
    {
        try
        {
            if (RuntimeUtil.IsWindows())
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = eventData.Color;
                if (eventData.WithTime)
                    Console.WriteLine($"{DateTimeUtil.Now.ToString(eventData.TimeFormat)}\t{eventData.Txt}");
                else
                    Console.WriteLine(eventData.Txt);
                Console.ForegroundColor = oldColor;
            }
            else
            {
                var colorText = ConsoleUtil._ToAnsiColor(eventData.Color);
                if (eventData.WithTime)
                    Console.WriteLine($"{colorText}{DateTimeUtil.Now.ToString(eventData.TimeFormat)}\t{eventData.Txt}\x1B[0m");
                else
                    Console.WriteLine($"{colorText}{eventData.Txt}\x1B[0m");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 记录事件处理过程中抛出的异常，避免异常向事件总线传播。
    /// </summary>
    /// <param name="exception">事件处理期间捕获的异常。</param>
    public Task OnErrorAsync(Exception exception)
    {
        Logger.Error(exception);
        return Task.CompletedTask;
    }
}
