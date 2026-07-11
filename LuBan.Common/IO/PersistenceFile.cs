/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.IO
*文件名： PersistenceUtil
*版本号： V1.0.0.0
*唯一标识：8fee2833-315a-48f2-8a96-c837878e33ba
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/10 14:08:07
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/10 14:08:07
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.IO;


/// <summary>
/// 持久化工具类
/// </summary>
public class PersistenceFile
{
    string _filePath = string.Empty;

    /// <summary>
    /// 持久化工具类
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="fileName"></param>
    public PersistenceFile(string dir, string fileName)
    {
        _filePath = PathUtil.GetRootFullName(dir, fileName);
    }

    /// <summary>
    /// 持久化工具类
    /// </summary>
    /// <param name="fileName"></param>
    public PersistenceFile(string fileName) : this("caches", fileName)
    {

    }


    /// <summary>
    /// 读取数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T ReadData<T>() where T : new()
    {
        try
        {
            var data = FileUtil.Read<T>(_filePath);
            data ??= new T();
            return data;
        }
        catch
        {
            return new T();
        }
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    public void WriteData<T>(T t) where T : new()
    {
        if (t != null)
        {
            FileUtil.Write(_filePath, t);
        }
    }
}
