/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives.VH.Models
*文件名： Result
*版本号： V1.0.0.0
*唯一标识：95a2cf20-ab4b-4b52-abcf-2dea7639665b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/29 11:29:37
*描述：微吼接口返回值
*
*=================================================
*修改标记
*修改时间：2025/4/29 11:29:37
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微吼接口返回值
*
*****************************************************************************/
namespace LuBan.Lives.VH.Models
{
    /// <summary>
    /// 微吼接口返回值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T>
    {
        /// <summary>
        /// 异常码,
        /// https://saas-doc.vhall.com/opendocs/show/1414
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// 异常消息
        /// </summary>
        [JsonProperty("msg")]
        public string Message { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        [JsonProperty("data")]
        public T data { get; set; }

        /// <summary>
        /// 请求id
        /// </summary>
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
    }
    /// <summary>
    /// 微吼接口返回值
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 异常码,
        /// https://saas-doc.vhall.com/opendocs/show/1414
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// 异常消息
        /// </summary>
        [JsonProperty("msg")]
        public string Message { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        [JsonProperty("data")]
        public dynamic data { get; set; }

        /// <summary>
        /// 请求id
        /// </summary>
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
    }
}
