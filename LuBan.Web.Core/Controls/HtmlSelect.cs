/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Controls
*文件名： HtmlSelect
*版本号： V1.0.0.0
*唯一标识：a4aab7af-e7e8-4a09-b2ff-4eb6722b912b
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/10/25 14:33:29
*描述：html的select控件
*
*=================================================
*修改标记
*修改时间：2022/10/25 14:33:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：html的select控件
*
*****************************************************************************/



namespace LuBan.Web.Core.Controls;

/// <summary>
/// html的select控件
/// </summary>
public class HtmlSelect
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 样式名称
    /// </summary>
    public string ClassName { get; set; } = "select2";
    /// <summary>
    /// 默认值
    /// </summary>
    public dynamic? DefaultValue { get; set; }
    /// <summary>
    /// 值
    /// </summary>
    public IEnumerable<HtmlSelectOption> Data { get; set; }
    /// <summary>
    /// style
    /// </summary>
    public string Style { get; set; } = "width: 100%;";

    /// <summary>
    /// select模型
    /// </summary>
    public HtmlSelect()
    {
        Data = new List<HtmlSelectOption>();
    }

    /// <summary>
    /// select模型
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="defaultValue"></param>
    /// <param name="data"></param>
    /// <param name="className"></param>
    /// <param name="style"></param>
    public HtmlSelect(string name, string description, dynamic? defaultValue, IEnumerable<HtmlSelectOption> data, string className = "select2", string style = "width: 100%;")
    {
        Name = name;
        Description = description;
        DefaultValue = defaultValue;
        ClassName = className;
        Data = data;
        Style = style;
    }

    /// <summary>
    /// select模型
    /// </summary>
    /// <param name="name"></param>
    /// <param name="data"></param>
    public HtmlSelect(string name, IEnumerable<HtmlSelectOption> data)
    {
        Name = name;
        Description = name;
        DefaultValue = "";
        ClassName = "";
        Data = data;
    }
}

/// <summary>
/// 快捷select控件操作
/// </summary>
public static class HtmlSelectExtension
{
    /// <summary>
    /// 构建select控件
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="defaultValue"></param>
    /// <param name="className"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    public static HtmlSelect ToHtmlSelect(this IEnumerable<HtmlSelectOption> data,
        string name,
        string description,
        dynamic? defaultValue = null,
        string className = "select2",
        string style = "width:100%;")
    {
        if (data == null) data = new List<HtmlSelectOption>();
        return new HtmlSelect()
        {
            Name = name,
            Description = description,
            DefaultValue = defaultValue,
            Data = data,
            ClassName = className,
            Style = style
        };
    }

    /// <summary>
    /// 构建select控件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dic"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="defaultValue"></param>
    /// <param name="className"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    public static HtmlSelect ToHtmlSelect<T>(this Dictionary<string, T> dic,
        string name,
        string description,
        dynamic? defaultValue = null,
        string className = "select2",
        string style = "width:100%;")
    {
        var list = new List<HtmlSelectOption>();

        if (dic != null && dic.Count > 0)
        {
            foreach (var item in dic)
            {
                list.Add(new HtmlSelectOption()
                {
                    Text = item.Key,
                    Value = item.Value
                });
            }
        }

        return ToHtmlSelect(list, name, description, defaultValue, className, style);
    }
    /// <summary>
    /// 构建select控件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="defaultValue"></param>
    /// <param name="className"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    public static HtmlSelect ToHtmlSelect<T>(this IEnumerable<T> data,
        string name,
        string description,
        dynamic? defaultValue = null,
        string className = "select2",
        string style = "width:100%;")
    {
        var list = new List<HtmlSelectOption>();

        if (data != null)
        {
            foreach (var item in data)
            {
                list.Add(new HtmlSelectOption()
                {
                    Text = item?.ToString() ?? "",
                    Value = item
                });
            }
        }

        return ToHtmlSelect(list, name, description, defaultValue, className, style);
    }

    /// <summary>
    /// 获取select html
    /// </summary>
    /// <param name="htmlHelper"></param>
    /// <param name="selectModel"></param>
    /// <returns></returns>
    public static IHtmlContent RawSelectForm(this IHtmlHelper htmlHelper, HtmlSelect selectModel)
    {
        var sb = new StringBuilder();
        try
        {
            sb.Append($"<label for=\"{selectModel.Name}\">{(selectModel.Description.IsNullOrEmpty() ? selectModel.Name : selectModel.Description)}</label>");
            if (selectModel.Style.IsNullOrEmpty())
            {
                sb.Append($"<select id=\"{selectModel.Name}\" name=\"{selectModel.Name}\" class=\"{selectModel.ClassName}\">");
            }
            else
            {
                sb.Append($"<select id=\"{selectModel.Name}\" name=\"{selectModel.Name}\" class=\"{selectModel.ClassName}\" style=\"{selectModel.Style}\">");
            }
            sb.Append($"<option value=\"\">请选择</option>");
            if (selectModel != null && selectModel.Data != null)
            {
                foreach (var item in selectModel.Data)
                {
                    if (item.Value == selectModel.DefaultValue)
                    {
                        sb.Append($"<option value=\"{item.Value}\" selected>{item.Text}</option>");
                    }
                    else
                    {
                        sb.Append($"<option value=\"{item.Value}\">{item.Text}</option>");
                    }
                }
            }
            sb.Append("</select>");
        }
        catch (Exception ex)
        {
            Logger.Error("HtmlSelectExtension.SelectForm", ex);
        }
        return htmlHelper.Raw(sb.ToString());
    }

    /// <summary>
    /// 获取select html 
    /// </summary>
    /// <param name="htmlHelper"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="defaultValue"></param>
    /// <param name="data"></param>
    /// <param name="className"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    public static IHtmlContent RawSelectForm(this IHtmlHelper htmlHelper, string name, string description, dynamic defaultValue, IEnumerable<HtmlSelectOption> data, string className = "select2", string style = "width:100%;")
    {
        return htmlHelper.RawSelectForm(new HtmlSelect(name, description, defaultValue, data, className, style));
    }

    /// <summary>
    /// 获取select html
    /// </summary>
    /// <param name="htmlHelper"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="className"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    public static IHtmlContent RawSelectForm(this IHtmlHelper htmlHelper, string name, IEnumerable<HtmlSelectOption> data, string className = "select2", string style = "width:100%;")
    {
        return htmlHelper.RawSelectForm(new HtmlSelect(name, string.Empty, null, data, className, style));
    }

    /// <summary>
    /// 获取select html
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="htmlHelper"></param>
    /// <param name="name"></param>
    /// <param name="dic"></param>
    /// <param name="className"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    public static IHtmlContent RawSelectForm<T>(this IHtmlHelper htmlHelper, string name, Dictionary<string, T> dic, string className = "select2", string style = "width:100%;")
    {
        return htmlHelper.RawSelectForm(dic.ToHtmlSelect(name, string.Empty, null, className, style));
    }

    /// <summary>
    /// 获取select html
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="htmlHelper"></param>
    /// <param name="name"></param>
    /// <param name="list"></param>
    /// <param name="className"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    public static IHtmlContent RawSelectForm<T>(this IHtmlHelper htmlHelper, string name, IEnumerable<T> list, string className = "select2", string style = "width:100%;")
    {
        return htmlHelper.RawSelectForm(list.ToHtmlSelect(name, string.Empty, null, className, style));
    }
}
