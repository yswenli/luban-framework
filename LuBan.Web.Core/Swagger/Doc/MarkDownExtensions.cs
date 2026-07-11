/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger.Doc
*文件名： MarkDownExtensions
*版本号： V1.0.0.0
*唯一标识：6960bb63-8e2d-471d-8823-cb2e1c071c42
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 11:28:14
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/6/21 11:28:14
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger.Doc
{
    /// <summary>
    /// MarkDown扩展方法类
    /// </summary>
    public static class MarkDownExtensions
    {
        /// <summary>
        /// 生成Markdown标题（# 号表示），len为标题级别，最大6级
        /// </summary>
        /// <param name="s">标题内容</param>
        /// <param name="len">标题级别（1-6）</param>
        /// <returns>Markdown格式的标题字符串</returns>
        public static string H(this string s, int len = 1) => $"{(len <= 6 && len > 0 ? string.Join("", Enumerable.Range(0, len).Select(x => "#")) : "#")} {s}";

        /// <summary>
        /// 生成Markdown加粗文本
        /// </summary>
        /// <param name="s">要加粗的内容</param>
        /// <returns>加粗格式的字符串</returns>
        public static string B(this string s) => $"**{s}**";

        /// <summary>
        /// 生成Markdown斜体文本
        /// </summary>
        /// <param name="s">要斜体的内容</param>
        /// <returns>斜体格式的字符串</returns>
        public static string I(this string s) => $"*{s}*";

        /// <summary>
        /// 生成Markdown无序列表项，len为前置空格数
        /// </summary>
        /// <param name="s">列表项内容</param>
        /// <param name="len">前置空格数</param>
        /// <returns>Markdown格式的无序列表项</returns>
        public static string Li(this string s, int len = 0) => $"{string.Join("", Enumerable.Range(0, len).Select(x => " "))}- {s}";

        /// <summary>
        /// 生成Markdown引用文本，len为引用级别
        /// </summary>
        /// <param name="s">引用内容</param>
        /// <param name="len">引用级别（1-3）</param>
        /// <returns>Markdown格式的引用字符串</returns>
        public static string Ref(this string s, int len = 1) => $"{(len <= 3 && len > 0 ? string.Join("", Enumerable.Range(0, len).Select(x => ">")) : ">")} {s}";

        /// <summary>
        /// 生成Markdown分割线
        /// </summary>
        /// <param name="s">无实际作用，仅为扩展方法格式</param>
        /// <returns>Markdown格式的分割线</returns>
        public static string Line(this string s) => $"***";

        /// <summary>
        /// 在字符串后添加Markdown换行符
        /// </summary>
        /// <param name="s">原始字符串</param>
        /// <returns>带换行的字符串</returns>
        public static string NewLine(this string s) => $"{s}  {Environment.NewLine}";

        /// <summary>
        /// 生成Markdown超链接
        /// </summary>
        /// <param name="name">链接显示名称</param>
        /// <param name="url">链接地址</param>
        /// <param name="title">链接标题（可选）</param>
        /// <returns>Markdown格式的超链接</returns>
        public static string Link(this string name, string url, string title = "") => $"[{name}]({url},{title})";

        /// <summary>
        /// 在字符串后添加Markdown换行符（与NewLine一致）
        /// </summary>
        /// <param name="s">原始字符串</param>
        /// <returns>带换行的字符串</returns>
        public static string Br(this string s) => $"{s}  {Environment.NewLine}";

        /// <summary>
        /// 生成Markdown代码块，默认类型为json
        /// </summary>
        /// <param name="s">代码内容</param>
        /// <param name="type">代码类型（如json、csharp等）</param>
        /// <returns>Markdown格式的代码块</returns>
        public static string Code(this string s, string type = "json") => $"```json{Environment.NewLine}{s}{Environment.NewLine}```{Environment.NewLine}";

        /// <summary>
        /// 将对象集合转换为Markdown表格
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="list">对象集合</param>
        /// <param name="headers">表头（可选，若不传则自动用属性名）</param>
        /// <param name="selector">自定义列选择器（可选）</param>
        /// <returns>Markdown表格字符串</returns>
        public static string ToMarkdownTable<T>(this IEnumerable<T> list, string[]? headers = null, Func<T, object[]>? selector = null)
        {
            if (list == null) return string.Empty;
            var items = list.ToList();
            if (!items.Any()) return string.Empty;
            var type = typeof(T);
            var props = type.GetProperties();
            // 自动获取表头
            var head = headers ?? props.Select(p => p.Name).ToArray();
            var sb = new StringBuilder();
            sb.Append("|");
            sb.Append(string.Join("|", head));
            sb.AppendLine("|");
            sb.Append("|");
            sb.Append(string.Join("|", head.Select(_ => ":----:")));
            sb.AppendLine("|");
            foreach (var item in items)
            {
                object[] values;
                if (selector != null)
                    values = selector(item);
                else
                    values = props?.Select(p => p.GetValue(item, null) ?? "").ToArray() ?? Array.Empty<object>();
                sb.Append("|");
                sb.Append(string.Join("|", values.Select(v => v?.ToString()?.Replace("|", "/") ?? "")));
                sb.AppendLine("|");
            }
            return sb.ToString();
        }
    }
}
