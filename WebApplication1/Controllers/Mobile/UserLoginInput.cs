/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Mobile
*文件名： UserLoginInput.cs
*版本号： V1.0.0.0
*唯一标识：b617fa77-dfbe-4b75-9fbe-a3634c6ebee3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：UserLoginInput 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：UserLoginInput 类
*
*****************************************************************************/

namespace WebApplication1.Controllers.Mobile
{
    /// <summary>
/// UserLoginInput 输入模型
/// </summary>
    public class UserLoginInput
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}