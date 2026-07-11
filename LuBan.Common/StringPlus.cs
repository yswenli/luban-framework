/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： StringPlus
*版本号： V1.0.0.0
*唯一标识：d6777bf1-cbb7-448f-bf4d-1f6ba3aadea7
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/11/1 10:27:03
*描述：字符串处理类
*
*=================================================
*修改标记
*修改时间：2022/11/1 10:27:03
*修改人： yswenli
*版本号： V1.0.0.0
*描述：字符串处理类
*
*****************************************************************************/
namespace System;

/// <summary>
/// 字符串处理类
/// </summary>
public class StringPlus : IDisposable
{
    private readonly StringBuilder _sb;

    /// <summary>
    /// Length
    /// </summary>
    public int Length
    {
        get
        {
            return _sb.Length;
        }
    }

    /// <summary>
    /// 字符串处理类
    /// </summary>
    public StringPlus()
    {
        _sb = new StringBuilder();
    }

    /// <summary>
    /// 字符串处理类
    /// </summary>
    /// <param name="str"></param>
    public StringPlus(string str) : this()
    {
        _sb.Append(str);
    }

    /// <summary>
    /// Append
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public void Append(dynamic text)
    {
        if (text == null) return;
        _sb.Append(text);
    }

    /// <summary>
    /// Append
    /// </summary>
    /// <param name="text"></param>
    /// <param name="repeatCount"></param>
    public void Append(dynamic text, int repeatCount)
    {
        if (text == null) return;
        _sb.Append(text, repeatCount);
    }

    /// <summary>
    /// 添加空行
    /// </summary>
    /// <returns></returns>
    public void AppendLine()
    {
        _sb.Append("\r\n");
    }

    /// <summary>
    /// 添加一行字符串
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public void AppendLine(dynamic text)
    {
        if (text == null) return;
        _sb.Append(text + "\r\n");
    }

    /// <summary>
    /// 添加若干个空格符后的文本内容
    /// </summary>
    /// <param name="spaceNum"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public void AppendSpace(int spaceNum, dynamic text)
    {
        if (text == null) return;
        for (int i = 0; i < spaceNum; i++)
        {
            _sb.Append("\t");  //制表符
        }
        _sb.Append(text);
    }

    /// <summary>
    /// 添加若干个空格符后的文本，并换行
    /// </summary>
    /// <param name="spaceNum">空格数</param>
    /// <param name="text"></param>
    /// <returns></returns>
    public void AppendSpaceLine(int spaceNum, dynamic text)
    {
        if (text == null) return;
        for (int i = 0; i < spaceNum; i++)
        {
            _sb.Append("\t");  //制表符
        }
        _sb.Append(text);
        _sb.Append("\r\n");
    }

    /// <summary>
    /// 按指定格式追加字符串
    /// </summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void AppendFormat(string format, params object[] args)
    {
        _sb.AppendFormat(format, args);
    }

    /// <summary>
    /// 删除末尾指定字符串
    /// </summary>
    /// <param name="strchar"></param>
    public void DelLastChar(string strchar)
    {
        string str = _sb.ToString();
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(strchar)) return;
        int index = str.LastIndexOf(strchar);
        if (index > -1)
        {
            _sb.Remove(index, strchar.Length);
        }
    }

    /// <summary>
    /// 删除最后一个逗号
    /// </summary>
    public void DelLastComma()
    {
        DelLastChar(",");
    }

    /// <summary>
    /// Remove
    /// </summary>
    /// <param name="start"></param>
    /// <param name="num"></param>
    public void Remove(int start, int num)
    {
        _sb.Remove(start, num);
    }

    /// <summary>
    /// RemoveFirst
    /// </summary>
    /// <param name="size"></param>
    public void RemoveFirst(int size = 1)
    {
        if (size < 1) size = 1;

        if (_sb.Length >= size)
            _sb.Remove(0, size);
    }
    /// <summary>
    /// RemoveLast
    /// </summary>
    /// <param name="size"></param>
    public void RemoveLast(int size = 1)
    {
        if (size < 1) size = 1;

        if (_sb.Length >= size)
            _sb.Remove(_sb.Length - size, size);
    }

    /// <summary>
    /// 插入
    /// </summary>
    /// <param name="index"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public void Insert(int index, dynamic text)
    {
        if (text == null) return;
        _sb.Insert(index, text);
    }
    /// <summary>
    /// Replace
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public void Replace(string oldValue, string newValue)
    {
        _sb.Replace(oldValue, newValue);
    }

    /// <summary>
    /// Replace
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    /// <param name="startIndex"></param>
    /// <param name="count"></param>
    public void Replace(string oldValue, string newValue, int startIndex, int count)
    {
        _sb.Replace(oldValue, newValue, startIndex, count);
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return _sb.ToString();
    }
    /// <summary>
    /// ToString
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public string ToString(int start, int count)
    {
        return _sb.ToString(start, count);
    }

    /// <summary>
    /// Converts the current string builder content to a stream.
    /// </summary>
    /// <remarks>The returned stream contains the UTF-8 encoded bytes of the string builder's content. The
    /// stream's position is set to the beginning.</remarks>
    /// <returns>A <see cref="Stream"/> containing the UTF-8 encoded representation of the string builder's content.</returns>
    public Stream ToStream()
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(_sb.ToString()))
        {
            Position = 0
        };
        return ms;
    }

    /// <summary>
    /// Value
    /// </summary>
    public string Value
    {
        get
        {
            return _sb.ToString();
        }
    }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="sp"></param>
    public static implicit operator StringBuilder(StringPlus sp)
    {
        return new StringBuilder(sp.ToString());
    }


    static readonly List<string> _sqlKeyWords = new List<string>() {

                    "SELECT * FROM TABLE",
                    "INSERT INTO TALBE(FILED) VALUES(VALUE)",
                    "DELETE FROM",
                    "UPDATE [TABLE] SET [FILED]=VALUE WHERE",
                    "DROP",
                    "ALTER",
                    "TOP",
                    "DISTINCT",
                    "ALL",
                    "AND",
                    "NOT",
                    "OR",
                    "WHERE",
                    "FROM",
                    "SET",
                    "VALUES",
                    "JOIN",
                    "LEFT",
                    "FULL",
                    "RIGHT",
                    "INNER",
                    "GROUP BY",
                    "BY",
                    "ORDER BY",
                    "ASC",
                    "DESC",
                    "UNION",
                    "BETWEEN",
                    "IS",
                    "WITH",
                    "AS",
                    "AVG",
                    "MIN",
                    "MAX",
                    "SUM",
                    "COUNT",
                    "HAVING",
                    "TRUNCATE TABLE",
                    "TABLE",
                    "VIEW",
                    "PROCEDURE",
                    "BEGIN",
                    "END",
                    "CREATE",
                    "ADD"
    };

    /// <summary>
    /// 提示字符集
    /// </summary>
    public static List<string> SQLKeyWords
    {
        get
        {
            return _sqlKeyWords;
        }
    }
    /// <summary>
    /// GetSQLKeyWords
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public static string[] GetSQLKeyWords(string keyCode)
    {
        return SQLKeyWords.Where(b => b.IndexOf(keyCode, StringComparison.OrdinalIgnoreCase) == 0).ToArray();
    }


    private static Regex regSpace = new Regex(@"\s");
    /// <summary>
    /// 去掉空格
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ReplaceSpace(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        char firstChar = value[0];
        if (firstChar >= 48 && firstChar <= 57)
        {
            //value = "F" + value;
            value = "_" + value;
        }
        return regSpace.Replace(value.Trim(), " ");
    }

    /// <summary>
    /// 首字母大写
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToUpperFirstword(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        return value.Substring(0, 1).ToUpper() + value.Substring(1);
    }


    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _sb.Clear();
    }
}
