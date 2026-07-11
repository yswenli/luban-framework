/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common
*文件名： ArgsUtil
*版本号： V1.0.0.0
*唯一标识：a071b2a8-4335-49a1-8281-16f06846b6c2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/7/7 11:09:17
*描述：初始化参数处理类
*
*=================================================
*修改标记
*修改时间：2025/7/7 11:09:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：初始化参数处理类
*
*****************************************************************************/

namespace LuBan.Common
{
    /// <summary>
    /// 初始化参数处理类
    /// </summary>
    public static class ArgsUtil
    {
        /// <summary>
        /// 读取初始化参数
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T? Read<T>(string[] args, string argName)
        {
            return new CommonLineArgsConfig(args).Read<T>(argName);
        }
    }

    /// <summary>
    /// 命令行参数配置类
    /// </summary>
    /// <remarks>
    /// 命令行参数配置类
    /// </remarks>
    /// <param name="args"></param>
    public class CommonLineArgsConfig(string[] args)
    {
        IConfigurationRoot _root = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

        /// <summary>
        /// 读取配置项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argName"></param>
        /// <returns></returns>
        public T? Read<T>(string argName)
        {
            return _root.GetValue<T>(argName);
        }
    }
}
