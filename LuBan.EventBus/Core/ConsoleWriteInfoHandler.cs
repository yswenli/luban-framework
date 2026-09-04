/****************************************************************************
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

public class ConsoleWriteInfoHandler : IEventHandler<ConsoleWriteInfo>
{
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

    public Task OnErrorAsync(Exception exception)
    {
        Logger.Error(exception);
        return Task.CompletedTask;
    }
}
