/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow
*文件名： ConvertExtention
*版本号： V1.0.0.0
*唯一标识：9332a27e-a866-478b-97e7-93a170e3c886
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/23 16:04:55
*描述：轻流配置
*
*=================================================
*修改标记
*修改时间：2024/12/23 16:04:55
*修改人： yswenli
*版本号： V1.0.0.0
*描述：轻流配置
*
*****************************************************************************/

namespace LuBan.Qingflow;

public static class ConvertExtention
{
    /// <summary>
    /// 将轻流数据中的字段转换成模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filedInfos"></param>
    /// <returns></returns>
    static T? ConvertTo<T>(List<FiledInfo> filedInfos)
         where T : class, IAppData
    {
        return filedInfos.Select(q => new TitleValue(q.QueTitle, q.Values?.FirstOrDefault()?.Value ?? ""))
            .ConvertToTitleValueCollection()
            .ConvertTo<T>();
    }

    /// <summary>
    /// 将轻流数据中的字段转换成模型列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="appDatas"></param>
    /// <returns></returns>
    public static List<T> ConvertToList<T>(this List<AppData> appDatas)
         where T : class, IAppData
    {
        var list = new List<T>();

        foreach (var appData in appDatas)
        {
            var model = ConvertTo<T>(appData.FiledInfos);
            if (model != null)
            {
                model.AppDataId = appData.AppDataId;
                list.Add(model);
            }

        }
        return list;
    }

    /// <summary>
    /// 将轻流数据中的字段转换成模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="appData"></param>
    /// <returns></returns>
    public static T? ConvertTo<T>(this AppData appData)
         where T : class, IAppData
    {
        var model = ConvertTo<T>(appData.FiledInfos);
        if (model != null)
        {
            model.AppDataId = appData.AppDataId;
        }
        return model;
    }


    /// <summary>
    /// 将轻流数据中的字段转换成模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableInfo"></param>
    /// <returns></returns>
    public static T? ConvertTo<T>(this TableInfo tableInfo)
         where T : class
    {
        return tableInfo.Select(q => new TitleValue(q.QueTitle, q.Values?.FirstOrDefault()?.Value ?? ""))
            .ConvertToTitleValueCollection()
            .ConvertTo<T>();
    }

    /// <summary>
    /// 将轻流数据中的字段转换成模型列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableInfos"></param>
    /// <returns></returns>
    public static List<T> ConvertToList<T>(this List<TableInfo> tableInfos)
         where T : class, new()
    {
        var list = new List<T>();

        foreach (var tableInfo in tableInfos)
        {
            var model = tableInfo.ConvertTo<T>();
            if (model != null)
                list.Add(model);
        }
        return list;
    }

}
