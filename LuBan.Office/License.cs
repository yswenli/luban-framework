/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Office
*文件名： License
*版本号： V1.0.0.0
*唯一标识：a01bef51-e743-4007-aa26-467891e55675
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/15 17:59:27
*描述：许可证
*
*=================================================
*修改标记
*修改时间：2024/7/15 17:59:27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：许可证
*
*****************************************************************************/
namespace LuBan.Office;

/// <summary>
/// 许可证
/// </summary>
[SupportedOSPlatform("windows")]
public class License : BaseSingleInstance<License>
{
    readonly string _licensePath;


    /// <summary>
    /// 许可证
    /// </summary>
    public License()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _licensePath = Path.Combine(PathUtil.GetRootFilePath("Libs"), "Aspose.Total.lic");
    }

    /// <summary>
    /// word
    /// </summary>
    public void SetWordsLicense()
    {
        new Aspose.Words.License().SetLicense(_licensePath);
    }
    /// <summary>
    /// pdf
    /// </summary>
    public void SetPdfLicense()
    {
        new Aspose.Pdf.License().SetLicense(_licensePath);
    }
    /// <summary>
    /// ppt
    /// </summary>
    public void SetSlidesLicense()
    {
        new Aspose.Slides.License().SetLicense(_licensePath);
    }
    /// <summary>
    /// Excel
    /// </summary>
    public void SetCellsLicense()
    {
        new Aspose.Cells.License().SetLicense(_licensePath);
    }
}
