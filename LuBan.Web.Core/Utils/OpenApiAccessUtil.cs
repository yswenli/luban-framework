/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Utils
*文件名： OpenApiAccessUtil
*版本号： V1.0.0.0
*唯一标识：9532b94f-b1eb-4853-8798-dc06b76ecd1c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/10 10:21:51
*描述：开放接口工具类
*
*=================================================
*修改标记
*修改时间：2025/4/10 10:21:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：开放接口工具类
*
*****************************************************************************/
namespace LuBan.Web.Core.Utils;

/// <summary>
/// 开放接口工具类
/// </summary>
public static class OpenApiAccessUtil
{
    /// <summary>
    /// 生成refreshToken
    /// </summary>
    /// <param name="accessKey"></param>
    /// <param name="accessSecrect"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<OpenAccessToken> GetRefreshTokenAsync([NotNull] string accessKey, [NotNull] string accessSecrect, int timeout = 7200)
    {
        var resp = new DbRepository<DbOpenAccess>();
        var entity = await resp.FirstAsync(q => q.IsDelete == false && q.AccessKey == accessKey && q.AccessSecret == accessSecrect);
        if (entity == null || entity.Id < 1) throw FriendlyError.Ex("用户凭证不正确");
        if (entity.BindUserId < 1 || !entity.IsEnabled) throw FriendlyError.Ex("当前凭证暂未启用，请联系管理员");
        if (entity.RefreshToken.IsNullOrEmpty()) throw FriendlyError.Ex("当前凭证未初始，请联系管理员");
        var json = AESUtil.Decrypt(entity.RefreshToken, CommonConst.SecretSalt);
        if (json.IsNullOrEmpty()) throw FriendlyError.Ex("refreshtoken格式有误");
        var data = SerializeUtil.Deserialize<OpenAccessUserIdExpired>(json);
        if (data == null) throw FriendlyError.Ex("refreshtoken格式有误");
        if (data.Expired < DateTime.Now) throw FriendlyError.Ex("refreshtoken已过期，请联系管理员");
        return new OpenAccessToken { RefreshToken = entity.RefreshToken, AccessToken = JwtExtention.CreateJwtToken(data.UserId, timeout) };
    }


    /// <summary>
    /// 通过refreshtoken获取accesstoken，
    /// 有效期2小时
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public static async Task<AccessToken> GetAccessTokenAsync([NotNull] string refreshToken, int timeout = 7200)
    {
        var resp = new DbRepository<DbOpenAccess>();
        var entity = await resp.FirstAsync(q => q.IsDelete == false && q.RefreshToken == refreshToken);
        if (entity == null || entity.Id < 1) throw FriendlyError.Ex("refreshtoken不正确");
        if (entity.BindUserId < 1 || !entity.IsEnabled) throw FriendlyError.Ex("当前凭证暂未启用，请联系管理员");
        var json = AESUtil.Decrypt(refreshToken, CommonConst.SecretSalt);
        if (json.IsNullOrEmpty()) throw FriendlyError.Ex("refreshtoken格式有误");
        var data = SerializeUtil.Deserialize<OpenAccessUserIdExpired>(json);
        if (data == null) throw FriendlyError.Ex("refreshtoken格式有误");
        if (data.Expired < DateTime.Now) throw FriendlyError.Ex("refreshtoken已过期，请联系管理员");
        var expireTime = DateTime.Now.AddSeconds(timeout);
        var token = JwtExtention.CreateJwtToken(data.UserId, timeout);
        return new AccessToken()
        {
            Token = token,
            ExpireTime = expireTime
        };
    }

    /// <summary>
    /// 生成refreshToken
    /// </summary>
    /// <param name="bindUserId"></param>
    /// <param name="expired"></param>
    /// <returns></returns>
    public static string GenerateRefreshToken(long bindUserId, TimeSpan expired)
    {
        var resp = new DbRepository<DbOpenAccess>();
        var user = resp.Change<DbUser>().First(q => q.IsDelete == false && q.Id == bindUserId);
        if (user == null || user.Id < 1) throw FriendlyError.Ex("不存在此用户");
        var data = new OpenAccessUserIdExpired()
        {
            UserId = bindUserId,
            Expired = DateTime.Now.AddMilliseconds(expired.TotalMilliseconds)
        };
        var json = data.ToJson();
        json = json.Insert(1, $"\"n\":\"{Guid.NewGuid():N}\",");
        return AESUtil.Encrypt(json, CommonConst.SecretSalt);
    }


    /// <summary>
    /// 参数生成工具,
    /// 根据输入的内容生成对应的参数
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static AraInfo AuthParmasTool(dynamic input)
    {
        string str = input?.ToString() ?? "";
        AraInfo result;
        if (str.IsNullOrEmpty())
        {
            return new AraInfo([]);
        }

        if (str.IndexOf("=") > -1)
        {
            result = new AraInfo(str.ToQueryDic());
        }
        else
        {
            return new AraInfo(str);
        }
        return result;
    }

}
