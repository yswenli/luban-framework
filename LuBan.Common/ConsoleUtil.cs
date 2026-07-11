/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： ConsoleUtil
*版本号： V1.0.0.0
*唯一标识：ea3c8e88-f7e7-4071-beb5-6129e434daf6
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/3/2 17:35:55
*描述：控制台工具类
*
*=====================================================================
*修改标记
*修改时间：2022/3/2 17:35:55
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：控制台工具类
*
*****************************************************************************/

namespace LuBan.Common;

/// <summary>
/// 控制台工具类,
/// https://spectreconsole.net/sponsors
/// </summary>
public static class ConsoleUtil
{
    static IEventBus? _eventBus;

    /// <summary>
    /// 设置事件总线
    /// </summary>
    /// <param name="eventBus"></param>
    public static void SetEventBus(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <summary>
    /// 控制台工具类
    /// </summary>
    static ConsoleUtil()
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
        }
        catch { }
    }

    /// <summary>
    /// 是否启用
    /// </summary>
    public static bool IsEnabled
    {
        get; set;
    } = true;


    /// <summary>
    /// 设置控制台标题
    /// </summary>
    /// <param name="title"></param>
    public static void SetTitle(string title)
    {
        try
        {
            Console.Title = title;
        }
        catch { }
    }

    /// <summary>
    /// 写入
    /// </summary>
    /// <param name="msg"></param>
    public static void Write(this string msg, ConsoleColor color = ConsoleColor.Gray)
    {
        try
        {
            AnsiConsole.Markup($"[{color}]{msg}[/]");
        }
        catch
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ForegroundColor = oldColor;
        }
    }

    /// <summary>
    /// 控制台输出内容
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="withTime"></param>
    /// <param name="timeFormat"></param>
    /// <param name="color"></param>
    public static void WriteLine(this string msg,
        bool withTime = true,
        string timeFormat = "HH:mm:ss.fff",
        ConsoleColor color = ConsoleColor.Gray)
    {
        var info = new ConsoleWriteInfo()
        {
            Color = color,
            TimeFormat = timeFormat,
            Txt = msg,
            WithTime = withTime
        };
        if (_eventBus == null)
        {
            _WriteDirect(info);
            return;
        }
        _ = _eventBus.PublishAsync(info);
    }

    private static void _WriteDirect(ConsoleWriteInfo info)
    {
        try
        {
            if (RuntimeUtil.IsWindows())
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = info.Color;
                if (info.WithTime)
                    Console.WriteLine($"{DateTimeUtil.Now.ToString(info.TimeFormat)}\t{info.Txt}");
                else
                    Console.WriteLine(info.Txt);
                Console.ForegroundColor = oldColor;
            }
            else
            {
                var colorText = _ToAnsiColor(info.Color);
                if (info.WithTime)
                    Console.WriteLine($"{colorText}{DateTimeUtil.Now.ToString(info.TimeFormat)}\t{info.Txt}\x1B[0m");
                else
                    Console.WriteLine($"{colorText}{info.Txt}\x1B[0m");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    public static string _ToAnsiColor(ConsoleColor color) => color switch
    {
        ConsoleColor.Black => "\x1B[30m",
        ConsoleColor.DarkRed => "\x1B[31m",
        ConsoleColor.DarkGreen => "\x1B[32m",
        ConsoleColor.DarkYellow => "\x1B[33m",
        ConsoleColor.DarkBlue => "\x1B[34m",
        ConsoleColor.DarkMagenta => "\x1B[35m",
        ConsoleColor.DarkCyan => "\x1B[36m",
        ConsoleColor.Gray => "\x1B[37m",
        ConsoleColor.DarkGray => "\x1B[90m",
        ConsoleColor.Red => "\x1B[91m",
        ConsoleColor.Green => "\x1B[92m",
        ConsoleColor.Yellow => "\x1B[93m",
        ConsoleColor.Blue => "\x1B[94m",
        ConsoleColor.Magenta => "\x1B[95m",
        ConsoleColor.Cyan => "\x1B[96m",
        ConsoleColor.White => "\x1B[97m",
        _ => "\x1B[39m",
    };

    /// <summary>
    /// 控制台输出换行
    /// </summary>
    public static void WriteLine()
    {
        WriteLine(string.Empty);
    }

    static int _lineCount = 0;
    /// <summary>
    /// 控制台输出内容
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="color"></param>
    public static void WriteLineWithCount(this string msg, ConsoleColor color = ConsoleColor.Gray)
    {
        Interlocked.Increment(ref _lineCount);
        WriteLine($"{_lineCount}.{msg}", color: color);
    }


    /// <summary>
    /// 控制台输出内容
    /// </summary>
    /// <param name="args"></param>
    public static void WriteArgs(this string[] args)
    {
        if (args == null || args.Length < 1) return;
        var sb = new StringPlus();
        sb.Append("args:");
        foreach (var item in args)
        {
            sb.Append(item + " ");
        }
        WriteLine(sb.ToString());
    }
    /// <summary>
    /// 控制台输出内容
    /// </summary>
    /// <param name="size"></param>
    public static void WriteNewLine(int size = 1)
    {
        try
        {
            if (size < 1) size = 1;
            for (int i = 0; i < size; i++)
            {
                Console.WriteLine();
            }
        }
        catch { }
    }

    /// <summary>
    /// 读取字符串
    /// </summary>
    /// <returns></returns>
    public static string ReadString()
    {
        try
        {
            return Console.ReadLine() ?? "";
        }
        catch { }
        return string.Empty;
    }

    /// <summary>
    /// 读取多段内容
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    public static string ReadText(int lines)
    {
        if (lines < 1) return string.Empty;
        var sb = new StringPlus();
        for (int i = 0; i < lines; i++)
        {
            var str = ReadString();
            if (string.IsNullOrEmpty(str))
            {
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine(str);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 读取多个字分隔的字符串
    /// </summary>
    /// <param name="splitStr"></param>
    /// <returns></returns>
    public static IEnumerable<string>? ReadStrings(string splitStr = " ")
    {
        try
        {
            var txt = Console.ReadLine();
            if (string.IsNullOrEmpty(txt)) return default;
            return txt.Split(splitStr, StringSplitOptions.TrimEntries);
        }
        catch
        {

        }
        return null;
    }

    /// <summary>
    /// 读取数字
    /// </summary>
    /// <param name="splitStr"></param>
    /// <returns></returns>
    public static IEnumerable<int> ReadInts(string splitStr = " ")
    {
        var arr = ReadStrings(splitStr);
        if (arr != null)
        {
            foreach (var item in arr)
            {
                if (int.TryParse(item, out int r))
                {
                    yield return r;
                }
            }
        }
        yield break;
    }

    /// <summary>
    /// 读取参数内容
    /// </summary>
    /// <param name="args"></param>
    /// <param name="name"></param>
    /// <param name="splitStrs"></param>
    /// <returns></returns>
    public static string? ReadValue(this string[] args, string name, string[]? splitStrs = null)
    {
        if (args == null || args.Length < 1) return null;

        if (splitStrs == null || splitStrs.Length < 1)
        {
            splitStrs = [":", "="];
        }

        foreach (var arg in args)
        {
            if (!string.IsNullOrEmpty(arg) && arg.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                var arr = arg.Split(splitStrs, StringSplitOptions.TrimEntries);
                if (arr != null && arr.Length > 1)
                {
                    return arr[1];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 读取参数内容
    /// </summary>
    /// <param name="args"></param>
    /// <param name="splitStrs"></param>
    /// <returns></returns>
    public static Dictionary<string, string>? ReadValues(this string[] args, string[]? splitStrs = null)
    {
        if (args == null || args.Length < 1) return null;

        var dic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        if (splitStrs == null || splitStrs.Length < 1)
        {
            splitStrs = [":", "="];
        }

        foreach (var arg in args)
        {
            if (!string.IsNullOrEmpty(arg))
            {
                var arr = arg.Split(splitStrs, StringSplitOptions.TrimEntries);

                if (arr != null)
                {
                    if (arr.Length == 2)
                    {
                        dic.TryAdd(arr[0], arr[1]);
                    }
                    else if (arr.Length == 1)
                    {
                        dic.TryAdd(arr[0], string.Empty);
                    }
                }
            }
        }
        return dic;
    }

    /// <summary>
    /// 输出名称
    /// </summary>
    public static void PrintName()
    {
        try
        {
            var base64Str = "ICBfICAgICAgICAgIF9fX18gICAgICAgICAgICAgICAgX19fX18gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIF8gICAgCiB8IHwgICBfICAgX3wgX18gKSAgX18gXyBfIF9fICAgfCAgX19ffCBfXyBfXyBfIF8gX18gX19fICAgX19fX18gICAgICBfX19fXyAgXyBfX3wgfCBfXwogfCB8ICB8IHwgfCB8ICBfIFwgLyBfYCB8ICdfIFwgIHwgfF8gfCAnX18vIF9gIHwgJ18gYCBfIFwgLyBfIFwgXCAvXCAvIC8gXyBcfCAnX198IHwvIC8KIHwgfF9ffCB8X3wgfCB8XykgfCAoX3wgfCB8IHwgfCB8ICBffHwgfCB8IChffCB8IHwgfCB8IHwgfCAgX18vXCBWICBWIC8gKF8pIHwgfCAgfCAgIDwgCiB8X19fX19cX18sX3xfX19fLyBcX18sX3xffCB8X3wgfF98ICB8X3wgIFxfXyxffF98IHxffCB8X3xcX19ffCBcXy9cXy8gXF9fXy98X3wgIHxffFxfXAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA=";
            base64Str.ToStr().Write(color: ConsoleColor.Green);
            Environment.NewLine.Write();
            Environment.NewLine.Write();
            var year = $"{"MjAyMi0=".ToStr()}{DateTime.Now.Year}";
            var author = "IEAgeXN3ZW5saSBhbGwgcmlnaHRzIHJlc2VydmVk".ToStr();
            $"{year}{author}".PadLeft(87).Write(ConsoleColor.DarkGray);
            Environment.NewLine.Write();
            Environment.NewLine.Write();
            Environment.NewLine.Write();
        }
        catch (Exception ex)
        {
            WriteLine($"ConsoleUtil.PrintName 异常，ex:{ex}");
        }
    }

    /// <summary>
    /// 输出进度
    /// </summary>
    /// <param name="action"></param>
    /// <param name="total"></param>
    /// <param name="step"></param>
    /// <param name="info"></param>
    public static void PrintProcess(Action action, int total, int step = 1, string info = "[green]已完成[/]")
    {
        using var locker = LockerBuilder.Default.Create("ConsoleUtil.PrintProcess");
        try
        {
            AnsiConsole.Progress()
                        .Columns(
                        [
                            new TaskDescriptionColumn(),    // Task description
                            new SpinnerColumn(Spinner.Known.Arrow3),            // Spinner
                            new ProgressBarColumn(),        // Progress bar
                            new PercentageColumn()         // Percentage         
                        ])
                        .Start((ctx) =>
                        {
                            var task = ctx.AddTask(info, new ProgressTaskSettings() { AutoStart = true, MaxValue = total });
                            while (!ctx.IsFinished)
                            {
                                try
                                {
                                    action?.Invoke();
                                }
                                catch { }
                                task.Increment(step);
                            }
                        });
        }
        catch (Exception ex)
        {
            WriteLine($"ConsoleUtil.PrintProcess 异常，ex:{ex}");
        }

    }
    /// <summary>
    /// 输出进度
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="action"></param>
    /// <param name="info"></param>
    public static void PrintProcess<T>(List<T> data, Action<T> action, string info = "[yellow]已完成[/]")
    {
        using var locker = LockerBuilder.Default.Create("ConsoleUtil.PrintProcess");
        try
        {
            AnsiConsole.Progress()
                .Columns(
                [
                    new TaskDescriptionColumn(),
                    new SpinnerColumn(Spinner.Known.Arrow3),
                    new ProgressBarColumn(),
                    new PercentageColumn()
                ])
                .Start((ctx) =>
                {
                    var task = ctx.AddTask(info, new ProgressTaskSettings() { AutoStart = true, MaxValue = data.Count });
                    foreach (var item in data)
                    {
                        try
                        {
                            action.Invoke(item);
                        }
                        catch (Exception ex)
                        {
                            WriteLine($"ConsoleUtil.PrintProcess 异常，ex:{ex}");
                        }
                        task.Increment(1);
                    }
                    ctx.Refresh();
                });
        }
        catch (Exception ex)
        {
            WriteLine($"ConsoleUtil.PrintProcess 异常，ex:{ex}");
        }

    }

    /// <summary>
    /// 输出进度
    /// </summary>
    /// <param name="action"></param>
    /// <param name="total"></param>
    /// <param name="step"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public static async Task PrintProcessAsync(Func<Task> action, int total, int step = 1, string info = "[yellow]已完成[/]")
    {
        using var locker = await LockerBuilder.Default.CreateAsync("ConsoleUtil.PrintProcess");
        try
        {
            await AnsiConsole.Progress()
                .Columns(
                [
                    new TaskDescriptionColumn(),    // Task description
                    new SpinnerColumn(Spinner.Known.Arrow3),            // Spinner
                    new ProgressBarColumn(),        // Progress bar
                    new PercentageColumn()         // Percentage         
                ])
                .Start(async (ctx) =>
                {
                    var task = ctx.AddTask(info, new ProgressTaskSettings() { AutoStart = true, MaxValue = total });
                    while (!ctx.IsFinished)
                    {
                        try
                        {
                            await action.Invoke();
                        }
                        catch { }
                        task.Increment(step);
                    }
                });
        }
        catch (Exception ex)
        {
            WriteLine($"ConsoleUtil.PrintProcess 异常，ex:{ex}");
        }

    }


    /// <summary>
    /// 输出进度
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="action"></param>
    /// <param name="info"></param>
    public static async Task PrintProcessAsync<T>(List<T> data, Func<T, Task> action, string info = "[yellow]已完成[/]")
    {
        using var locker = await LockerBuilder.Default.CreateAsync("ConsoleUtil.PrintProcess");

        try
        {
            await AnsiConsole.Progress()
                .Columns(
                [
                    new TaskDescriptionColumn(),    // Task description
                    new SpinnerColumn(Spinner.Known.Arrow3),            // Spinner
                    new ProgressBarColumn(),        // Progress bar
                    new PercentageColumn()         // Percentage         
                ])
                .Start(async (ctx) =>
                {
                    var task = ctx.AddTask(info, new ProgressTaskSettings() { AutoStart = true, MaxValue = data.Count });
                    foreach (var item in data)
                    {
                        try
                        {
                            await action.Invoke(item);
                        }
                        catch { }
                        task.Increment(1);
                    }
                });
        }
        catch (Exception ex)
        {
            WriteLine($"ConsoleUtil.PrintProcess 异常，ex:{ex}");
        }
    }

    /// <summary>
    /// 输出进度
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <param name="msg"></param>
    /// <param name="color"></param>
    public static T? PrintProcess<T>(Func<T> action, string msg = "正在处理中", string color = "yellow")
    {
        using var locker = LockerBuilder.Default.Create("ConsoleUtil.PrintProcess");
        try
        {
            return AnsiConsole.Status()
                    .Spinner(Spinner.Known.DotsCircle)
                    .SpinnerStyle(Style.Parse(color))
                    .Start(msg, ctx =>
                    {
                        return action.Invoke();
                    });
        }
        catch (Exception ex)
        {
            WriteLine($"ConsoleUtil.PrintProcess 异常，ex:{ex}");
        }

        return default;
    }



    /// <summary>
    /// 绘制表格
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="model"></param>
    /// <param name="title"></param>
    public static void WriteTable<TModel>(TModel model, string title = "") where TModel : class, new()
    {
        try
        {
            var table = MemoryCache.Instance.GetOrSet<ConsoleTable<TModel>>($"{CacheConst.KeyConst}console_table:{typeof(TModel)}", (k) => new ConsoleTable<TModel>(title));
            table?.Write(model);
        }
        catch { }
    }
    /// <summary>
    /// 绘制表格
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="models"></param>
    /// <param name="title"></param>
    /// <param name="isClear"></param>
    public static void WriteTable<TModel>(List<TModel> models, string title = "", bool isClear = true) where TModel : class, new()
    {
        var table = new ConsoleTable<TModel>(title);
        foreach (var model in models)
        {
            table.Write(model, isClear);
        }
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public static void Close()
    {
        // EventBus 的生命周期由依赖注入管理
    }

}

/// <summary>
/// 控制台输出信息
/// </summary>
public class ConsoleWriteInfo : IEventData
{
    /// <summary>
    /// 事件名称
    /// </summary>
    public string Name { get; set; } = "ConsoleWrite";
    /// <summary>
    /// 事件发生时间
    /// </summary>
    public DateTime EventTime { get; set; } = DateTime.Now;
    /// <summary>
    /// 事件源
    /// </summary>
    public object? EventSource { get; set; }
    /// <summary>
    /// txt
    /// </summary>
    public string Txt { get; set; }
    /// <summary>
    /// 是否带时间
    /// </summary>
    public bool WithTime { get; set; } = true;
    /// <summary>
    /// 时间格式
    /// </summary>
    public string TimeFormat { get; set; } = "HH:mm:ss.fff";
    /// <summary>
    /// 颜色
    /// </summary>
    public ConsoleColor Color { get; set; } = ConsoleColor.Gray;
}

/// <summary>
/// 控制台表格
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class ConsoleTable<TModel> where TModel : class, new()
{
    bool _isFirst = true;
    int _fixedCount = 4;
    Spectre.Console.Table _table;
    PropertyInfo[] _properties;
    /// <summary>
    /// 控制台表格
    /// </summary>
    /// <param name="title"></param>
    public ConsoleTable(string title = "")
    {
        _table = new Spectre.Console.Table();
        _table.Border(TableBorder.Rounded)
            .BorderStyle(Style.Plain.Foreground);
        if (title.IsNotNullOrEmpty())
        {
            _table.Title($"[bold cyan]{title}[/]");
            _fixedCount += 1;
        }
        _properties = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        if (_properties.Length > 0)
        {
            foreach (var item in _properties)
            {
                _table.AddColumn($"[bold cyan]{item.Name}[/]", (c) => c.Alignment = Justify.Left);
            }
        }
    }
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <param name="isClear"></param>
    public void Write(TModel model, bool isClear = true)
    {
        if (isClear)
            _table.Rows.Clear();
        List<string> values = [];
        foreach (var item in _properties)
        {
            values.Add(item.GetValue(model)?.ToString() ?? "");
        }
        _table.AddRow(values.ToArray());
        if (_isFirst)
        {
            _isFirst = false;
        }
        else
        {
            AnsiConsole.Cursor.MoveUp(_table.Rows.Count + _fixedCount);
        }
        AnsiConsole.Write(_table);
    }

}