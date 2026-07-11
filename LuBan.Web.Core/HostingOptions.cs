/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core
*文件名： HostingOptions
*版本号： V1.0.0.0
*唯一标识：22b62be0-c35d-4419-958c-b28b2d5d1599
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/7/20 18:15:47
*描述：HostingOptions
*
*=================================================
*修改标记
*修改时间：2023/7/20 18:15:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：HostingOptions
*
*****************************************************************************/
namespace LuBan.Web.Core;

/// <summary>
/// HostingOptions,
/// appsettings.json
/// </summary>
[DataContract]
public class HostingOptions : BaseSingleInstance<HostingOptions>
{
    /// <summary>
    /// 服务名称
    /// </summary>
    [DataMember, Required]
    public string ServiceName { get; set; }

    /// <summary>
    /// api服务域名
    /// </summary>
    [DataMember, Required]
    public string Domain { get; set; }

    /// <summary>
    /// 环境
    /// </summary>
    [DataMember]
    public string? Environment { get; set; }

    /// <summary>
    /// 是否启用分布式锁
    /// </summary>
    [DataMember]
    public bool EnableRedisCache { get; set; } = false;

    /// <summary>
    /// 是否启用后台任务
    /// </summary>
    [DataMember]
    public bool EnableBackgroundJob { get; set; } = true;

    /// <summary>
    /// 后台任务名称列表，空默认全部启用
    /// </summary>
    [DataMember]
    public List<string> BackgroundJobNames { get; set; } = [];

    /// <summary>
    /// 是否启用健康检查
    /// </summary>
    [DataMember]
    public bool EnableHealthCheck { get; set; } = false;


    /// <summary>
    /// 是否启用视频缩略图
    /// </summary>
    [DataMember]
    public bool EnableVideoThumbnail { get; set; }

    /// <summary>
    /// aspnetcore配置
    /// </summary>
    [DataMember, Required]
    public AppOptions AppOptions { get; set; }

    /// <summary>
    /// nacos配置
    /// </summary>

    [DataMember(Name = "NacosConfig")]
    public NacosConfig? NacosConfig { get; set; }


    #region static

    static HostingOptions _hostingOptions;

    /// <summary>
    /// appsettings.json
    /// </summary>
    /// <returns></returns>
    public static HostingOptions Default
    {
        get
        {
            if (_hostingOptions == null)
            {

                var options = ConfigUtil.Read<HostingOptions>();
                if (options == null) throw new InvalidDataException($"当前配置文件中缺少HostingOptions配置");
                _hostingOptions = options;
            }
            return _hostingOptions;
        }
    }

    #endregion

}
