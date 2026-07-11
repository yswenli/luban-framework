namespace LuBan.EventBus.Core;

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
