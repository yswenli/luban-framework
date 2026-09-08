/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Models
*文件名： PhoneVerifyCodeInfo
*版本号： V1.0.0.0
*唯一标识：d7c2f1a3-e5b6-4c9d-8f2a-4e6d8b0f3c7e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/09/08 10:00:00
*描述：手机验证码缓存模型
*
*=================================================
*修改标记
*修改时间：2026/09/08 10:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：创建
*
*****************************************************************************/
namespace LuBan.Web.Core.Models;

public class PhoneVerifyCodeInfo
{
    public string Code { get; set; }
    public DateTime CreateTime { get; set; }
    public bool IsUsed { get; set; }
    public int ExpireMinutes { get; set; }
}