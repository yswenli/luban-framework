/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit.Models
*文件名： UserAddress
*版本号： V1.0.0.0
*唯一标识：b99cf70a-7158-487b-9ffd-9d90d261ad11
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/15 10:26:44
*描述：邮件地址
*
*=================================================
*修改标记
*修改时间：2024/8/15 10:26:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：邮件地址
*
*****************************************************************************/
namespace LuBan.EMailKit.Models;

/// <summary>
/// 邮件地址
/// </summary>
public class UserAddress
{
    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 邮件地址
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// 邮件地址
    /// </summary>
    public UserAddress()
    {

    }

    /// <summary>
    /// 邮件地址
    /// </summary>
    /// <param name="name"></param>
    /// <param name="address"></param>
    public UserAddress(string name, string address)
    {
        Name = name;
        Address = address;
    }
}
