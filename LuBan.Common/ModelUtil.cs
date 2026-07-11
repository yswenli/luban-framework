/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： ModelUtil
*版本号： V1.0.0.0
*唯一标识：be5f0f5f-0698-4653-8219-2db0eb59ac17
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/6/2 15:39:05
*描述：
*
*=====================================================================
*修改标记
*修改时间：2021/6/2 15:39:05
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/


namespace LuBan.Common
{

    /// <summary>
    /// 实体操作类
    /// </summary>
    public static class ModelUtil
    {
        /// <summary>
        /// 转换成另外一个实体
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="convertMatchType"></param>
        /// <returns></returns>
        public static object? To(this object source, Type targetType, EnumConvertMatchType convertMatchType = EnumConvertMatchType.IgnoreCase)
        {
            if (source != null && source.GetType().IsClass)
            {
                var sourceProperties = source.GetType().GetProperties();
                if (sourceProperties == null || !sourceProperties.Any()) return null;

                var type = targetType;
                var target = Activator.CreateInstance(targetType);

                if (target == null) return null;

                var targetProperties = type.GetProperties();

                foreach (var targetProperty in targetProperties)
                {
                    PropertyInfo? sourceProperty = null;

                    switch (convertMatchType)
                    {
                        case EnumConvertMatchType.ExactlyMatch:
                            sourceProperty = sourceProperties.Where(b => b.Name == targetProperty.Name).FirstOrDefault();
                            break;
                        case EnumConvertMatchType.IgnoreCase:
                            sourceProperty = sourceProperties.Where(b => b.Name.ToLower() == targetProperty.Name.ToLower()).FirstOrDefault();
                            break;
                        case EnumConvertMatchType.Contain:
                            sourceProperty = sourceProperties.Where(b => b.Name.IndexOf(targetProperty.Name) > -1 || targetProperty.Name.IndexOf(b.Name) > -1).FirstOrDefault();
                            break;
                        case EnumConvertMatchType.ContainAndIgnoreCase:
                            sourceProperty = sourceProperties.Where(b => b.Name.IndexOf(targetProperty.Name, StringComparison.CurrentCultureIgnoreCase) > -1 || targetProperty.Name.IndexOf(b.Name, StringComparison.CurrentCultureIgnoreCase) > -1).FirstOrDefault();
                            break;
                    }

                    if (sourceProperty != null)
                    {
                        try
                        {
                            object? val = null;

                            if (sourceProperty.PropertyType == targetProperty.PropertyType)
                            {
                                val = ReflectionUtil.GetPropertyValue(source, sourceProperty);

                                if (val != null)
                                {
                                    ReflectionUtil.SetPropertyValue(target, targetProperty, val);
                                }
                            }
                            else if (sourceProperty.PropertyType.GetInterface("IEnumerable", true) != null && targetProperty.PropertyType.GetInterface("IEnumerable", true) != null)
                            {
                                val = ReflectionUtil.GetPropertyValue(source, sourceProperty);
                                if (val != null)
                                {
                                    var sType = targetProperty.PropertyType.GenericTypeArguments[0];
                                    var list = val.ToEntityList(sType, convertMatchType);
                                    targetProperty.SetValue(target, list);
                                    //ReflectionUtil.SetPropertyValue(target, targetProperty, list);
                                }
                            }
                            else
                            {
                                val = ReflectionUtil.GetPropertyValue(source, sourceProperty);

                                //不同类型转换

                                #region 日期

                                if (sourceProperty.PropertyType == typeof(DateTime))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        if (val != null)
                                        {
                                            var dt = (DateTime)val;
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, dt.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                                        }
                                    }
                                    else if (targetProperty.PropertyType == typeof(Nullable<DateTime>))
                                    {
                                        if (val != null)
                                        {
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, val);
                                        }
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(Nullable<DateTime>))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        if (val != null)
                                        {
                                            var dt = (Nullable<DateTime>)val;
                                            if (dt.HasValue)
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, dt.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                                            }
                                        }
                                    }
                                    else if (targetProperty.PropertyType == typeof(DateTime))
                                    {
                                        if (val != null)
                                        {
                                            var dt = (Nullable<DateTime>)val;
                                            if (dt.HasValue)
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, dt.Value);
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region 字符串

                                if (sourceProperty.PropertyType == typeof(string))
                                {
                                    var str = val?.ToString() ?? "";

                                    if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                    {
                                        var num = 0;

                                        if (!string.IsNullOrWhiteSpace(str))
                                        {
                                            int.TryParse(str, out num);
                                        }

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, num);

                                    }
                                    else if (targetProperty.PropertyType == typeof(long) || targetProperty.PropertyType == typeof(Nullable<long>))
                                    {
                                        long num = 0;

                                        if (!string.IsNullOrWhiteSpace(str))
                                        {
                                            long.TryParse(str, out num);
                                        }

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, num);
                                    }
                                    else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                    {
                                        short num = 0;

                                        if (!string.IsNullOrWhiteSpace(str))
                                        {
                                            short.TryParse(str, out num);
                                        }

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, num);
                                    }
                                    else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                    {
                                        byte num = 0;

                                        if (!string.IsNullOrWhiteSpace(str))
                                        {
                                            byte.TryParse(str, out num);
                                        }

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, num);
                                    }
                                    else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                    {
                                        float num = 0;

                                        if (!string.IsNullOrWhiteSpace(str))
                                        {
                                            float.TryParse(str, out num);
                                        }

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, num);
                                    }
                                    else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                    {
                                        double num = 0;

                                        if (!string.IsNullOrWhiteSpace(str))
                                        {
                                            double.TryParse(str, out num);
                                        }

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, num);
                                    }
                                    else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                    {
                                        decimal num = 0;

                                        if (!string.IsNullOrWhiteSpace(str))
                                        {
                                            decimal.TryParse(str, out num);
                                        }

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, num);
                                    }
                                    else if (targetProperty.PropertyType == typeof(bool) || targetProperty.PropertyType == typeof(Nullable<bool>))
                                    {
                                        var bVal = false;

                                        if (!string.IsNullOrWhiteSpace(str))
                                        {
                                            bool.TryParse(str, out bVal);
                                        }

                                        if (str != "0" && str != "false")
                                        {
                                            bVal = true;
                                        }

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, bVal);
                                    }
                                    else if (targetProperty.PropertyType == typeof(DateTime) || targetProperty.PropertyType == typeof(Nullable<DateTime>))
                                    {
                                        if (!string.IsNullOrWhiteSpace(str))
                                        {
                                            if (DateTime.TryParse(str, out DateTime dt))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, dt);
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region 数字

                                if (sourceProperty.PropertyType == typeof(byte))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, val?.ToString() ?? "");
                                    }
                                    else if (targetProperty.PropertyType == typeof(Nullable<byte>))
                                    {
                                        if (val != null)
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, val);
                                    }
                                    else if (targetProperty.PropertyType.IsEnum)
                                    {
                                        var eVal = Enum.Parse(targetProperty.PropertyType, val?.ToString() ?? "");

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, eVal);
                                    }
                                    else if (targetProperty.PropertyType == typeof(bool) || targetProperty.PropertyType == typeof(Nullable<bool>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, (Convert.ToByte(val)) != 0);
                                    }
                                    else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToSingle(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(val));
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(Nullable<byte>))
                                {
                                    if (val != null)
                                    {
                                        var nVal = (Nullable<byte>)val;

                                        if (nVal.HasValue)
                                        {
                                            if (targetProperty.PropertyType == typeof(string))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value.ToString());
                                            }
                                            else if (targetProperty.PropertyType == typeof(byte))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal);
                                            }
                                            else if (targetProperty.PropertyType.IsEnum)
                                            {
                                                var eVal = Enum.Parse(targetProperty.PropertyType, nVal.Value.ToString());

                                                ReflectionUtil.SetPropertyValue(target, targetProperty, eVal);
                                            }
                                            else if (targetProperty.PropertyType == typeof(bool) || targetProperty.PropertyType == typeof(Nullable<bool>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (Convert.ToByte(nVal.Value)) != 0);

                                            }
                                            else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(nVal.Value));

                                            }
                                            else if (targetProperty.PropertyType == typeof(long) || targetProperty.PropertyType == typeof(Nullable<long>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (long)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (float)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(nVal.Value));

                                            }
                                        }
                                    }
                                }

                                if (sourceProperty.PropertyType == typeof(short))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, val?.ToString() ?? "");
                                    }
                                    else if (targetProperty.PropertyType == typeof(Nullable<short>))
                                    {
                                        var sval = Convert.ToInt16(val);
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, sval);
                                    }
                                    else if (targetProperty.PropertyType.IsEnum)
                                    {
                                        var eVal = Enum.Parse(targetProperty.PropertyType, val?.ToString() ?? "");

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, eVal);
                                    }
                                    else if (targetProperty.PropertyType == typeof(bool) || targetProperty.PropertyType == typeof(Nullable<bool>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, (Convert.ToInt16(val)) != 0);
                                    }
                                    else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToSingle(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(val));
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(Nullable<short>))
                                {
                                    if (val != null)
                                    {
                                        var nVal = (Nullable<short>)val;

                                        if (nVal.HasValue)
                                        {
                                            if (targetProperty.PropertyType == typeof(string))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value.ToString());
                                            }
                                            else if (targetProperty.PropertyType == typeof(short))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType.IsEnum)
                                            {
                                                var eVal = Enum.Parse(targetProperty.PropertyType, nVal.Value.ToString());

                                                ReflectionUtil.SetPropertyValue(target, targetProperty, eVal);
                                            }
                                            else if (targetProperty.PropertyType == typeof(bool) || targetProperty.PropertyType == typeof(Nullable<bool>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (Convert.ToInt16(nVal.Value)) != 0);

                                            }
                                            else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(long) || targetProperty.PropertyType == typeof(Nullable<long>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (long)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (float)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(nVal.Value));
                                            }
                                        }
                                    }
                                }

                                if (sourceProperty.PropertyType == typeof(int))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, val?.ToString() ?? "");
                                    }
                                    else if (targetProperty.PropertyType == typeof(Nullable<int>))
                                    {
                                        if (val != null)
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, val);
                                    }
                                    else if (targetProperty.PropertyType.IsEnum)
                                    {
                                        var eVal = Enum.Parse(targetProperty.PropertyType, val?.ToString() ?? "");

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, eVal);
                                    }
                                    else if (targetProperty.PropertyType == typeof(bool) || targetProperty.PropertyType == typeof(Nullable<bool>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, (Convert.ToInt32(val)) != 0);

                                    }
                                    else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToSingle(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(val));
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(Nullable<int>))
                                {
                                    if (val != null)
                                    {
                                        var nVal = (Nullable<int>)val;

                                        if (nVal.HasValue)
                                        {
                                            if (targetProperty.PropertyType == typeof(string))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value.ToString());
                                            }
                                            else if (targetProperty.PropertyType == typeof(int))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType.IsEnum)
                                            {
                                                var eVal = Enum.Parse(targetProperty.PropertyType, nVal.Value.ToString());

                                                ReflectionUtil.SetPropertyValue(target, targetProperty, eVal);
                                            }
                                            else if (targetProperty.PropertyType == typeof(bool) || targetProperty.PropertyType == typeof(Nullable<bool>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (Convert.ToInt32(nVal.Value)) != 0);

                                            }
                                            else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(long) || targetProperty.PropertyType == typeof(Nullable<long>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (long)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (float)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(nVal.Value));
                                            }
                                        }
                                    }
                                }

                                if (sourceProperty.PropertyType == typeof(long))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, val?.ToString() ?? "");
                                    }
                                    else if (targetProperty.PropertyType == typeof(Nullable<long>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt64(val));
                                    }
                                    else if (targetProperty.PropertyType.IsEnum)
                                    {
                                        var eVal = Enum.Parse(targetProperty.PropertyType, val?.ToString() ?? "");

                                        ReflectionUtil.SetPropertyValue(target, targetProperty, eVal);
                                    }
                                    else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToSingle(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(val));
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(Nullable<long>))
                                {
                                    if (val != null)
                                    {
                                        var nVal = (Nullable<long>)val;

                                        if (nVal.HasValue)
                                        {
                                            if (targetProperty.PropertyType == typeof(string))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value.ToString());
                                            }
                                            else if (targetProperty.PropertyType == typeof(long))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType.IsEnum)
                                            {
                                                var eVal = Enum.Parse(targetProperty.PropertyType, nVal.Value.ToString());

                                                ReflectionUtil.SetPropertyValue(target, targetProperty, eVal);
                                            }
                                            else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(nVal.Value));
                                            }
                                        }
                                    }
                                }


                                if (sourceProperty.PropertyType == typeof(float))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, val?.ToString() ?? "");
                                    }
                                    else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(long) || targetProperty.PropertyType == typeof(Nullable<long>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt64(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(Nullable<float>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToSingle(val));
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(Nullable<float>))
                                {
                                    if (val != null)
                                    {
                                        var nVal = (Nullable<float>)val;

                                        if (nVal.HasValue)
                                        {
                                            if (targetProperty.PropertyType == typeof(string))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value.ToString());
                                            }
                                            else if (targetProperty.PropertyType == typeof(float))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(long) || targetProperty.PropertyType == typeof(Nullable<long>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (long)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(nVal.Value));
                                            }
                                        }
                                    }
                                }

                                if (sourceProperty.PropertyType == typeof(double))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, val?.ToString() ?? "");
                                    }
                                    else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(long) || targetProperty.PropertyType == typeof(Nullable<long>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt64(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToSingle(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(Nullable<double>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(val));
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(Nullable<double>))
                                {
                                    if (val != null)
                                    {
                                        var nVal = (Nullable<double>)val;

                                        if (nVal.HasValue)
                                        {
                                            if (targetProperty.PropertyType == typeof(string))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value.ToString());
                                            }
                                            else if (targetProperty.PropertyType == typeof(decimal) || targetProperty.PropertyType == typeof(Nullable<decimal>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(long) || targetProperty.PropertyType == typeof(Nullable<long>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (long)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (float)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(double))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(nVal.Value));
                                            }
                                        }
                                    }
                                }

                                if (sourceProperty.PropertyType == typeof(decimal))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, val?.ToString() ?? "");
                                    }
                                    else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(long) || targetProperty.PropertyType == typeof(Nullable<long>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt64(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToSingle(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(val));
                                    }
                                    else if (targetProperty.PropertyType == typeof(Nullable<decimal>))
                                    {
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDecimal(val));
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(Nullable<decimal>))
                                {
                                    if (val != null)
                                    {
                                        var nVal = (Nullable<decimal>)val;

                                        if (nVal.HasValue)
                                        {
                                            if (targetProperty.PropertyType == typeof(string))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value.ToString());
                                            }
                                            else if (targetProperty.PropertyType == typeof(decimal))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(byte) || targetProperty.PropertyType == typeof(Nullable<byte>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToByte(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(short) || targetProperty.PropertyType == typeof(Nullable<short>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt16(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(int) || targetProperty.PropertyType == typeof(Nullable<int>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToInt32(nVal.Value));
                                            }
                                            else if (targetProperty.PropertyType == typeof(long) || targetProperty.PropertyType == typeof(Nullable<long>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (long)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(float) || targetProperty.PropertyType == typeof(Nullable<float>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, (float)nVal.Value);
                                            }
                                            else if (targetProperty.PropertyType == typeof(double) || targetProperty.PropertyType == typeof(Nullable<double>))
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, Convert.ToDouble(nVal.Value));
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region 枚举

                                if (sourceProperty.PropertyType.IsEnum)
                                {
                                    if (sourceProperty.PropertyType == typeof(string))
                                    {
                                        if (val != null)
                                        {
                                            var str = Enum.GetName(val.GetType(), val);
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, str ?? "");
                                        }
                                    }
                                    else if (sourceProperty.PropertyType == typeof(byte))
                                    {
                                        var nVal = Convert.ToByte(val);
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, nVal);
                                    }
                                    else if (sourceProperty.PropertyType == typeof(short))
                                    {
                                        var nVal = Convert.ToInt16(val);
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, nVal);
                                    }
                                    else if (sourceProperty.PropertyType == typeof(int))
                                    {
                                        var nVal = Convert.ToInt32(val);
                                        ReflectionUtil.SetPropertyValue(target, targetProperty, nVal);
                                    }
                                }
                                #endregion

                                #region bool

                                if (sourceProperty.PropertyType == typeof(bool))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        if (val != null)
                                        {
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, val?.ToString() ?? "");
                                        }
                                    }
                                    else if (targetProperty.PropertyType == typeof(Nullable<bool>))
                                    {
                                        if (val != null)
                                        {
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, val);
                                        }
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(Nullable<bool>))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        if (val != null)
                                        {
                                            var tval = (Nullable<bool>)val;
                                            if (tval.HasValue)
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, tval?.ToString() ?? "");
                                            }
                                        }
                                    }
                                    else if (targetProperty.PropertyType == typeof(bool))
                                    {
                                        if (val != null)
                                        {
                                            var tavl = (Nullable<bool>)val;
                                            if (tavl.HasValue)
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, tavl.Value);
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region guid

                                if (sourceProperty.PropertyType == typeof(Guid))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        if (val != null)
                                        {
                                            var guid = (Guid)val;
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, guid.ToString("N"));
                                        }
                                        else
                                        {
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, Guid.Empty.ToString("N"));
                                        }
                                    }
                                    else if (targetProperty.PropertyType == typeof(Nullable<Guid>))
                                    {
                                        if (val != null)
                                        {
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, val);
                                        }
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(Nullable<Guid>))
                                {
                                    if (targetProperty.PropertyType == typeof(string))
                                    {
                                        if (val != null)
                                        {
                                            var guid = (Nullable<Guid>)val;
                                            if (guid.HasValue)
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, guid.Value.ToString("N"));
                                            }
                                        }
                                    }
                                    else if (targetProperty.PropertyType == typeof(Guid))
                                    {
                                        if (val != null)
                                        {
                                            var guid = (Nullable<Guid>)val;
                                            if (guid.HasValue)
                                            {
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, guid.Value);
                                            }
                                        }
                                    }
                                }
                                else if (sourceProperty.PropertyType == typeof(string))
                                {
                                    var str = val?.ToString() ?? "";

                                    if (targetProperty.PropertyType == typeof(Nullable<Guid>) || targetProperty.PropertyType == typeof(Guid))
                                    {
                                        if (string.IsNullOrEmpty(str))
                                        {
                                            ReflectionUtil.SetPropertyValue(target, targetProperty, Guid.Empty);
                                        }
                                        else
                                        {
                                            if (Guid.TryParse(str, out Guid sval))
                                                ReflectionUtil.SetPropertyValue(target, targetProperty, sval);
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"modelutil 中发生异常，指定的类不是公开的或者不支持的属性类型，sourceProperty:{sourceProperty.Name} {sourceProperty.PropertyType}, targetProperty:{targetProperty.Name} {targetProperty.PropertyType} ,err:{ex.Message}");
                        }
                    }
                }
                return target;
            }
            return null;
        }


        /// <summary>
        /// 转换成另外一个实体列表
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="convertMatchType"></param>
        /// <returns></returns>
        public static IList? ToEntityList(this object source, Type targetType, EnumConvertMatchType convertMatchType = EnumConvertMatchType.IgnoreCase)
        {
            if (source != null)
            {
                if (source.GetType().GetInterface("IList", true) != null)
                {
                    var list = (System.Collections.IEnumerable)source;

                    var result = targetType.CreateList();

                    foreach (var item in list)
                    {
                        result.Add(item.To(targetType, convertMatchType));
                    }

                    return result;
                }
            }
            return null;
        }


        /// <summary>
        /// 转换成另外一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="convertMatchType"></param>
        /// <returns></returns>
        public static T? To<T>(this object source, EnumConvertMatchType convertMatchType = EnumConvertMatchType.IgnoreCase) where T : class
        {
            return source.To(typeof(T), convertMatchType) as T;
        }



        /// <summary>
        /// 转换成另外一个实体列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="convertMatchType"></param>
        /// <returns></returns>
        public static List<T>? ToEntityList<T>(this object source, EnumConvertMatchType convertMatchType = EnumConvertMatchType.IgnoreCase) where T : class
        {
            if (source != null)
            {
                if (source.GetType().GetInterface("IEnumerable", true) != null)
                {
                    var list = (System.Collections.IEnumerable)source;

                    var result = new List<T>();

                    foreach (var item in list)
                    {
                        var val = item.To<T>(convertMatchType);
                        if (val != null)
                            result.Add(val);
                    }

                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// 转换成另外一个实体列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="convertMatchType"></param>
        /// <returns></returns>
        public static List<T>? ToList<T>(this IEnumerable<object> source, EnumConvertMatchType convertMatchType = EnumConvertMatchType.IgnoreCase) where T : class
        {
            if (source == null || !source.Any()) return null;
            List<T> result = new List<T>();
            foreach (var item in source)
            {
                if (item != null)
                {
                    var val = item.To<T>(convertMatchType);
                    if (val != null)
                        result.Add(val);
                }
            }
            return result;
        }



        /// <summary>
        /// 从obj中获取url query
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ignores"></param>
        /// <returns></returns>
        public static string GetUrlQueryTextByObj(this object obj, params string[] ignores)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            var sb = new StringPlus();

            var type = obj.GetType();

            if ((type.IsClass && type.Name != "String" && !type.IsGenericType) || obj.IsAnonymousType())
            {
                var properties = type.GetProperties();

                foreach (var property in properties)
                {
                    var propertiyName = property.GetJsonPropertityName();

                    if (propertiyName.IsNullOrEmpty()) continue;

                    var val = ReflectionUtil.GetPropertyValue(obj, property);

                    if (val == null) continue;

                    if (ignores != null && ignores.Length > 0 && ignores.Contains(propertiyName, DefaultEqualityComparer<string>.Default)) continue;


                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        if (property.PropertyType.IsGenericType)
                        {
                            if (property.PropertyType.IsNullable())
                            {
                                var propType = Nullable.GetUnderlyingType(property.PropertyType);
                                if (propType == null) continue;

                                if (propType.IsClass)
                                {
                                    if (propType != typeof(string))
                                    {
                                        sb.Append($"&{propertiyName}={GetUrlQueryTextByObj(val, ignores!).UrlEncode()}");
                                    }
                                    else
                                    {
                                        sb.Append($"&{propertiyName}={(val?.ToString() ?? "").UrlEncode()}");
                                    }
                                }
                                else
                                {
                                    if (val is DateTime dt)
                                    {
                                        sb.Append($"&{propertiyName}={(dt.ToString("yyyy-MM-dd HH:mm:ss")).UrlEncode()}");
                                    }
                                    else
                                    {
                                        sb.Append($"&{propertiyName}={(val?.ToString() ?? "").UrlEncode()}");
                                    }
                                }
                            }
                            else if (property.PropertyType.IsList())
                            {
                                StringBuilder lb = new StringPlus();

                                var list = (IEnumerable)val;

                                foreach (var vitem in list)
                                {
                                    if (vitem != null)
                                    {
                                        var vtype = vitem.GetType();
                                        if (vtype.IsClass)
                                        {
                                            if (vtype != typeof(string))
                                            {
                                                lb.Append($"{GetUrlQueryTextByObj(vitem, ignores!).UrlEncode()}%2C");
                                            }
                                            else
                                            {
                                                lb.Append($"{(vitem.ToString() ?? "").UrlEncode()}%2C");
                                            }
                                        }
                                        else
                                        {
                                            lb.Append($"{(vitem?.ToString() ?? "").UrlEncode()}%2C");
                                        }
                                    }
                                }
                                if (lb.Length > 3)
                                {
                                    sb.Append($"&{propertiyName}={lb.ToString().Substring(0, lb.Length - 3)}");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (val is DateTime dt)
                        {
                            sb.Append($"&{propertiyName}={(dt.ToString("yyyy-MM-dd HH:mm:ss")).UrlEncode()}");
                        }
                        else
                        {
                            sb.Append($"&{propertiyName}={(val.ToString() ?? "").UrlEncode()}");
                        }
                    }
                }
            }

            var queryStr = sb.ToString();

            if (queryStr.StartsWith("&"))
            {
                queryStr = "?" + queryStr.Substring(1);
            }

            return queryStr;
        }

        /// <summary>
        /// 从实体中获取url query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="ignores"></param>
        /// <returns></returns>
        public static string GetUrlQueryText<T>(this T t, params string[] ignores) where T : class, new()
        {
            return GetUrlQueryTextByObj(t, ignores);
        }

        /// <summary>
        /// 从实体中获取url query dic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="ignores"></param>
        /// <returns></returns>
        public static SortedDictionary<string, string> GetQueryDic<T>(this T t, params string[] ignores) where T : class, new()
        {
            if (t == null)
            {
                return [];
            }

            SortedDictionary<string, string> result = new SortedDictionary<string, string>();

            var type = t.GetType();

            if (type.IsClass && type.Name != "String" && !type.IsGenericType)
            {
                var properties = type.GetProperties();

                foreach (var property in properties)
                {
                    var val = ReflectionUtil.GetPropertyValue(t, property);

                    if (val == null) continue;

                    if (ignores != null && ignores.Length > 0 && ignores.Contains(property.Name, DefaultEqualityComparer<string>.Default)) continue;

                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        if (property.PropertyType.IsGenericType)
                        {
                            if (property.PropertyType.IsNullable())
                            {
                                var propType = Nullable.GetUnderlyingType(property.PropertyType);
                                if (propType == null) continue;

                                if (propType.IsClass && propType != typeof(string))
                                {
                                    result.TryAdd(property.Name, GetUrlQueryTextByObj(val, ignores!));
                                }
                                else
                                {
                                    result.TryAdd(property.Name, val?.ToString() ?? "");
                                }
                            }
                            else if (property.PropertyType.IsList())
                            {
                                StringBuilder lb = new StringPlus();

                                var list = (IEnumerable)val;

                                foreach (var vitem in list)
                                {
                                    if (vitem != null)
                                    {
                                        var vtype = vitem.GetType();
                                        if (vtype.IsClass && vtype != typeof(string))
                                        {
                                            lb.Append($"{GetUrlQueryTextByObj(vitem, ignores!)}%2C");
                                        }
                                        else
                                        {
                                            lb.Append($"{vitem}%2C");
                                        }
                                    }
                                }

                                if (lb.Length > 3)
                                {
                                    result.TryAdd(property.Name, lb.ToString().Substring(0, lb.Length - 3));
                                }
                            }
                        }
                    }
                    else
                    {
                        result.TryAdd(property.Name, val?.ToString() ?? "");
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 将简单实体转换成Dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="ignores"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary<T>(this T t, params string[] ignores) where T : class, new()
        {
            var retsult = new Dictionary<string, object>();

            var properties = ReflectionUtil.GetPropertities<T>();

            if (ignores == null)
            {
                foreach (var property in properties)
                {
                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        if (property.PropertyType.IsNullable())
                        {
                            var ptype = Nullable.GetUnderlyingType(property.PropertyType);
                            if (ptype != null)
                            {
                                var val = Convert.ChangeType(ReflectionUtil.GetPropertyValue(t, property), ptype);
                                if (val != null)
                                    retsult.TryAdd(property.Name, val);
                            }
                        }
                        else
                        {
                            var val = ReflectionUtil.GetPropertyValue(t, property);
                            if (val != null)
                                retsult.TryAdd(property.Name, val);
                        }
                    }
                    else
                    {
                        var val = ReflectionUtil.GetPropertyValue(t, property);
                        if (val != null)
                            retsult.TryAdd(property.Name, val);
                    }
                }
            }
            else
            {
                foreach (var property in properties)
                {
                    if (ignores.Contains(property.Name, DefaultEqualityComparer<string>.Default))
                    {
                        continue;
                    }

                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        if (property.PropertyType.IsNullable())
                        {
                            var ptype = Nullable.GetUnderlyingType(property.PropertyType);
                            if (ptype != null)
                            {
                                var val = Convert.ChangeType(ReflectionUtil.GetPropertyValue(t, property), ptype);
                                if (val != null)
                                    retsult.TryAdd(property.Name, val);
                            }
                        }
                        else
                        {
                            var val = ReflectionUtil.GetPropertyValue(t, property);
                            if (val != null)
                                retsult.TryAdd(property.Name, val);
                        }
                    }
                    else
                    {
                        var val = ReflectionUtil.GetPropertyValue(t, property);
                        if (val != null)
                            retsult.TryAdd(property.Name, val);
                    }
                }
            }

            return retsult;
        }

        /// <summary>
        /// 将简单实体转换成SortedDictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="ignores"></param>
        /// <returns></returns>
        public static SortedDictionary<string, object> ToSortedDictionary<T>(this T t, params string[] ignores) where T : class, new()
        {
            var dic = t.ToDictionary(ignores);

            SortedDictionary<string, object> _sdic = new SortedDictionary<string, object>();

            foreach (var item in dic)
            {
                _sdic.TryAdd(item.Key, item.Value);
            }

            dic.Clear();

            return _sdic;
        }

        /// <summary>
        /// 将实体转换成url query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="ignores"></param>
        /// <returns></returns>
        public static string ToUrlQueryText<T>(this T t, params string[] ignores) where T : class, new()
        {
            return t.GetUrlQueryText(ignores);
        }

        /// <summary>
        /// 获取全部实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="fromPage"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetAllAsync<T>(Func<int, int, Task<List<T>>> func, int fromPage = 1, int size = 100)
        {
            var page = fromPage;
            var result = new List<T>();
            while (true)
            {
                try
                {
                    var list = await func.Invoke(page, size);
                    if (list != null && list.Count > 0)
                    {
                        result.AddRange(list);
                        if (list.Count < size)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                finally
                {
                    page++;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取全部实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="datetime"></param>
        /// <param name="fromPage"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetAllAsync<T>(Func<string, int, int, Task<List<T>>> func, string datetime, int fromPage = 1, int size = 100)
        {
            var page = fromPage;
            var result = new List<T>();
            while (true)
            {
                try
                {
                    var list = await func.Invoke(datetime, page, size);
                    if (list != null && list.Count > 0)
                    {
                        result.AddRange(list);
                        if (list.Count < size)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                finally
                {
                    page++;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取全部实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="fromPage"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetAllAsync<T>(Func<long, long, int, int, Task<List<T>>> func, long from, long to, int fromPage = 1, int size = 100)
        {
            var page = fromPage;
            var result = new List<T>();
            while (true)
            {
                try
                {
                    var list = await func.Invoke(from, to, page, size);
                    if (list != null && list.Count > 0)
                    {
                        result.AddRange(list);
                        if (list.Count < size)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                finally
                {
                    page++;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取全部实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="datetime"></param>
        /// <param name="fromPage"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetAllAsync<T>(Func<string, long, long, int, int, Task<List<T>>> func, string datetime, long from, long to, int fromPage = 1, int size = 100)
        {
            var page = fromPage;

            var result = new List<T>();

            while (true)
            {
                var list = await func.Invoke(datetime, from, to, page, size);

                if (list != null && list.Count > 0)
                {
                    result.AddRange(list);

                    page++;

                    if (list.Count < size)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

            }
            return result;
        }
        /// <summary>
        /// 获取全部实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="openid"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="datetime"></param>
        /// <param name="fromPage"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetAllAsync<T>(Func<string, long, long, string, int, int, Task<List<T>>> func, string openid, long from, long to, string datetime, int fromPage = 1, int size = 100)
        {
            var page = fromPage;
            var result = new List<T>();
            while (true)
            {
                try
                {
                    var list = await func.Invoke(openid, from, to, datetime, page, size);
                    if (list != null && list.Count > 0)
                    {
                        result.AddRange(list);
                        if (list.Count < size)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                finally
                {
                    page++;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="refreshToken"></param>
        /// <param name="marketPlaceID"></param>
        /// <param name="filter"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static List<T> GetAllListByOffset<T>(Func<string, string, string, int, int, List<T>> func, string refreshToken, string marketPlaceID, string filter = "", int offset = 0, int size = 100)
        {
            var result = new List<T>();
            while (true)
            {
                var list = func.Invoke(refreshToken, marketPlaceID, filter, offset, size);
                if (list != null && list.Count > 0)
                {
                    offset += list.Count;
                    result.AddRange(list);
                    if (list.Count < size)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return result;
        }


        /// <summary>
        /// 将claim转为实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static T? ToModel<T>(this IEnumerable<Claim> claims) where T : class, new()
        {
            if (claims == null || !claims.Any()) return default;

            var t = new T();

            var type = typeof(T);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var claim = claims.Where(q => q.Type.Equals(property.Name, true)).FirstOrDefault();

                if (claim != null && claim.Value != null)
                {
                    if (property.PropertyType.Name.Equals("List`1"))
                    {
                        var arr = claim.Value.Split(",").ToList();
                        property.SetValue(t, arr);
                    }
                    else
                    {
                        property.SetValue(t, claim.Value);
                    }
                }
            }

            return t;
        }


        /// <summary>
        /// 获取属性名称，兼容从System.Text.JsonJsonPropertyNameAttribute或newton.json.JsonPropertyAttribute特性中中获取属性名称
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static string GetJsonPropertityName(this PropertyInfo propertyInfo)
        {
            //默认属性名称
            var propertityName = propertyInfo.Name.ToLower();
            var attrs = propertyInfo.GetCustomAttributes(true);
            if (attrs == null || attrs.Length < 1) return propertityName;
            foreach (var attr in attrs)
            {
                if (attr is JsonIgnoreAttribute || attr is System.Text.Json.Serialization.JsonIgnoreAttribute) return string.Empty;
                if (attr is JsonPropertyAttribute jp)
                {
                    return jp.PropertyName ?? "";
                }
                if (attr is System.Text.Json.Serialization.JsonPropertyNameAttribute jpn)
                {
                    return jpn.Name;
                }
            }
            return propertityName;
        }

    }


    /// <summary>
    /// 自定义相等比较类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultEqualityComparer<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// 判断对象是否相等
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(T? x, T? y)
        {
            var type = typeof(T);

            if (type.IsClass && type != typeof(string))
            {
                if (type.IsGenericType)
                {
                    if (type.IsNullable())
                    {
                        var t = Nullable.GetUnderlyingType(type);
                        if (t == null) return false;
                        var v1 = Convert.ChangeType(x, t);
                        var v2 = Convert.ChangeType(y, t);

                        if (t.IsClass && t != typeof(string))
                        {
                            return (x?.GetHashCode() ?? null) == (y?.GetHashCode() ?? null);
                        }
                        else if (type == typeof(string))
                        {
                            var x1 = x as string;
                            var y1 = y as string;
                            return x1.IsNotNullOrEmpty() && x1.Equals(y1, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            return x != null && x.Equals(y);
                        }
                    }
                    else
                    {
                        return (x?.GetHashCode() ?? null) == (y?.GetHashCode() ?? null);
                    }
                }
                else
                {
                    return (x?.GetHashCode() ?? null) == (y?.GetHashCode() ?? null);
                }
            }
            else if (type == typeof(string))
            {
                var x1 = x as string;
                var y1 = y as string;
                if (x1 == null) return false;
                return x1.Equals(y1, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                if (x == null) return false;
                return x.Equals(y);
            }

        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode([DisallowNull] T obj)
        {
            return obj.GetHashCode();
        }

        #region static

        /// <summary>
        /// 返回默认的相等对比实例
        /// </summary>
        public static DefaultEqualityComparer<T> Default
        {
            get
            {
                return new DefaultEqualityComparer<T>();
            }
        }

        #endregion
    }
}
