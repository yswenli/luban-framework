/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： CloudStorageFileHandlerPathUnitTest
*唯一标识：上传目录规范化 / 对象存储 ObjectKey 前缀合法性单测
*创建时间：2026/10/08
*描述：修复回归——savePath 不能使用 Path.GetFullPath（Linux 下产生以 "/" 开头的绝对路径，
*      同时作为本地目录与 OSS ObjectKey，触发阿里云 Invalid Object Key）。
*      验证 NormalizeSavePath/ResolveSavePath 始终返回非空、不以 "/" 开头的相对路径。
*
*****************************************************************************/
using LuBan.CloudStorage;

using System.Text.RegularExpressions;

namespace LuBan.XTestProject;

[TestClass]
public class CloudStorageFileHandlerPathUnitTest
{
    const string DefaultPathTemplate = "upload/{yyyy}/{MM}/{dd}";

    [TestMethod]
    [DataRow("upload/dev/avatar", "upload/dev/avatar", DisplayName = "普通相对路径原样保留")]
    [DataRow("upload\\dev\\avatar", "upload/dev/avatar", DisplayName = "反斜杠归一为正斜杠")]
    [DataRow("upload//dev///avatar", "upload/dev/avatar", DisplayName = "重复分隔符折叠")]
    [DataRow("upload/dev/avatar/", "upload/dev/avatar", DisplayName = "尾部斜杠剔除")]
    [DataRow("/app/upload/dev/avatar", "app/upload/dev/avatar", DisplayName = "Linux 绝对路径剔除前导斜杠（线上回归根因）")]
    [DataRow("/upload/dev/avatar", "upload/dev/avatar", DisplayName = "前导斜杠剔除")]
    [DataRow("C:/sites/upload/dev/avatar", "sites/upload/dev/avatar", DisplayName = "Windows 盘符段剔除")]
    [DataRow("C:\\sites\\upload\\dev\\avatar", "sites/upload/dev/avatar", DisplayName = "Windows 盘符+反斜杠剔除")]
    [DataRow("../avatar", "avatar", DisplayName = "父目录遍历段剔除")]
    [DataRow("../../etc/passwd", "etc/passwd", DisplayName = "多级父目录遍历段剔除")]
    [DataRow("a/../../b", "a/b", DisplayName = "中间父目录遍历段剔除")]
    [DataRow("....//avatar", "..../avatar", DisplayName = "四连点非遍历段保留")]
    [DataRow("  upload/dev/avatar  ", "upload/dev/avatar", DisplayName = "各段首尾空白裁剪")]
    [DataRow("upload/ /avatar", "upload/avatar", DisplayName = "纯空白段剔除")]
    [DataRow("   ", "", DisplayName = "纯空白")]
    [DataRow(".", "", DisplayName = "单点段剔除后为空")]
    [DataRow("..", "", DisplayName = "双点段剔除后为空")]
    [DataRow("/", "", DisplayName = "纯根路径剔除后为空")]
    [DataRow("", "", DisplayName = "空串")]
    public void NormalizeSavePath_AlwaysRelative(string input, string expected)
    {
        var actual = FileHandler.NormalizeSavePath(input);

        Assert.AreEqual(expected, actual);
        Assert.IsFalse(actual.StartsWith("/"), "规范化结果不能以 '/' 开头");
        Assert.IsFalse(actual.StartsWith("\\"), "规范化结果不能以 '\\' 开头");
    }

    [TestMethod]
    [DataRow("upload/dev/avatar", DisplayName = "普通相对路径")]
    [DataRow("upload\\dev\\avatar", DisplayName = "反斜杠路径")]
    [DataRow("/app/upload/dev/avatar", DisplayName = "Linux 绝对路径")]
    [DataRow("C:/sites/upload/dev/avatar", DisplayName = "Windows 绝对路径")]
    [DataRow("../avatar", DisplayName = "遍历路径")]
    public void ResolveSavePath_NonEmpty_ReturnsNormalizedRelative(string savePath)
    {
        var result = FileHandler.ResolveSavePath(savePath, DefaultPathTemplate);

        Assert.AreEqual(FileHandler.NormalizeSavePath(savePath), result);
        Assert.IsFalse(result.StartsWith("/"), "结果不能以 '/' 开头，否则 OSS 会抛 Invalid Object Key");
    }

    [TestMethod]
    [DataRow(null, DisplayName = "null 回退模板")]
    [DataRow("", DisplayName = "空串回退模板")]
    [DataRow("   ", DisplayName = "空白回退模板")]
    [DataRow("/", DisplayName = "纯根路径回退模板")]
    [DataRow("..", DisplayName = "纯父目录段回退模板")]
    [DataRow("./", DisplayName = "纯当前目录段回退模板")]
    public void ResolveSavePath_EmptyOrDefault_FallsBackToTemplate(string? savePath)
    {
        var result = FileHandler.ResolveSavePath(savePath, DefaultPathTemplate);

        Assert.IsFalse(result.IsNullOrEmpty(), "回退结果必须非空");
        Assert.IsFalse(result.StartsWith("/"), "回退结果不能以 '/' 开头");
        Assert.IsTrue(result.StartsWith("upload/"), $"回退结果应来自配置模板，实际：{result}");
        Assert.IsFalse(result.Contains("{"), "模板占位符应全部被替换，实际：" + result);
        Assert.IsTrue(Regex.IsMatch(result, @"^upload/\d{4}/\d{2}/\d{2}$"), $"应为 upload/yyyy/MM/dd，实际：{result}");
    }

    [TestMethod]
    public void ObjectKey_ProdAvatarScenario_IsRelativeAndValidForOss()
    {
        // 线上头像接口传入的 savePath（TJCES FileService.GetSavePath 返回值）
        const string savePath = "upload/dev/avatar";
        const string finalName = "131234567890.png";

        var folder = FileHandler.ResolveSavePath(savePath, DefaultPathTemplate);
        var objectKey = $"{folder}/{finalName}";

        Assert.AreEqual("upload/dev/avatar/131234567890.png", objectKey);
        Assert.IsFalse(objectKey.StartsWith("/"), "ObjectKey 不能以 '/' 开头");
        Assert.IsFalse(objectKey.StartsWith("\\"), "ObjectKey 不能以 '\\' 开头");
        Assert.IsTrue(objectKey.Length is > 0 and <= 1023, "ObjectKey 长度必须在 1..1023 之间");
    }

    [TestMethod]
    public void ResolveSavePath_MustNotReproduceLinuxAbsoluteBug()
    {
        // 旧实现 Path.GetFullPath("upload/dev/avatar") 在 Linux 容器(WORKDIR /app)下生成
        // "/app/upload/dev/avatar"，作为 ObjectKey 前缀非法。修复后必须仍是相对路径。
        var folder = FileHandler.ResolveSavePath("upload/dev/avatar", DefaultPathTemplate);

        Assert.AreEqual("upload/dev/avatar", folder);
        Assert.IsFalse(folder.Contains(":"), "结果不应含 Windows 盘符");
        Assert.IsFalse(folder.StartsWith("/"), "结果不应是 Linux 绝对路径");
    }
}