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
    public class AddressComponentData
    {
        public string Nation { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string Street_Number { get; set; }
    }
    public class AdInfoData
    {
        public string Nation { get; set; }
        public string Adcode { get; set; }
        public string City { get; set; }
        public string District { get; set; }
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
    public class LocationData
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
    public class AdInfoData
    {
        public string Adcode { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
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

    public class LocationData
    {
        public double Lat { get; set; }
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
    public class LocationData
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}