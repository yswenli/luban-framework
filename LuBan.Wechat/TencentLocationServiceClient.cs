/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Wechat
*文件名： TencentLocationServiceClient
*版本号： V1.0.0.0
*唯一标识：8d549389-72db-4725-8833-7faeebce127e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/6/25 14:08:24
*描述：腾讯位置服务客户端
*
*=================================================
*修改标记
*修改时间：2025/6/25 14:08:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：腾讯位置服务客户端
*
*****************************************************************************/
namespace LuBan.Wechat;


/// <summary>
/// 腾讯位置服务客户端，
/// 默认配置格式：TencentLocationService:Key
/// </summary>
public class TencentLocationServiceClient : BaseSingleInstance<TencentLocationServiceClient>
{
    HttpClientProxy _httpClientUtil;

    /// <summary>
    /// 开发密钥（Key）	
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// 腾讯位置服务客户端
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="Exception"></exception>
    public TencentLocationServiceClient(string? key)
    {
        if (key.IsNullOrEmpty()) throw new Exception("开发密钥（Key）不能为空");
        Key = key;
        _httpClientUtil = HttpClientProxy.Create("https://apis.map.qq.com", useLog: true);
    }

    /// <summary>
    /// 腾讯位置服务客户端
    /// </summary>
    public TencentLocationServiceClient() : this(ConfigUtil.Read("TencentLocationService:Key"))
    {

    }
    /// <summary>
    /// 逆地址解析，根据经纬度获取详细地址信息
    /// 文档：https://lbs.qq.com/service/webService/webServiceGuide/address/Gcoder
    /// </summary>
    /// <param name="lat">纬度</param>
    /// <param name="lng">经度</param>
    /// <returns>返回腾讯位置服务的结构化结果对象</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<TencentGeocoderResult?> GetLocationAsync(double lat, double lng)
    {
        if (lat == 0 || lng == 0) throw new ArgumentNullException("经纬度不能为空");
        var location = $"{lat},{lng}";
        var url = $"/ws/geocoder/v1/?location={Uri.EscapeDataString(location)}&key={Key}";
        var responseBytes = await _httpClientUtil.GetBytesAsync(url);
        if (responseBytes == null || responseBytes.Length == 0)
        {
            throw new Exception("获取位置失败: 响应内容为空");
        }
        var json = System.Text.Encoding.UTF8.GetString(responseBytes);
        // 反序列化为结构化对象
        return json.ToObject<TencentGeocoderResult>();
    }

    /// <summary>
    /// 地点搜索（周边搜索）
    /// 文档：https://lbs.qq.com/service/webService/webServiceGuide/search/webServiceSearch
    /// </summary>
    /// <param name="keyword">搜索关键字</param>
    /// <param name="lat">中心点纬度</param>
    /// <param name="lng">中心点经度</param>
    /// <param name="radius">搜索半径，单位米，默认1000</param>
    /// <param name="pageSize">每页条数，最大20，默认10</param>
    /// <param name="pageIndex">页码，默认1</param>
    /// <returns>返回腾讯地点搜索结构化结果</returns>
    public async Task<TencentPlaceSearchResult?> SearchPlacesAsync(string keyword, double lat, double lng, int radius = 1000, int pageSize = 10, int pageIndex = 1)
    {
        if (string.IsNullOrWhiteSpace(keyword)) throw new ArgumentNullException(nameof(keyword), "搜索关键字不能为空");
        if (lat == 0 || lng == 0) throw new ArgumentNullException("经纬度不能为空");
        if (radius < 10 || radius > 1000) throw new ArgumentOutOfRangeException(nameof(radius), "搜索半径范围为10-1000米");
        if (pageSize < 1 || pageSize > 20) pageSize = 10;
        if (pageIndex < 1) pageIndex = 1;
        var url = $"/ws/place/v1/search?keyword={Uri.EscapeDataString(keyword)}&boundary=nearby({lat},{lng},{radius})&page_size={pageSize}&page_index={pageIndex}&key={Key}";
        var responseBytes = await _httpClientUtil.GetBytesAsync(url);
        if (responseBytes == null || responseBytes.Length == 0)
        {
            throw new Exception("地点搜索失败: 响应内容为空");
        }
        var json = System.Text.Encoding.UTF8.GetString(responseBytes);
        return json.ToObject<TencentPlaceSearchResult>();
    }

    /// <summary>
    /// 指定城市/区域地点搜索
    /// 文档：https://lbs.qq.com/service/webService/webServiceGuide/search/webServiceSearch
    /// </summary>
    /// <param name="keyword">搜索关键字</param>
    /// <param name="region">城市或区域名称，如"北京"或"北京市海淀区"</param>
    /// <param name="pageSize">每页条数，最大20，默认10</param>
    /// <param name="pageIndex">页码，默认1</param>
    /// <returns>返回腾讯地点搜索结构化结果</returns>
    public async Task<TencentPlaceSearchResult?> SearchPlacesByRegionAsync(string keyword, string region, int pageSize = 10, int pageIndex = 1)
    {
        if (string.IsNullOrWhiteSpace(keyword)) throw new ArgumentNullException(nameof(keyword), "搜索关键字不能为空");
        if (string.IsNullOrWhiteSpace(region)) throw new ArgumentNullException(nameof(region), "城市/区域不能为空");
        if (pageSize < 1 || pageSize > 20) pageSize = 10;
        if (pageIndex < 1) pageIndex = 1;
        var url = $"/ws/place/v1/search?keyword={Uri.EscapeDataString(keyword)}&boundary=region({Uri.EscapeDataString(region)})&page_size={pageSize}&page_index={pageIndex}&key={Key}";
        var responseBytes = await _httpClientUtil.GetBytesAsync(url);
        if (responseBytes == null || responseBytes.Length == 0)
        {
            throw new Exception("区域地点搜索失败: 响应内容为空");
        }
        var json = System.Text.Encoding.UTF8.GetString(responseBytes);
        return json.ToObject<TencentPlaceSearchResult>();
    }

    /// <summary>
    /// 坐标转换（支持批量）
    /// 文档：https://lbs.qq.com/service/webService/webServiceGuide/webServiceTranslate
    /// </summary>
    /// <param name="locations">待转换坐标集合，(纬度, 经度)</param>
    /// <param name="type">输入坐标类型，1-GPS，2-sogou经纬度，3-baidu经纬度，4-mapbar经纬度，6-sogou墨卡托</param>
    /// <returns>返回腾讯坐标转换结构化结果</returns>
    public async Task<TencentCoordTranslateResult?> TranslateCoordinatesAsync(IEnumerable<(double lat, double lng)> locations, int type)
    {
        if (locations == null || !locations.Any()) throw new ArgumentNullException(nameof(locations), "坐标集合不能为空");
        if (type != 1 && type != 2 && type != 3 && type != 4 && type != 6) throw new ArgumentOutOfRangeException(nameof(type), "type参数不合法");
        var locStr = string.Join(";", locations.Select(l => $"{l.lat},{l.lng}"));
        var url = $"/ws/coord/v1/translate?locations={Uri.EscapeDataString(locStr)}&type={type}&key={Key}";
        var responseBytes = await _httpClientUtil.GetBytesAsync(url);
        if (responseBytes == null || responseBytes.Length == 0)
        {
            throw new Exception("坐标转换失败: 响应内容为空");
        }
        var json = System.Text.Encoding.UTF8.GetString(responseBytes);
        return json.ToObject<TencentCoordTranslateResult>();
    }

    /// <summary>
    /// 获取省市区三级行政区划列表
    /// 文档：https://lbs.qq.com/service/webService/webServiceGuide/search/webServiceDistrict
    /// </summary>
    /// <param name="structType">是否返回嵌套结构，1为嵌套结构</param>
    /// <returns>返回省市区结构化结果</returns>
    public async Task<TencentDistrictResult?> GetDistrictListAsync(int structType = 1)
    {
        var url = $"/ws/district/v1/list?key={Key}&struct_type={structType}";
        var responseBytes = await _httpClientUtil.GetBytesAsync(url);
        if (responseBytes == null || responseBytes.Length == 0)
        {
            throw new Exception("获取省市区列表失败: 响应内容为空");
        }
        var json = System.Text.Encoding.UTF8.GetString(responseBytes);
        return json.ToObject<TencentDistrictResult>();
    }

    /// <summary>
    /// 获取下级行政区划
    /// 文档：https://lbs.qq.com/service/webService/webServiceGuide/search/webServiceDistrict
    /// </summary>
    /// <param name="parentId">父级行政区划ID(adcode)，为空时返回省级</param>
    /// <returns>返回下级行政区划结构化结果</returns>
    public async Task<TencentDistrictResult?> GetDistrictChildrenAsync(string? parentId = null)
    {
        var url = $"/ws/district/v1/getchildren?key={Key}";
        if (!string.IsNullOrWhiteSpace(parentId))
        {
            url += $"&id={Uri.EscapeDataString(parentId)}";
        }
        var responseBytes = await _httpClientUtil.GetBytesAsync(url);
        if (responseBytes == null || responseBytes.Length == 0)
        {
            throw new Exception("获取下级行政区划失败: 响应内容为空");
        }
        var json = System.Text.Encoding.UTF8.GetString(responseBytes);
        return json.ToObject<TencentDistrictResult>();
    }

    /// <summary>
    /// 行政区划搜索
    /// 文档：https://lbs.qq.com/service/webService/webServiceGuide/search/webServiceDistrict
    /// </summary>
    /// <param name="keyword">搜索关键词或adcode，支持多个adcode英文逗号分隔</param>
    /// <returns>返回行政区划搜索结构化结果</returns>
    public async Task<TencentDistrictResult?> SearchDistrictAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) throw new ArgumentNullException(nameof(keyword), "搜索关键词不能为空");
        var url = $"/ws/district/v1/search?key={Key}&keyword={Uri.EscapeDataString(keyword)}";
        var responseBytes = await _httpClientUtil.GetBytesAsync(url);
        if (responseBytes == null || responseBytes.Length == 0)
        {
            throw new Exception("行政区划搜索失败: 响应内容为空");
        }
        var json = System.Text.Encoding.UTF8.GetString(responseBytes);
        return json.ToObject<Models.TencentDistrictResult>();
    }
}
