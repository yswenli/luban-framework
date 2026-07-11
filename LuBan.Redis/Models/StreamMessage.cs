/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Models
*文件名： StreamMessage
*版本号： V1.0.0.0
*唯一标识：7c776938-4ccc-4275-a575-150db0e749be
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/14 10:39:00
*描述：redis stream 消息
*
*=================================================
*修改标记
*修改时间：2025/8/14 10:39:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：redis stream 消息
*
*****************************************************************************/
namespace LuBan.Redis.Models;

/// <summary>
/// redis stream 消息
/// </summary>
/// <typeparam name="T"></typeparam>
public class StreamMessage<T> where T : class, new()
{
    /// <summary>
    /// 消息id
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 消息内容
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// redis stream 消息
    /// </summary>
    public StreamMessage()
    {

    }

    /// <summary>
    /// redis stream 消息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public StreamMessage(string id, T data)
    {
        Id = id;
        Data = data;
    }
}
