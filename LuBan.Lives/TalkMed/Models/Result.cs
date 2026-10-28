/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Lives.TalkMed.Models
*文件名： Result.cs
*版本号： V1.0.0.0
*唯一标识：42baaa1c-837b-4b97-abcd-6675e63ce387
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：Result 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Result 类
*
*****************************************************************************/



namespace LuBan.Lives.TalkMed.Models;

[DataContract]
/// <summary>
/// Result 结果类
/// </summary>
public class Result<T>
{
    /// <summary>
    /// 接口响应状态码，0 通常表示成功（接口字段 code）。
    /// </summary>
    [DataMember(Name = "code")]
    public int Code { get; set; }

    /// <summary>
    /// 接口返回的业务数据体（接口字段 data）。
    /// </summary>
    [DataMember(Name = "data")]
    public T Data { get; set; }

    /// <summary>
    /// 接口响应提示信息（接口字段 message）。
    /// </summary>
    [DataMember(Name = "message")]
    public string Message { get; set; }
}