/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.XTestProject.Models
*文件名： TestInfo.cs
*版本号： V1.0.0.0
*唯一标识：4dc92fc5-b58e-47aa-8e43-2617681f1acd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：TestInfo 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：TestInfo 类
*
*****************************************************************************/

namespace LuBan.XTestProject.Models
{
    /// <summary>
/// TestInfo 模型类
/// </summary>
    public class TestInfo
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartTime { get; set; }
    }
}
