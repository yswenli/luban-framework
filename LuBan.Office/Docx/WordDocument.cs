/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Office.Docx
*文件名： WordDocument
*版本号： V1.0.0.0
*唯一标识：360abc27-6c77-4aad-96c2-5e0bef5e4505
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/22 15:01:22
*描述：Word文档
*
*=================================================
*修改标记
*修改时间：2025/10/22 15:01:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Word文档
*
*****************************************************************************/
using Aspose.Words;
using Aspose.Words.Drawing;
using Aspose.Words.Layout;
using Aspose.Words.Replacing;
using Aspose.Words.Tables;

using System.Reflection;
using System.Text.RegularExpressions;

namespace LuBan.Office.Docx;


/// <summary>
/// Word文档，
/// https://docs.aspose.com/words/zh/net/developer-guide/
/// </summary>
[SupportedOSPlatform("windows")]
public class WordDocument
{
    Document _doc;

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// pdf文档
    /// </summary>
    /// <param name="filePath"></param>
    public WordDocument(string filePath)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            throw new PlatformNotSupportedException("LuBan.Office only support Windows");
        }
        License.Instance.SetWordsLicense();
        FilePath = filePath;
        _doc = new Document(filePath);
    }

    /// <summary>
    /// 替换文本
    /// </summary>
    /// <param name="oldText"></param>
    /// <param name="newText"></param>
    /// <returns></returns>
    public bool ReplaceText(string oldText, string newText)
    {
        return _doc.Range.Replace(oldText, newText, new FindReplaceOptions(FindReplaceDirection.Forward)) > 0;
    }


    /// <summary>
    /// 插入文本
    /// </summary>
    /// <param name="text"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool InsertText(string text, int index)
    {
        if (index == -1)
        {
            Paragraph para = new Paragraph(_doc);
            para.AppendChild(new Run(_doc, text));
            _doc.FirstSection.Body.AppendChild(para);
        }
        else
        {
            Paragraph para = _doc.FirstSection.Body.Paragraphs[index];
            para.AppendChild(new Run(_doc, text));
        }
        return true;
    }

    /// <summary>
    /// 获取文档总行数
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public int GetParagraphCount()
    {
        var collector = new LayoutCollector(_doc);
        var it = new LayoutEnumerator(_doc);
        var total = 0;
        foreach (Paragraph paragraph in _doc.GetChildNodes(NodeType.Paragraph, true))
        {
            var paraBreak = collector.GetEntity(paragraph);

            object? stop = null;
            var prevItem = paragraph.PreviousSibling;
            if (prevItem != null)
            {
                var prevBreak = collector.GetEntity(prevItem);
                if (prevItem is Paragraph)
                {
                    it.Current = collector.GetEntity(prevItem); // para break
                    it.MoveParent();    // last line
                    stop = it.Current;
                }
                else if (prevItem is Table)
                {
                    var table = (Table)prevItem;
                    it.Current = collector.GetEntity(table.LastRow.LastCell.LastParagraph); // cell break
                    it.MoveParent();    // cell
                    it.MoveParent();    // row
                    stop = it.Current;
                }
                else
                {
                    throw new Exception();
                }
            }

            it.Current = paraBreak;
            it.MoveParent();

            var count = 1;
            while (it.Current != stop)
            {
                if (!it.MovePreviousLogical())
                    break;
                count++;
            }

            //const int MAX_CHARS = 16;
            //var paraText = paragraph.GetText();
            //if (paraText.Length > MAX_CHARS)
            //    paraText = $"{paraText.Substring(0, MAX_CHARS)}...";

            //Console.WriteLine($"Paragraph '{paraText}' has {count} line(-s).");
            total += count;
        }
        return total;
    }

    /// <summary>
    /// 替换文本为图片
    /// </summary>
    /// <param name="oldText"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public bool ReplaceTextWithImage(string oldText, string fileName)
    {
        Regex reg = new Regex(oldText);
        _doc.Range.Replace(reg, new ReplaceAndInsertImage(fileName), false);
        return true;
    }

    /// <summary>
    /// 插入图片到指定位置
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public bool InserImage(string fileName, int left = 100, int top = 100, int width = 100, int height = 100)
    {
        var builder = new DocumentBuilder(_doc);
        var shape = builder.InsertImage(fileName,
            RelativeHorizontalPosition.Margin,
            left,
            RelativeVerticalPosition.Margin,
            top,
            width,
            height,
            WrapType.Square);
        return true;
    }

    /// <summary>
    /// 替换表格文本
    /// </summary>
    /// <param name="tableIndex"></param>
    /// <param name="replaceData"></param>
    /// <returns></returns>
    public bool ReplaceTextWithTable(int tableIndex, (string, string)[] replaceData)
    {
        var sections = _doc.Sections;
        if (sections == null || sections.Count == 0) return false;
        var offset = 0;
        Table? table = null;
        foreach (Section section in sections)
        {
            var tables = section.Body.Tables;
            if (tables != null && tables.Count > 0)
            {
                for (var i = 0; i < tables.Count; i++)
                {
                    offset++;
                    if (offset > tableIndex)
                    {
                        table = tables[i];
                        break;
                    }
                }
            }
        }
        if (table == null) return false;
        RowCollection rows = table.Rows;
        if (rows == null || rows.Count == 0) return false;
        foreach (Row row in rows)
        {
            CellCollection cells = row.Cells;
            if (cells == null || cells.Count == 0) return false;
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell == null) continue;
                foreach (var item in replaceData)
                {
                    if (cell.GetText().Contains(item.Item1))
                    {
                        cell.Range.Replace(item.Item1, item.Item2, false, true);
                    }
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 插入替换的数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="DocChuli"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool ReplaceTextWithBookmark<T>(T data)
    {
        if (_doc.Range.Bookmarks.Count > 0)
        {
            if (data != null)
            {
                PropertyInfo[] propertys = typeof(T).GetProperties();
                foreach (PropertyInfo item in propertys)
                {
                    if (_doc.Range.Bookmarks[item.Name] != null)
                    {
                        if (item.PropertyType == typeof(bool))
                        {
                            bool isSelect = bool.Parse(item.GetValue(data, null)?.ToString() ?? "0");
                            var builder = new DocumentBuilder(_doc);
                            builder.MoveToBookmark(item.Name);
                            builder.InsertCheckBox("", isSelect, 0);
                        }
                        else
                        {
                            var val = item.GetValue(data, null);
                            if (val != null)
                            {
                                string valueString = val.ToString() ?? "";
                                _doc.Range.Bookmarks[item.Name].Text = valueString;
                            }
                            else
                            {
                                _doc.Range.Bookmarks[item.Name].Text = "";
                            }
                        }
                    }
                }
            }
        }
        return true;
    }


    /// <summary>
    /// 导出为pdf
    /// </summary>
    /// <param name="pdfFilePath"></param>
    public void ExportToPdf(string pdfFilePath)
    {
        _doc.Save(pdfFilePath, SaveFormat.Pdf);
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="outputFilePath"></param>
    public void Save(string? outputFilePath = null)
    {
        if (string.IsNullOrEmpty(outputFilePath))
        {
            _doc.Save(FilePath);
        }
        else
        {
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
            _doc.Save(outputFilePath);
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Save();
    }
}




public class ReplaceAndInsertImage : IReplacingCallback
{
    /// <summary>
    /// 需要插入的图片路径
    /// </summary>
    public string url { get; set; }

    public ReplaceAndInsertImage(string url)
    {
        this.url = url;
    }

    public ReplaceAction Replacing(ReplacingArgs e)
    {
        //获取当前节点
        var node = e.MatchNode;
        //获取当前文档
        Document? doc = node.Document as Document;
        if (doc == null) return ReplaceAction.Replace;
        var builder = new DocumentBuilder(doc);
        //将光标移动到指定节点
        builder.MoveTo(node);
        //插入图片
        builder.InsertImage(url);
        return ReplaceAction.Replace;
    }
}
