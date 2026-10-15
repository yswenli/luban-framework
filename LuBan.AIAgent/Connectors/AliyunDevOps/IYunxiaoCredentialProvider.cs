/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Connectors.AliyunDevOps
*文件名： IYunxiaoCredentialProvider
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：云效凭据提供者接口
*
*****************************************************************************/

namespace LuBan.AIAgent.Connectors.AliyunDevOps;

/// <summary>
/// 云效凭据提供者接口 - 用于获取个人访问令牌（PAT）
/// </summary>
public interface IYunxiaoCredentialProvider
{
    /// <summary>
    /// 获取云效个人访问令牌（PAT）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>PAT 字符串，未配置时返回 null</returns>
    Task<string?> GetPatAsync(CancellationToken cancellationToken = default);
}