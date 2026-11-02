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
*公司名称：河之洲
*命名空间：LuBan.Wechat
*文件名： IWechatCorpClient
*版本号： V1.0.0.0
*唯一标识：77b36566-fddc-474d-b19b-a2207ea7d2bc
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/13 10:58:40
*描述：企业微信客户端
*
*=================================================
*修改标记
*修改时间：2024/11/13 10:58:40
*修改人： yswenli
*版本号： V1.0.0.0
*描述：企业微信客户端
*
*****************************************************************************/

namespace LuBan.Wechat;

/// <summary>
/// 企业微信客户端
/// </summary>
public interface IWechatCorpClient
{
    /// <summary>
    /// 企业微信客户端
    /// </summary>
    WechatWorkClient Client { get; }
    /// <summary>
    /// 获取AccessToken
    /// </summary>
    /// <param name="suiteAccessToken"></param>
    /// <returns></returns>
    Task<AccessToken?> GetAccessToken(string? suiteAccessToken = null);
}
