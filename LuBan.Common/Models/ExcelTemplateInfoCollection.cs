/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Models
*文件名： ExcelTemplateInfoCollection
*版本号： V1.0.0.0
*唯一标识：805973b3-f1a5-40d0-842f-20c517f650b8
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 16:51:29
*描述：格表模板信息
*
*=================================================
*修改标记
*修改时间：2022/6/21 16:51:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：格表模板信息
*
*****************************************************************************/
namespace LuBan.Common.Models;

/// <summary>
/// 格表模板信息
/// </summary>
public class ExcelTemplateInfo
{
    /// <summary>
    /// 表名
    /// </summary>
    public string SheetName { get; set; }
    /// <summary>
    /// 模板文件地址
    /// </summary>
    public Stream TemplateFileStream { get; set; }
    /// <summary>
    /// 模板文件是否是2007版的
    /// </summary>
    public bool Is2007 { get; set; } = true;
    /// <summary>
    /// 标签与值集合
    /// </summary>
    public Dictionary<string, string> LableValuePairs { get; set; }

    /// <summary>
    /// 数据列表开始行数，
    /// -1表示根据List.自动匹配开始行数
    /// </summary>
    public int DataListRowStartIndex { get; set; } = -1;

    /// <summary>
    /// 数据列表开始列数
    /// </summary>
    public int DataListColumnStartIndex { get; set; } = 0;

    /// <summary>
    /// 列表数据
    /// </summary>
    public List<Dictionary<string, string>> List { get; set; }

    /// <summary>
    /// 条形码的文本
    /// </summary>
    public string BarCodeText { get; set; }

    /// <summary>
    /// 条形码还是二维码
    /// </summary>
    public EnumCodeType BarCodeType { get; set; }

    /// <summary>
    /// 条形码的宽
    /// </summary>
    public int? BarCodeWidth { get; set; }
    /// <summary>
    /// 条形码的高
    /// </summary>
    public int? BarCodeHeight { get; set; }
    /// <summary>
    /// 条形码的列开始位置
    /// </summary>
    public int colStart { get; set; }
    /// <summary>
    /// 条形码的列结束位置
    /// </summary>
    public int colEnd { get; set; }
    /// <summary>
    /// 条形码的行开始位置
    /// </summary>
    public int rowStart { get; set; }
    /// <summary>
    /// 条形码的行结束位置 （例子rowstart：3，rowStart：5，colstart：8，colend：10， C8-D9区域插入条形码）
    /// </summary>
    public int rowEnd { get; set; }


}

/// <summary>
/// 格表模板信息集合
/// </summary>
public class ExcelTemplateInfoCollection : List<ExcelTemplateInfo>
{

}
