/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.TestProject1
*文件名： PptxUnitTest
*版本号： V1.0.0.0
*唯一标识：5884f17d-c239-4290-a7db-6b0f8a38c5bb
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/6/17 16:46:58
*描述：测试Pptx
*
*=================================================
*修改标记
*修改时间：2024/6/17 16:46:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：测试Pptx
*
*****************************************************************************/



namespace LuBan.TestProject1;

/// <summary>
/// 测试Pptx
/// </summary>
[TestClass]
public class PptxTest
{
    string srcFileName, dstFileName, picture1_replace_png, picture1_replace_jpeg, picture1_replace_mp4;

    /// <summary>
    /// 初始化
    /// </summary>
    [TestInitialize]
    public void Init()
    {

        srcFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "ReplaceLotsOfPicturesAndSlides.pptx");
        dstFileName = Path.Combine(PathUtil.GetRootFilePath("files"), "ReplaceLotsOfPicturesAndSlides_output.pptx");
        File.Delete(dstFileName);
        File.Copy(srcFileName, dstFileName);

        picture1_replace_png = Path.Combine(PathUtil.GetRootFilePath("files"), "picture1_replace.png");
        picture1_replace_jpeg = Path.Combine(PathUtil.GetRootFilePath("files"), "picture1_replace.jpg");
        picture1_replace_mp4 = Path.Combine(PathUtil.GetRootFilePath("files"), "picture1_replace.mp4");
    }

    /// <summary>
    /// 测试获取文本
    /// </summary>
    [TestMethod]
    public void GetAllText()
    {
        //PptDocument.LoadFromPdf(dstFileName);

        using (PptDocument document = new PptDocument(dstFileName))
        {
            var allText1 = document.GetAllText();

            var slide = document.GetSlideByIndex(0);

            if (slide != null)
            {
                var allText2 = slide.GetAllText();

                var count = allText2.Count();
            }

            var slides = document.GetSlidesByNoteText("{{LotsOfPictures}}");
            if (slides != null)
            {
                foreach (var textSlide in slides)
                {
                    var shapes1 = textSlide.GetShapesByAltTxt("{{text1}}");
                    if (shapes1 != null)
                    {
                        foreach (var shape in shapes1)
                        {
                            //替换文字
                            shape.SetText("alter text");
                        }

                    }

                }
            }

        }
    }


    /// <summary>
    /// 测试表格内容
    /// </summary>

    [TestMethod]
    public void ReplaceTableContent()
    {
        using (PptDocument document = new PptDocument(dstFileName))
        {
            var slide = document.GetSlideByNumber(3);

            var shapes = slide.GeTableShapes();

            foreach (var shape in shapes)
            {
                //shape.ReplaceTextInTable("{{name1}}", "李四");
                //shape.ReplaceTextInTable("{{name2}}", "王五");
                //shape.ReplaceTextInTable("{{position1}}", "测试");
                //shape.ReplaceTextInTable("{{position2}}", "产品");

                shape.ReplaceTextInTable(("{{name1}}", "李四"), ("{{name2}}", "王五"), ("{{position1}}", "测试"), ("{{position2}}", "产品"));
            }
        }
    }


    /// <summary>
    /// 替换图片视频
    /// </summary>
    [TestMethod]
    public void ReplaceLotsOfPicturesAndSlides()
    {
        try
        {
            byte picture1_replace_empty = new byte { };

            using (PptDocument document = new PptDocument(dstFileName))
            {
                var slides1 = document.GetSlidesByNoteText("{{LotsOfPictures}}");
                if (slides1 != null)
                {

                    foreach (var slide in slides1)
                    {
                        var shapes1 = slide.GetShapesByAltTxt("{{picture1png}}");
                        if (shapes1 != null)
                        {
                            foreach (var shape in shapes1)
                            {
                                shape.ReplaceImage(picture1_replace_png);
                            }
                        }

                        var shapes2 = slide.GetShapesByAltTxt("{{picture1jpeg}}");
                        if (shapes2 != null)
                        {
                            foreach (var shape in shapes2)
                            {
                                shape.ReplaceImage(picture1_replace_png);
                            }
                        }
                    }
                }

                var slides2 = document.GetSlidesByNoteText("{{LotsOfVideos}}");
                if (slides2 != null)
                {
                    foreach (var slide in slides2)
                    {
                        var shapes = slide.GetShapesByAltTxt("{{picture1mp4}}");
                        if (shapes != null)
                        {
                            foreach (var shape in shapes)
                            {
                                shape.ReplaceVideo(picture1_replace_mp4, picture1_replace_jpeg);
                            }
                        }
                    }
                }
            }
        }
        catch
        {

        }

    }
}
