namespace LuBan.Wechat.Models;
/// <summary>
/// 获取微信二维码请求数据
/// </summary>
public class JSConfigRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// OpenId
    /// </summary>
    public string OpenId { get; set; }
    /// <summary>
    /// UnionId
    /// </summary>
    public string Debug { get; set; }
}
