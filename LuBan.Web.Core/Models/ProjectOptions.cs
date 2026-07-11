/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Models
*文件名： ProjectOptions
*版本号： V1.0.0.0
*唯一标识：f0ad20ec-9c36-4803-93e5-890d7bf7b327
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/10/16 9:17:31
*描述：项目配置
*
*=================================================
*修改标记
*修改时间：2024/10/16 9:17:31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：项目配置
*
*****************************************************************************/
namespace LuBan.Web.Core.Models;

/// <summary>
/// 项目配置
/// </summary>
public class ProjectOptions
{
    /// <summary>
    /// 项目配置
    /// </summary>
    public static ProjectOptions Instance
    {
        get
        {
            var configData = new DbRepository<DbConfig>().First(q => q.Code == CommonConst.SysManagementPlatformCode);
            if (configData != null && configData.Value.IsNotNullOrEmpty())
            {
                var data = SerializeUtil.Deserialize<ProjectOptions>(configData.Value);
                if (data == null)
                {
                    return new ProjectOptions();
                }
                else
                {
                    return data;
                }
            }
            return new ProjectOptions();
        }
    }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string Name { get; set; } = "LuBan";
    /// <summary>
    /// 项目描述
    /// </summary>
    public string Version { get; set; } = "LuBan管理平台";
    /// <summary>
    /// 项目logo
    /// </summary>
    public string Logo { get; set; } = "/admin/img/logo.png";
    /// <summary>
    /// 底部信息
    /// </summary>
    public string Footer { get; set; } = $"Copyright © {DateTime.Now.Year} yswenli All rights reserved.";
    /// <summary>
    /// 主题颜色
    /// </summary>
    public string MainColor { get; set; } = "#11559C";
    /// <summary>
    /// 菜单背景颜色
    /// </summary>
    public string MenuBGColor { get; set; } = "#1977DA";
    /// <summary>
    /// 欢迎语
    /// </summary>
    public string WelcomeText { get; set; } = "欢迎使用LuBan管理平台";
    /// <summary>
    /// 水印文本
    /// </summary>
    public string Watermark { get; set; } = "LuBan管理平台";
}
