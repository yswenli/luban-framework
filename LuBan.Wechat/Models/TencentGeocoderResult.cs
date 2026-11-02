/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Wechat.Models
*文件名： TencentGeocoderResult
*版本号： V1.0.0.0
*唯一标识：8d549389-72db-4725-8833-7faeebce127e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/6/25 14:08:24
*描述：腾讯逆地址解析结果
*
*=================================================
*修改标记
*修改时间：2025/6/25 14:08:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：腾讯逆地址解析结果
*
*****************************************************************************/

namespace LuBan.Wechat.Models;

/// <summary>
/// 腾讯逆地址解析结果
/// </summary>
public class TencentGeocoderResult
{
    /// <summary>
    /// 状态码，0为正常
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// 状态说明
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// 本次请求的唯一标识
    /// </summary>
    public string RequestId { get; set; }
    /// <summary>
    /// 逆地址解析结果
    /// </summary>
    public ResultData Result { get; set; }

    /// <summary>
    /// 逆地址解析的结果数据，包含格式化地址、地址部件与行政区划信息。
    /// </summary>
    public class ResultData
    {
        /// <summary>
        /// 标准格式化地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 地址部件
        /// </summary>
        public AddressComponentData Address_Components { get; set; }
        /// <summary>
        /// 行政区划信息
        /// </summary>
        public AdInfoData Ad_Info { get; set; }
        // 可根据需要扩展POI等字段
    }
    /// <summary>
    /// 逆地址解析返回的地址部件，按国家、省、市、区、街道逐级拆分。
    /// </summary>
    public class AddressComponentData
    {
        /// <summary>
        /// 国家名称（接口字段 Nation）。
        /// </summary>
        public string Nation { get; set; }
        /// <summary>
        /// 省份名称（接口字段 Province）。
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 城市名称（接口字段 City）。
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区县名称（接口字段 District）。
        /// </summary>
        public string District { get; set; }
        /// <summary>
        /// 街道名称（接口字段 Street）。
        /// </summary>
        public string Street { get; set; }
        /// <summary>
        /// 门牌号（接口字段 Street_Number）。
        /// </summary>
        public string Street_Number { get; set; }
    }
    /// <summary>
    /// 逆地址解析返回的行政区划信息，含行政区划编码与命名。
    /// </summary>
    public class AdInfoData
    {
        /// <summary>
        /// 国家名称（接口字段 Nation）。
        /// </summary>
        public string Nation { get; set; }
        /// <summary>
        /// 行政区划编码，即 adcode（接口字段 Adcode）。
        /// </summary>
        public string Adcode { get; set; }
        /// <summary>
        /// 城市名称（接口字段 City）。
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区县名称（接口字段 District）。
        /// </summary>
        public string District { get; set; }
        /// <summary>
        /// 省份名称（接口字段 Province）。
        /// </summary>
        public string Province { get; set; }
    }
}


/// <summary>
/// 腾讯地点搜索结果
/// </summary>
public class TencentPlaceSearchResult
{
    /// <summary>
    /// 状态码，0为正常
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// 状态说明
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// 搜索结果总数
    /// </summary>
    public int Count { get; set; }
    /// <summary>
    /// 本次请求的唯一标识
    /// </summary>
    public string Request_Id { get; set; }
    /// <summary>
    /// 地点POI列表
    /// </summary>
    public List<PoiData> Data { get; set; }

    /// <summary>
    /// 地点搜索返回的单个 POI，包含名称、地址、分类、坐标与距离。
    /// </summary>
    public class PoiData
    {
        /// <summary>
        /// POI唯一标识
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// 坐标
        /// </summary>
        public LocationData Location { get; set; }
        /// <summary>
        /// 行政区划信息
        /// </summary>
        public AdInfoData Ad_Info { get; set; }
        /// <summary>
        /// 距离中心点距离
        /// </summary>
        public double _Distance { get; set; }
    }
    /// <summary>
    /// 地点搜索返回的 POI 经纬度坐标。
    /// </summary>
    public class LocationData
    {
        /// <summary>
        /// 纬度（接口字段 Lat）。
        /// </summary>
        public double Lat { get; set; }
        /// <summary>
        /// 经度（接口字段 Lng）。
        /// </summary>
        public double Lng { get; set; }
    }
    /// <summary>
    /// 地点搜索返回的 POI 所属行政区划信息。
    /// </summary>
    public class AdInfoData
    {
        /// <summary>
        /// 行政区划编码，即 adcode（接口字段 Adcode）。
        /// </summary>
        public string Adcode { get; set; }
        /// <summary>
        /// 省份名称（接口字段 Province）。
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 城市名称（接口字段 City）。
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区县名称（接口字段 District）。
        /// </summary>
        public string District { get; set; }
    }
}

/// <summary>
/// 腾讯坐标转换结果
/// </summary>
public class TencentCoordTranslateResult
{
    /// <summary>
    /// 状态码，0为正常
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// 状态说明
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// 转换后坐标列表
    /// </summary>
    public List<LocationData> Locations { get; set; }

    /// <summary>
    /// 坐标转换后单个位置的经纬度结果。
    /// </summary>
    public class LocationData
    {
        /// <summary>
        /// 转换后的纬度（接口字段 Lat）。
        /// </summary>
        public double Lat { get; set; }
        /// <summary>
        /// 转换后的经度（接口字段 Lng）。
        /// </summary>
        public double Lng { get; set; }
    }
}

/// <summary>
/// 腾讯行政区划查询结果
/// </summary>
public class TencentDistrictResult
{
    /// <summary>
    /// 状态码，0为正常
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// 状态说明
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// 请求唯一标识
    /// </summary>
    public string Request_Id { get; set; }
    /// <summary>
    /// 行政区划数据版本
    /// </summary>
    public long Data_Version { get; set; }
    /// <summary>
    /// 行政区划结果
    /// </summary>
    public List<DistrictData> Result { get; set; }

    /// <summary>
    /// 行政区划查询返回的单个区划节点，可递归包含下级区划。
    /// </summary>
    public class DistrictData
    {
        /// <summary>
        /// 行政区划唯一标识（adcode）
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 简称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 全称
        /// </summary>
        public string Fullname { get; set; }
        /// <summary>
        /// 经纬度
        /// </summary>
        public LocationData Location { get; set; }
        /// <summary>
        /// 拼音
        /// </summary>
        public List<string> Pinyin { get; set; }
        /// <summary>
        /// 行政区划级别
        /// </summary>
        public int? Level { get; set; }
        /// <summary>
        /// 下级区划
        /// </summary>
        public List<DistrictData> Districts { get; set; }
    }
    /// <summary>
    /// 行政区划节点的中心点经纬度。
    /// </summary>
    public class LocationData
    {
        /// <summary>
        /// 中心点纬度（接口字段 Lat）。
        /// </summary>
        public double Lat { get; set; }
        /// <summary>
        /// 中心点经度（接口字段 Lng）。
        /// </summary>
        public double Lng { get; set; }
    }
}