/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common
*文件名： Tracer
*版本号： V1.0.0.0
*唯一标识：50a1be9f-32e0-4ac3-ad9f-e36d6fcf8c3a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/5/13 9:36:07
*描述：跟踪
*
*=================================================
*修改标记
*修改时间：2025/5/13 9:36:07
*修改人： yswenli
*版本号： V1.0.0.0
*描述：跟踪
*
*****************************************************************************/
namespace System;

/// <summary>
/// 跟踪
/// </summary>
public class Tracer : IDisposable
{
    Stopwatch _stopwatch;

    /// <summary>
    /// 跟踪
    /// </summary>
    public Tracer()
    {
        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// 输出消息
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    public void WriteLine(string msg, params object[] args)
    {
        _stopwatch.Stop();
        var cost = _stopwatch.ElapsedMilliseconds;
        var sp = new StringPlus("-".Repeat(30));
        sp.AppendLine($"时间：{DateTime.Now}");
        sp.AppendLine($"消息：{msg}");
        sp.AppendLine($"用时：{cost}");
        if (args != null && args.Length > 0)
        {
            var json = SerializeUtil.Serialize(args);
            if (json.IsNotNullOrEmpty())
            {
                sp.AppendLine($"参数：{json}");
            }
        }
        sp.AppendLine("-".Repeat(30));
        ConsoleUtil.WriteLine(sp.ToString(), false);
        _stopwatch.Restart();
    }
    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        _stopwatch.Stop();
    }
}

/// <summary>
/// 追踪扩展类
/// </summary>
public static class TracerExtention
{
    /// <summary>
    /// 启动追踪
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static Tracer StartTracing(this string msg)
    {
        var tracer = new Tracer();
        tracer.WriteLine(msg);
        return tracer;
    }
}
