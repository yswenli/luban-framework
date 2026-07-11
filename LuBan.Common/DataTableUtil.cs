/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： DataTableUtil
*版本号： V1.0.0.0
*唯一标识：418c7370-3967-4960-9622-3fcc3a9a4600
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 17:49:34
*描述：datatable工具类
*
*=====================================================================
*修改标记
*修改时间：2022/6/21 17:49:34
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：datatable工具类
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// datatable工具类
/// </summary>
public static class DataTableUtil
{
    /// <summary>
    /// 将实体列表转换成datatable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="convertNames">指定转换的列名</param>
    /// <returns></returns>
    public static DataTable ToDataTable<T>(this IEnumerable<T> list, params NamePair[] convertNames) where T : class, new()
    {
        return ToDataTable(list, convertNames?.ToList() ?? null);
    }

    /// <summary>
    /// 将实体列表转换成datatable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="convertNames">指定转换的列名</param>
    /// <returns></returns>
    public static DataTable ToDataTable<T>(this IEnumerable<T> list, IEnumerable<NamePair>? convertNames) where T : class, new()
    {
        DataTable dt = new DataTable();

        PropertyInfo[] properties = typeof(T).GetProperties();

        if (convertNames == null || !convertNames.Any())
        {
            foreach (PropertyInfo pi in properties)
            {
                if (!pi.CanRead) continue;

                if (pi.PropertyType.IsNullable())
                {
                    var propType = Nullable.GetUnderlyingType(pi.PropertyType);
                    if (propType != null)
                        dt.Columns.Add(pi.Name, propType);
                }
                else
                {
                    dt.Columns.Add(pi.Name, pi.PropertyType);
                }
            }

            foreach (T item in list)
            {
                if (item == null) continue;

                DataRow newRow = dt.NewRow();

                foreach (PropertyInfo pi in properties)
                {
                    if (!pi.CanRead) continue;

                    if (pi.PropertyType.IsNullable())
                    {
                        var propType = Nullable.GetUnderlyingType(pi.PropertyType);

                        var pval = ReflectionUtil.GetPropertyValue(item, pi);

                        if (pval != null)
                        {
                            if (propType != null)
                            {
                                var value = Convert.ChangeType(pval, propType);
                                newRow[pi.Name] = value;
                            }

                        }
                    }
                    else
                    {
                        newRow[pi.Name] = ReflectionUtil.GetPropertyValue(item, pi);
                    }
                }
                dt.Rows.Add(newRow);
            }
        }
        else
        {
            var namePairCollection = new NamePairCollection(convertNames);

            foreach (var item in namePairCollection)
            {
                var pi = properties.Where(b => b.Name == item.SourceName).FirstOrDefault();
                if (pi != null)
                {
                    if (!pi.CanRead) continue;
                }
            }

            foreach (var namePair in namePairCollection)
            {
                var pi = properties.Where(q => q.CanRead
                && q.Name.Equals(namePair.SourceName, true)
                && namePair.TargetName.IsNotNullOrEmpty()).FirstOrDefault();
                if (pi == null) continue;
                if (pi.PropertyType.IsNullable())
                {
                    var propType = Nullable.GetUnderlyingType(pi.PropertyType);
                    if (propType != null)
                        dt.Columns.Add(namePair.TargetName, propType);
                }
                else
                {
                    dt.Columns.Add(namePair.TargetName, pi.PropertyType);
                }
            }

            foreach (T item in list)
            {
                if (item == null) continue;

                DataRow newRow = dt.NewRow();

                foreach (PropertyInfo pi in properties)
                {
                    if (!pi.CanRead) continue;

                    var targetName = namePairCollection.GetTargetName(pi.Name);

                    if (string.IsNullOrEmpty(targetName))
                    {
                        continue;
                    }

                    if (pi.PropertyType.IsNullable())
                    {
                        var propType = Nullable.GetUnderlyingType(pi.PropertyType);

                        var pval = ReflectionUtil.GetPropertyValue(item, pi);

                        if (pval != null && propType != null)
                        {
                            var value = Convert.ChangeType(pval, propType);

                            newRow[targetName] = value;
                        }
                    }
                    else
                    {
                        newRow[targetName] = ReflectionUtil.GetPropertyValue(item, pi);
                    }
                }
                dt.Rows.Add(newRow);
            }

        }

        return dt;
    }

    /// <summary>
    /// 将DataTable转换成实体列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dataTable"></param>
    /// <param name="convertNames">指定转换的列名</param>
    /// <returns></returns>
    public static List<T> ToList<T>(this DataTable dataTable, IEnumerable<NamePair>? convertNames = null) where T : class, new()
    {
        List<T> ts = [];

        string tempName = "";

        if (convertNames == null || !convertNames.Any())
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                if (dr == null || dr.HasErrors) continue;

                PropertyInfo[] properties = ReflectionUtil.GetPropertities<T>();

                if (properties == null || properties.Length < 1) continue;

                var t = Activator.CreateInstance<T>();

                foreach (PropertyInfo pi in properties)
                {
                    tempName = pi.Name;

                    if (dataTable.Columns.Contains(tempName))
                    {
                        if (!pi.CanWrite) continue;

                        object value = dr[tempName];

                        if (value != DBNull.Value && value != null)

                            ReflectionUtil.SetPropertyValue(t, pi, value);
                    }
                }
                ts.Add(t);
            }
        }
        else
        {
            var namePairCollection = new NamePairCollection(convertNames);

            foreach (DataRow dr in dataTable.Rows)
            {
                if (dr == null || dr.HasErrors || dr.ItemArray.Length < 1) continue;

                T t = new T();

                PropertyInfo[] propertys = t.GetType().GetProperties();

                foreach (DataColumn column in dataTable.Columns)
                {
                    var targetName = namePairCollection.GetTargetName(column.ColumnName);

                    if (!string.IsNullOrEmpty(targetName))
                    {
                        var property = propertys.Where(b => b.Name == targetName).FirstOrDefault();

                        if (property == null || dr.IsNull("tempName")) continue;

                        object value = dr[tempName];

                        if (value != DBNull.Value && value != null)

                            ReflectionUtil.SetPropertyValue(t, property, value);
                    }
                }

                ts.Add(t);
            }
        }

        return ts;
    }

    /// <summary> 
    /// 合并datatable,
    /// 将两个列不同(结构不同)的DataTable合并成一个新的DataTable 
    /// </summary> 
    /// <param name="dataTable1">表1</param> 
    /// <param name="dataTable2">表2</param> 
    /// <param name="tableName">合并后新的表名</param> 
    /// <returns>合并后的新表</returns> 
    public static DataTable MingleDataTable(DataTable dataTable1, DataTable dataTable2, string tableName)
    {
        DataTable newDataTable = new DataTable();
        if (dataTable1.Rows.Count > dataTable2.Rows.Count)
        {
            newDataTable = MingleData(dataTable1, dataTable2);
        }
        else
        {
            newDataTable = MingleData(dataTable2, dataTable1);
        }

        newDataTable.TableName = tableName; //设置DT的名字 
        return newDataTable;
    }

    private static DataTable MingleData(DataTable dt1, DataTable dt2)
    {
        //克隆DataTable1的结构
        DataTable newDataTable = dt1.Clone();
        for (int i = 0; i < dt2.Columns.Count; i++)
        {
            //再向新表中加入DataTable2的列结构
            newDataTable.Columns.Add(dt2.Columns[i].ColumnName);
        }
        object[] obj = new object[newDataTable.Columns.Count];
        //添加DataTable1的数据
        for (int i = 0; i < dt1.Rows.Count; i++)
        {
            dt1.Rows[i].ItemArray.CopyTo(obj, 0);
            newDataTable.Rows.Add(obj);
        }
        for (int i = 0; i < dt2.Rows.Count; i++)
        {
            for (int j = 0; j < dt2.Columns.Count; j++)
            {
                newDataTable.Rows[i][j + dt1.Columns.Count] = dt2.Rows[i][j].ToString();
            }
        }
        return newDataTable;
    }

    /// <summary>
    /// 合并datatable,
    /// 将两个结构相同DataTable合并成一个新的DataTable 
    /// </summary>
    /// <param name="dataTable1"></param>
    /// <param name="dataTable2"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public static DataTable MergeDataTable(DataTable dataTable1, DataTable dataTable2, string tableName)
    {
        //克隆DataTable1的结构
        DataTable newDataTable = dataTable1.Clone();
        object[] obj = new object[newDataTable.Columns.Count];
        //添加DataTable1的数据
        for (int i = 0; i < dataTable1.Rows.Count; i++)
        {
            dataTable1.Rows[i].ItemArray.CopyTo(obj, 0);
            newDataTable.Rows.Add(obj);
        }
        //添加DataTable2的数据
        for (int i = 0; i < dataTable2.Rows.Count; i++)
        {
            dataTable2.Rows[i].ItemArray.CopyTo(obj, 0);
            newDataTable.Rows.Add(obj);
        }
        newDataTable.TableName = tableName;
        return newDataTable;
    }

    /// <summary>
    /// 将DataTable转换成Dictionary
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public static string ToJson(this DataTable table)
    {
        if (table == null || table.Rows.Count < 1) return "[]";

        var jsonBuilder = new StringPlus();

        if (table.Rows.Count > 0)
        {
            jsonBuilder.Append("[");
            for (int i = 0; i < table.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    var val = table.Rows[i][j]?.ToString() ?? "";

                    if (table.Columns[j].DataType.Name == "DateTime" || table.Columns[j].DataType.Name == "MySqlDateTime")
                    {
                        var rowVal = table.Rows[i][j];
                        if (rowVal != null)
                        {
                            var rowValStr = rowVal.ToString();
                            if (rowValStr.IsNotNullOrEmpty())
                                val = DateTime.Parse(rowValStr).ToString("yyyy-MM-ddTHH:mm:ss");
                        }
                    }

                    var name = table.Columns[j].ColumnName.ToString();

                    if (j < table.Columns.Count - 1)
                    {
                        jsonBuilder.Append("\"" + name + "\":" + "\"" + val + "\",");
                    }
                    else if (j == table.Columns.Count - 1)
                    {
                        jsonBuilder.Append("\"" + name + "\":" + "\"" + val + "\"");
                    }
                }
                if (i == table.Rows.Count - 1)
                {
                    jsonBuilder.Append("}");
                }
                else
                {
                    jsonBuilder.Append("},");
                }
            }
            jsonBuilder.Append("]");
        }
        return jsonBuilder.ToString();
    }

    /// <summary>
    /// 将数据表按指定字段排序
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="columnNames"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    public static DataTable? OrderBy(this DataTable dt, string columnNames, bool sort = true)
    {
        if (dt == null || dt.Rows.Count < 1) return dt;
        dt.DefaultView.Sort = $"{columnNames} {(sort ? "ASC" : "DESC")}";
        return dt.DefaultView.ToTable();
    }
}
