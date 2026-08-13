using System.Data;
using LuBan.Common.Data;

namespace LuBan.UnitTestProject;

[TestClass]
public class ExcelUtilUnitTest
{
    [TestMethod]
    public void ExportAndImport_DataTable_RoundTrip()
    {
        var dt = new DataTable();
        dt.Columns.Add("Name");
        dt.Columns.Add("Age");
        dt.Rows.Add("Alice", 30);
        dt.Rows.Add("Bob", 25);

        var ms = ExcelUtil.ExportStreamFromDataTable(dt, "test.xlsx");
        Assert.IsTrue(ms.Length > 0);

        ms.Position = 0;
        var result = ExcelUtil.ImportFromStream(ms);
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result!.Rows.Count);
        Assert.AreEqual("Alice", result.Rows[0]["Name"].ToString());
        Assert.AreEqual("Bob", result.Rows[1]["Name"].ToString());
    }

    [TestMethod]
    public void ExportAndImport_DataTable_WithCustomColumnNames()
    {
        var dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Columns.Add("Col2");
        dt.Rows.Add("A", "B");

        var ms = ExcelUtil.ExportStreamFromDataTable(dt, "test.xlsx", columnNameList: new[] { "Name", "Value" });
        ms.Position = 0;

        var result = ExcelUtil.ImportFromStream(ms, columnNameList: new[] { "Name", "Value" });
        Assert.IsNotNull(result);
        Assert.AreEqual("Name", result!.Columns[0].ColumnName);
        Assert.AreEqual("Value", result.Columns[1].ColumnName);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.AreEqual("A", result.Rows[0]["Name"].ToString());
    }

    [TestMethod]
    public void ExportFile_DataTable_CreatesFile()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id");
        dt.Columns.Add("Data");
        dt.Rows.Add(1, "test");

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        try
        {
            ExcelUtil.ExportFileFromDataTable(dt, tempFile);
            Assert.IsTrue(File.Exists(tempFile));
            Assert.IsTrue(new FileInfo(tempFile).Length > 0);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [TestMethod]
    public void ImportFromStream_NullStream_ReturnsNull()
    {
        var result = ExcelUtil.ImportFromStream(null!);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ExportFileFromModels_GroupedData_CreatesMultiSheet()
    {
        var data = new List<TestItem>
        {
            new TestItem { Group = "GroupA", Name = "Item1", Value = 10 },
            new TestItem { Group = "GroupA", Name = "Item2", Value = 20 },
            new TestItem { Group = "GroupB", Name = "Item3", Value = 30 },
        };

        var groups = data.GroupBy(x => x.Group);

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_multi_{Guid.NewGuid()}.xlsx");
        try
        {
            ExcelUtil.ExportFileFromModels(groups, tempFile);
            Assert.IsTrue(File.Exists(tempFile));
            Assert.IsTrue(new FileInfo(tempFile).Length > 0);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    public class TestItem
    {
        public string Group { get; set; } = "";
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }

    [TestMethod]
    public void MergeExcelFileByStream_MergesSheets()
    {
        var dt1 = new DataTable();
        dt1.Columns.Add("Col1");
        dt1.Rows.Add("A");

        var dt2 = new DataTable();
        dt2.Columns.Add("Col2");
        dt2.Rows.Add("B");

        var ms1 = ExcelUtil.ExportStreamFromDataTable(dt1, "s1.xlsx", sheetName: "Sheet1");
        var ms2 = ExcelUtil.ExportStreamFromDataTable(dt2, "s2.xlsx", sheetName: "Sheet1");

        var files = new Dictionary<string, Stream>
        {
            ["First"] = ms1,
            ["Second"] = ms2
        };

        var merged = ExcelUtil.MergeExcelFileByStream(files);
        Assert.IsTrue(merged.Length > 0);
    }
}
