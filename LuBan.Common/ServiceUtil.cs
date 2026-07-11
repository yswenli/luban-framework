/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： ServiceUtil
*版本号： V1.0.0.0
*唯一标识：7857de76-845c-4229-b67d-9e81cb69117a
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/3/22 11:37:19
*描述：windows服务工具类
*
*=====================================================================
*修改标记
*修改时间：2022/3/22 11:37:19
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：windows服务工具类
*
*****************************************************************************/
using System.Runtime.Versioning;
using System.ServiceProcess;

namespace LuBan.Common;

/// <summary>
/// windows服务工具类,
/// 仅针对windows，需要用管理员，并在服务上开启与桌面交互选项
/// </summary>
[SupportedOSPlatform("windows")]
public static class ServiceUtil
{
    /// <summary>
    /// 安装服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="fileName"></param>
    /// <param name="description"></param>
    /// <param name="autoStart"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [SupportedOSPlatform("windows")]
    public static bool Install(string serviceName, string fileName, string description, bool autoStart = true)
    {
        if (string.IsNullOrEmpty(serviceName)) throw new Exception("服务名称不能为空");

        if (string.IsNullOrEmpty(fileName) || !FileUtil.Exists(fileName)) throw new Exception("文件地址不能为空或文件不存在,fileName:" + fileName);

        try
        {
            if (ProcessUtil.Execute($"sc create \"{serviceName}\" binPath= \"{fileName}\" {(autoStart ? "start= auto" : "")} displayname=\"{serviceName}\"", out string _))
            {
                if (!string.IsNullOrEmpty(description))
                {
                    ProcessUtil.Execute($"sc description \"{serviceName}\" \"{description}\"", out string _);
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("ServiceUtil.Install", ex, serviceName, fileName, description, autoStart);
        }
        return false;
    }

    /// <summary>
    /// 卸载服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [SupportedOSPlatform("windows")]
    public static bool Uninstall(string serviceName)
    {
        if (string.IsNullOrEmpty(serviceName)) throw new Exception("服务名称不能为空");

        try
        {
            return ProcessUtil.Execute($"sc delete \"{serviceName}\"", out string _);
        }
        catch (Exception ex)
        {
            Logger.Error("ServiceUtil.Uninstall", ex, serviceName);
        }
        return false;
    }

    /// <summary>
    /// 启动服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [SupportedOSPlatform("windows")]
    public static bool Start(string serviceName, int timeOut = 60)
    {
        if (string.IsNullOrEmpty(serviceName)) throw new Exception("服务名称不能为空");

        try
        {
            var sc = new ServiceController(serviceName);
            if (sc != null)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(timeOut));
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("ServiceUtil.Start", ex, serviceName);
        }
        return false;
    }

    /// <summary>
    /// 关闭服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [SupportedOSPlatform("windows")]
    public static bool Stop(string serviceName, int timeOut = 60)
    {
        if (string.IsNullOrEmpty(serviceName)) throw new Exception("服务名称不能为空");

        try
        {
            var sc = new ServiceController(serviceName);
            if (sc != null)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(timeOut));
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("ServiceUtil.Stop", ex, serviceName);
        }
        return false;
    }


    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [SupportedOSPlatform("windows")]
    public static bool Exists(string serviceName)
    {
        try
        {
            var sc = new ServiceController(serviceName);
            if (sc != null)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.ErrorWithOutEvent("ServiceUtil.Exists", ex, serviceName);
        }
        return false;
    }

    /// <summary>
    /// 是否在运行
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [SupportedOSPlatform("windows")]
    public static bool IsRunning(string serviceName)
    {
        try
        {
            var sc = new ServiceController(serviceName);
            if (sc != null)
            {
                return sc.Status == ServiceControllerStatus.Running;
            }
        }
        catch (Exception ex)
        {
            Logger.ErrorWithOutEvent("ServiceUtil.IsRunning", ex, serviceName);
        }
        return false;
    }
}
