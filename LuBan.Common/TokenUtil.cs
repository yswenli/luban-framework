/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common.Encryption
*文件名： TokenUtil
*版本号： V1.0.0.0
*唯一标识：21506346-7fff-4fae-a260-14386825e643
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/8/27 9:35:22
*描述：token工具类
*
*=====================================================================
*修改标记
*修改时间：2021/8/27 9:35:22
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：token工具类
*
*****************************************************************************/
namespace LuBan.Common
{
    /// <summary>
    /// token工具类
    /// </summary>
    public static class TokenUtil
    {
        static readonly string _passwords = "RGV2ZWxvcGVkIGJ5IE1hc29uLldlbg==";

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <returns></returns>
        public static string GetTokenString(string version = "1.0")
        {
            var tokenInfo = new TokenInfo()
            {
                Version = version,
                DateTime = DateTimeUtil.Now
            };
            tokenInfo.ComparisonCode = tokenInfo.GetComparisonCode();
            var str = $"{tokenInfo.DateTime}*{tokenInfo.Version}*{tokenInfo.ComparisonCode}";
            var ebytes = AESUtil.Encrypt(Encoding.UTF8.GetBytes(str), _passwords);
            return Encrypt.Library.Base64Util.ToUriSafeEncode(ebytes);
        }

        /// <summary>
        /// 检验并返回 Token
        /// </summary>
        /// <param name="tokenStr"></param>
        /// <param name="err"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static TokenInfo? GetToken(string tokenStr, out string err, int timeOut = 10)
        {
            err = string.Empty;

            try
            {
                var ebytes = Encrypt.Library.Base64Util.ToUriSafeDecode(tokenStr);

                var str = Encoding.UTF8.GetString(AESUtil.Decrypt(ebytes, _passwords));

                var arr = str.Split("*");

                var tokenInfo = new TokenInfo()
                {
                    DateTime = DateTime.Parse(arr[0]),
                    Version = arr[1],
                    ComparisonCode = arr[2]
                };

                if (tokenInfo.ComparisonCode != tokenInfo.GetComparisonCode())
                {
                    err = "输入的token无效";

                    return null;
                }

                if (tokenInfo.DateTime.AddMinutes(timeOut) < DateTimeUtil.Now)
                {
                    err = "输入的Token已过期";

                    return null;
                }

                return tokenInfo;
            }
            catch
            {
                err = "输入的token无效";
            }

            return null;
        }

        /// <summary>
        /// 获取将对象序列化后的md5值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetToken(this object obj)
        {
            if (obj == null) return string.Empty;
            var json = obj.ToJson();
            if (!string.IsNullOrEmpty(json))
            {
                return MD5Util.GetMD5Str(json).EncodeForUriSafe();
            }
            return string.Empty;
        }

        #region TokenInfo
        /// <summary>
        /// 验证码实体
        /// </summary>
        public class TokenInfo
        {
            /// <summary>
            /// 版本
            /// </summary>
            [JsonProperty("v")]
            public string Version { get; set; }
            /// <summary>
            /// 时间
            /// </summary>
            [JsonProperty("d")]
            public DateTime DateTime { get; set; }
            /// <summary>
            /// 校验码
            /// </summary>
            [JsonProperty("c")]
            public string ComparisonCode { get; set; }

            /// <summary>
            /// 生成校验码
            /// </summary>
            /// <returns></returns>
            public string GetComparisonCode()
            {
                return MD5Util.GetHMACMD5($"Version={Version}&DateTime={DateTime}", _passwords).EncodeForUriSafe();
            }
        }
        #endregion
    }
}
