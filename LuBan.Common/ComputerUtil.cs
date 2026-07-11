/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common
*文件名： ComputerUtil
*版本号： V1.0.0.0
*唯一标识：dfc1efd8-da78-4f79-be34-b5cc9e899ce6
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 15:51:22
*描述：计算机工具类
*
*=================================================
*修改标记
*修改时间：2023/12/5 15:51:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：计算机工具类
*
*****************************************************************************/
namespace LuBan.Common;

#pragma warning disable CA1416

/// <summary>
/// 计算机工具类
/// </summary>
public static class ComputerUtil
{
    /// <summary>
    /// 内存信息
    /// </summary>
    /// <returns></returns>
    public static MemoryMetrics GetComputerInfo()
    {
        MemoryMetricsClient client = new();
        MemoryMetrics memoryMetrics = IsUnix() ? client.GetUnixMetrics() : client.GetWindowsMetrics();

        memoryMetrics.FreeRam = Math.Round(memoryMetrics.Free / 1024, 2) + "GB";
        memoryMetrics.UsedRam = Math.Round(memoryMetrics.Used / 1024, 2) + "GB";
        memoryMetrics.TotalRam = Math.Round(memoryMetrics.Total / 1024, 2) + "GB";
        memoryMetrics.RamRate = memoryMetrics.Total > 0 
            ? Math.Ceiling(100 * memoryMetrics.Used / memoryMetrics.Total).ToString() + "%" 
            : "0%";
        memoryMetrics.CpuRate = Math.Ceiling(GetCPURate().ParseToDouble()) + "%";
        return memoryMetrics;
    }

    /// <summary>
    /// 磁盘信息
    /// </summary>
    /// <returns></returns>
    public static List<DiskInfo> GetDiskInfos()
    {
        List<DiskInfo> diskInfos = new();

        if (IsUnix())
        {
            try
            {
                var output = ShellUtil.Bash(@"df -mT | awk '/^\/dev\/(sd|vd|xvd|nvme|sda|vda)/ {print $1,$2,$3,$4,$5,$6}'");
                var disks = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (disks.Length == 0) return diskInfos;

                foreach (var item in disks)
                {
                    var disk = item.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (disk == null || disk.Length < 6)
                        continue;

                    var diskInfo = new DiskInfo()
                    {
                        DiskName = disk[0],
                        TypeName = disk[1],
                        TotalSize = long.Parse(disk[2]) / 1024,
                        Used = long.Parse(disk[3]) / 1024,
                        AvailableFreeSpace = long.Parse(disk[4]) / 1024,
                        AvailablePercent = decimal.Parse(disk[5].Replace("%", ""))
                    };
                    diskInfos.Add(diskInfo);
                }
            }
            catch (Exception ex) { Logger.Error(ex); }
        }
        else
        {
            var driv = DriveInfo.GetDrives().Where(u => u.IsReady);
            foreach (var item in driv)
            {
                if (item.DriveType == DriveType.CDRom) continue;
                var obj = new DiskInfo()
                {
                    DiskName = item.Name,
                    TypeName = item.DriveType.ToString(),
                    TotalSize = item.TotalSize / 1024 / 1024 / 1024,
                    AvailableFreeSpace = item.AvailableFreeSpace / 1024 / 1024 / 1024,
                };
                obj.Used = obj.TotalSize - obj.AvailableFreeSpace;
                obj.AvailablePercent = obj.TotalSize > 0 
                    ? decimal.Ceiling(obj.Used / (decimal)obj.TotalSize * 100) 
                    : 0;
                diskInfos.Add(obj);
            }
        }
        return diskInfos;
    }

    /// <summary>
    /// 获取外网IP地址
    /// </summary>
    /// <returns></returns>
    public static string GetIpFromOnline()
    {
        var url = "http://myip.ipip.net";
        var stream = HttpClientProxy.DownloadBytes(url).ToStream();
        var streamReader = new StreamReader(stream);
        var html = streamReader.ReadToEnd();
        return !html.Contains("当前 IP：") ? "未知" : html.Replace("当前 IP：", "").Replace("来自于：", "");
    }

    public static bool IsUnix()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    public static string GetCPURate()
    {
        string cpuRate;
        if (IsUnix())
        {
            string output = ShellUtil.Bash("top -b -n1 | grep \"Cpu(s)\" | awk '{print $2 + $4}'");
            cpuRate = output.Trim();
        }
        else
        {
            string output = ShellUtil.Cmd("wmic", "cpu get LoadPercentage");
            cpuRate = output.Replace("LoadPercentage", string.Empty).Trim();
        }
        return cpuRate;
    }

    /// <summary>
    /// 获取系统运行时间
    /// </summary>
    /// <returns></returns>
    public static string GetRunTime()
    {
        string runTime = string.Empty;
        if (IsUnix())
        {
            string output = ShellUtil.Bash("uptime -s").Trim();
            runTime = DateTimeUtil.FormatTime((DateTime.Now - output.ParseToDateTime()).TotalMilliseconds.ToString().Split('.')[0].ParseToLong());
        }
        else
        {
            string output = ShellUtil.Cmd("wmic", "OS get LastBootUpTime/Value");
            string[] outputArr = output.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (outputArr.Length >= 2)
                runTime = DateTimeUtil.FormatTime((DateTime.Now - outputArr[1].Split('.')[0].ParseToDateTime()).TotalMilliseconds.ToString().Split('.')[0].ParseToLong());
        }
        return runTime;
    }
}

/// <summary>
/// 内存信息
/// </summary>
public class MemoryMetrics
{
    [JsonIgnore]
    public double Total { get; set; }

    [JsonIgnore]
    public double Used { get; set; }

    [JsonIgnore]
    public double Free { get; set; }

    /// <summary>
    /// 已用内存
    /// </summary>
    public string UsedRam { get; set; }

    /// <summary>
    /// CPU使用率%
    /// </summary>
    public string CpuRate { get; set; }

    /// <summary>
    /// 总内存 GB
    /// </summary>
    public string TotalRam { get; set; }

    /// <summary>
    /// 内存使用率 %
    /// </summary>
    public string RamRate { get; set; }

    /// <summary>
    /// 空闲内存
    /// </summary>
    public string FreeRam { get; set; }
}

/// <summary>
/// 磁盘信息
/// </summary>
public class DiskInfo
{
    /// <summary>
    /// 磁盘名
    /// </summary>
    public string DiskName { get; set; }

    /// <summary>
    /// 类型名
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// 总剩余
    /// </summary>
    public long TotalFree { get; set; }

    /// <summary>
    /// 总量
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// 已使用
    /// </summary>
    public long Used { get; set; }

    /// <summary>
    /// 可使用
    /// </summary>
    public long AvailableFreeSpace { get; set; }

    /// <summary>
    /// 使用百分比
    /// </summary>
    public decimal AvailablePercent { get; set; }
}

public class MemoryMetricsClient
{
    /// <summary>
    /// windows系统获取内存信息
    /// </summary>
    /// <returns></returns>
    public MemoryMetrics GetWindowsMetrics()
    {
        var metrics = new MemoryMetrics();
        try
        {
            string output = ShellUtil.Cmd("wmic", "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value");
            var lines = output.Trim().Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            if (lines.Length < 2) return metrics;
            var freeMemoryParts = lines[0].Split('=', StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (freeMemoryParts.Length < 2 || totalMemoryParts.Length < 2) return metrics;
            metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);//m
            metrics.Used = metrics.Total - metrics.Free;

        }
        catch (Exception ex) { Logger.Error(ex); }
        return metrics;
    }

    /// <summary>
    /// Unix系统获取
    /// <returns></returns>
    public MemoryMetrics GetUnixMetrics()
    {
        var metrics = new MemoryMetrics();
        try
        {
            string output = ShellUtil.Bash("free -m | awk '{print $2,$3,$4,$5,$6}'");
            var lines = output.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            if (lines.Length < 2) return metrics;

            {
                var memory = lines[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (memory.Length >= 3)
                {
                    metrics.Total = double.Parse(memory[0]);
                    metrics.Used = double.Parse(memory[1]);
                    metrics.Free = double.Parse(memory[2]);//m
                }
            }
        }
        catch (Exception ex) { Logger.Error(ex); }
        return metrics;
    }
}

#pragma warning restore CA1416
