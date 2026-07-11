/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage
*文件名： UrlUpdateHelper
*版本号： V1.0.0.0
*唯一标识：af627f18-3e46-44d4-9da6-6eb1b5bf4a05
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/9 18:04:01
*描述：
*用于处理标记了NewUrlAttribute的字段，更新过期的URL为有效的临时URL
*=================================================
*修改标记
*修改时间：2024/7/9 18:04:01
*修改人： yswenli
*版本号： V1.0.0.0
*描述：用于处理标记了NewUrlAttribute的字段，更新过期的URL为有效的临时URL
*=================================================
*修改标记
*修改时间：2026/1/6
*修改人： yswenli
*版本号： V1.0.1.0
*描述：用于处理标记了NewUrlAttribute的字段，更新过期的URL为有效的临时URL
*
*****************************************************************************/

using LuBan.Common.Data;

using System.Collections;
using System.Reflection;

namespace LuBan.CloudStorage
{
    /// <summary>
    /// 用于处理标记了NewUrlAttribute的字段，更新过期的URL为有效的临时URL
    /// </summary>
    public static class UrlUpdateHelper
    {
        /// <summary>
        /// 更新对象中所有标记了NewUrlAttribute的字段或属性的URL
        /// </summary>
        /// <param name="obj">要更新的对象（可以是单个对象或集合）</param>
        public static async Task UpdateUrlsAsync(object obj)
        {
            if (obj == null)
                return;

            var type = obj.GetType();

            if (!type.IsClass || type == typeof(string) || type == typeof(object)) return;

            // 处理集合类型
            if (obj is Result r)
            {
                if (r.Result == null) return;
                if (r.Result is IEnumerable list)
                {
                    foreach (var item in list)
                    {
                        await UpdateUrlsAsync(item);
                    }
                    return;
                }
                else
                {
                    await UpdateUrlsAsync(r.Result);
                }
            }

            var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field);

            foreach (var member in members)
            {
                var attribute = member.GetCustomAttribute<NewUrlAttribute>();
                if (attribute == null)
                    continue;

                string? currentUrl = null;
                Type? memberType = null;

                if (member.MemberType == MemberTypes.Property)
                {
                    var property = (PropertyInfo)member;
                    if (!property.CanRead || !property.CanWrite)
                        continue;

                    currentUrl = property.GetValue(obj) as string;
                    memberType = property.PropertyType;
                }
                else if (member.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo)member;
                    currentUrl = field.GetValue(obj) as string;
                    memberType = field.FieldType;
                }

                if (memberType != typeof(string) || currentUrl.IsNullOrEmpty())
                    continue;

                try
                {
                    // 仅处理带过期标识的URL
                    if (!IsExpiringUrl(currentUrl))
                        continue;

                    // 从NacosConfigUtil中读取当前配置
                    var uploadOptions = NacosConfigUtil.Read<UploadOptions>();
                    if (uploadOptions == null || !uploadOptions.EnableCloudStorage)
                        continue;

                    // 创建云存储客户端
                    var cloudStorageClient = CloundStorageClientFactory.Create(uploadOptions.CloudStorageOptions);

                    // 提取完整的objectKey（包含路径）
                    string fileName = ExtractObjectKey(currentUrl);
                    if (fileName.IsNullOrEmpty())
                        continue;

                    // 生成新的临时URL
                    var expireTime = DateTimeOffset.Now.AddMinutes(attribute.ExpireMinutes);
                    var newUrl = await cloudStorageClient.GetSasUri(fileName, expireTime);

                    if (newUrl.IsNotNullOrEmpty())
                    {
                        // 更新对象中的值
                        if (member.MemberType == MemberTypes.Property)
                        {
                            ((PropertyInfo)member).SetValue(obj, newUrl);
                        }
                        else if (member.MemberType == MemberTypes.Field)
                        {
                            ((FieldInfo)member).SetValue(obj, newUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"更新URL失败: {member.Name}");
                }
            }
        }

        /// <summary>
        /// 判断URL是否为带过期标识的URL
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>是否为带过期标识的URL</returns>
        private static bool IsExpiringUrl(string url)
        {
            if (url.IsNullOrEmpty())
                return false;

            try
            {
                // 常见的云存储临时URL参数
                var expireParams = new[] { "X-Amz-Algorithm", "X-Amz-Date", "X-Amz-Expires", "X-Amz-Credential", "X-Amz-Signature", "X-Oss-Expires" };

                // 检查URL是否包含查询参数
                int queryIndex = url.IndexOf('?');
                if (queryIndex == -1)
                    return false;

                // 检查URL是否包含至少一个过期标识参数
                string queryString = url.Substring(queryIndex + 1);
                return expireParams.Any(param => queryString.IndexOf(param, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            catch
            {
                // 如果解析失败，认为不是带过期标识的URL
                return false;
            }
        }

        /// <summary>
        /// 从URL中提取完整的objectKey（包含路径）
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>完整的objectKey</returns>
        private static string ExtractObjectKey(string url)
        {
            if (url.IsNullOrEmpty())
                return string.Empty;
            try
            {
                var uri = new Uri(url);
                return uri.LocalPath;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
