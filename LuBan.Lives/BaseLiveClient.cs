/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives
*文件名： BaseMeetingClient
*版本号： V1.0.0.0
*唯一标识：6954603c-5133-402e-ba73-52c4ab7d1496
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/18 17:30:27
*描述：直播会议客户端基础类
*
*=================================================
*修改标记
*修改时间：2024/9/18 17:30:27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：直播会议客户端基础类
*
*****************************************************************************/
namespace LuBan.Lives;

/// <summary>
/// 直播会议客户端基础类
/// </summary>
public abstract class BaseLiveClient
{
    protected LiveOption _liveOption;

    protected HttpClientProxy _httpClient;

    /// <summary>
    /// 直播会议客户端基础类
    /// </summary>
    /// <param name="liveConfig"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public BaseLiveClient(LiveOption? liveConfig)
    {
        if (liveConfig == null) throw new ArgumentNullException("LiveConfig配置不能为空");
        _liveOption = liveConfig;
        _httpClient = HttpClientProxy.Create(_liveOption.Url, useLog: true);
    }

    /// <summary>
    /// 获取基础请求头
    /// </summary>
    /// <returns></returns>
    protected abstract Dictionary<string, string> GetBaseHeaders();


    #region 基本请求
    /// <summary>
    /// get请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <returns></returns>
    public async Task<T> Get<T>(string resource)
    {
        return await _httpClient.GetAsync<T>(resource, GetBaseHeaders());
    }

    /// <summary>
    /// post请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<T> Post<T>(string resource, dynamic model)
    {
        var json = SerializeUtil.Serialize(model);
        return await _httpClient.PostJsonAsync<T>(resource, json, GetBaseHeaders());
    }

    /// <summary>
    /// post请求
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<string> Post(string resource, dynamic model)
    {
        var json = SerializeUtil.Serialize(model);
        return await _httpClient.PostJsonAsync(resource, json, GetBaseHeaders());
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<T> Delete<T>(string resource, dynamic model)
    {
        var json = SerializeUtil.Serialize(model);
        return await _httpClient.DeleteJsonAsync<T>(resource, json, GetBaseHeaders());
    }

    #endregion
}
