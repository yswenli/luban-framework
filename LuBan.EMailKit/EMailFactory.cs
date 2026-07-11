/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit
*文件名： EMailFactory
*版本号： V1.0.0.0
*唯一标识：290b2ed0-8aff-4e3c-9a66-3e1f2311af0c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/14 18:22:30
*描述：邮件工厂
*
*=================================================
*修改标记
*修改时间：2024/8/14 18:22:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：邮件工厂
*
*****************************************************************************/


namespace LuBan.EMailKit;

/// <summary>
/// 邮件工厂
/// </summary>
public static class EMailFactory
{
    /// <summary>
    /// 创建邮件客户端
    /// </summary>
    /// <param name="eMailConfig"></param>
    /// <returns></returns>
    public static IEMailClient Create(EMailClientConfig eMailConfig)
    {
        switch (eMailConfig.ClientType)
        {
            case EnumClientType.SMTP:
                return new EMailSmtp(eMailConfig);
            case EnumClientType.IMAP:
                return new EmailImap(eMailConfig);
            case EnumClientType.POP3:
                return new EMailPop3(eMailConfig);
            default:
                throw new NotImplementedException("eMailConfig.ClientType is not implemented");
        }
    }

    /// <summary>
    /// 创建邮件客户端，默认使用配置文件中的第一个
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IEMailClient Create()
    {
        var options = NacosConfigUtil.Read<EMailOptions>();
        if (options == null || options.EMailClientConfigs == null || options.EMailClientConfigs.Count == 0) throw new NotImplementedException("EMailOptions is null");
        return Create(options.EMailClientConfigs[0]);
    }
}
