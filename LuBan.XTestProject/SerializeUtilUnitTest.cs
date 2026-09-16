/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： SerializeUtilUnitTest
*版本号： V1.0.0.0
*唯一标识：
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/16
*描述：SerializeUtil 序列化格式回归测试
*
*=================================================
*修改标记
*修改时间：2026/9/16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SerializeUtil 序列化格式回归测试
*
*****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using LuBan.Common;

namespace LuBan.XTestProject;

/// <summary>
/// SerializeUtil 序列化格式回归测试。
/// 断言值取自重构前实现的真实输出快照，用于保证 options 缓存化改造前后逐字节一致。
/// </summary>
[TestClass]
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "测试项目不参与裁剪")]
public class SerializeUtilUnitTest
{
    /// <summary>固定样本：Age/Enabled 为默认值，Remark 为 null，CreateTime 带毫秒。</summary>
    private static Sample NewSample() => new()
    {
        Name = "张三",
        Age = 0,
        Enabled = false,
        Remark = null,
        CreateTime = new DateTime(2026, 9, 16, 10, 20, 30, 456)
    };

    #region Serialize 写出格式

    /// <summary>默认参数：保留声明名（非 camelCase）、忽略 null、保留默认值、日期带毫秒、不缩进。</summary>
    [TestMethod]
    public void Serialize_Default_KeepsPascalCaseAndSkipsNull()
    {
        Assert.AreEqual(
            """{"Name":"张三","Age":0,"Enabled":false,"CreateTime":"2026-09-16 10:20:30.456"}""",
            SerializeUtil.Serialize(NewSample()));
    }

    /// <summary>defalutVal=false 切到 WhenWritingDefault，0 与 false 一并被忽略。</summary>
    [TestMethod]
    public void Serialize_NoDefaultVal_SkipsClrDefaults()
    {
        Assert.AreEqual(
            """{"Name":"张三","CreateTime":"2026-09-16 10:20:30.456"}""",
            SerializeUtil.Serialize(NewSample(), defalutVal: false));
    }

    /// <summary>nullValue=true 切到 Never，null 字段显式写出。</summary>
    [TestMethod]
    public void Serialize_WithNullValue_WritesNullExplicitly()
    {
        Assert.AreEqual(
            """{"Name":"张三","Age":0,"Enabled":false,"Remark":null,"CreateTime":"2026-09-16 10:20:30.456"}""",
            SerializeUtil.Serialize(NewSample(), nullValue: true));
    }

    /// <summary>camelCase=true 只改属性名策略，不改日期格式。</summary>
    [TestMethod]
    public void Serialize_CamelCase_OnlyRenamesProperties()
    {
        Assert.AreEqual(
            """{"name":"张三","age":0,"enabled":false,"createTime":"2026-09-16 10:20:30.456"}""",
            SerializeUtil.Serialize(NewSample(), camelCase: true));
    }

    /// <summary>四个开关可叠加组合，互不干扰。</summary>
    [TestMethod]
    public void Serialize_CombinedFlags_AreIndependent()
    {
        Assert.AreEqual(
            """{"name":"张三","createTime":"2026-09-16 10:20:30.456"}""",
            SerializeUtil.Serialize(NewSample(), defalutVal: false, nullValue: true, camelCase: true));
    }

    /// <summary>indented=true 输出多行。</summary>
    [TestMethod]
    public void Serialize_Indented_WritesMultipleLines()
    {
        const string nl = "\n";
        var expected = "{" + nl
            + "  \"Name\": \"张三\"," + nl
            + "  \"Age\": 0," + nl
            + "  \"Enabled\": false," + nl
            + "  \"CreateTime\": \"2026-09-16 10:20:30.456\"" + nl
            + "}";
        var json = SerializeUtil.Serialize(NewSample(), indented: true).Replace("\r\n", "\n");
        Assert.AreEqual(expected, json);
    }

    /// <summary>null 入参返回空串而非 "null"。</summary>
    [TestMethod]
    public void Serialize_Null_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, SerializeUtil.Serialize(null!));
    }

    /// <summary>中文不被转义（UnsafeRelaxedJsonEscaping）。</summary>
    [TestMethod]
    public void Serialize_Chinese_IsNotEscaped()
    {
        var json = SerializeUtil.Serialize(new Dictionary<string, object?> { ["a"] = 1, ["b"] = "中文", ["c"] = null });
        Assert.AreEqual("""{"a":1,"b":"中文","c":null}""", json);
    }

    /// <summary>
    /// Exception 走自定义转换器：属性名手写，不受 camelCase 策略影响；
    /// StackTrace/Source 为 null 时仍写出（转换器绕过 DefaultIgnoreCondition）。
    /// </summary>
    [TestMethod]
    public void Serialize_Exception_UsesCustomConverterAndIgnoresNamingPolicy()
    {
        var expected = """{"ExceptionType":"System.InvalidOperationException","Message":"boom","StackTrace":null,"Source":null}""";
        var ex = new InvalidOperationException("boom");
        Assert.AreEqual(expected, SerializeUtil.Serialize(ex));
        Assert.AreEqual(expected, SerializeUtil.Serialize(ex, camelCase: true));
        Assert.AreEqual(expected, ex.ToJson());
    }

    /// <summary>匿名对象输出与裸 JsonSerializer.Serialize 一致，保证工具插件替换后回传格式不变。</summary>
    [TestMethod]
    public void Serialize_AnonymousType_MatchesRawJsonSerializer()
    {
        var anon = new { oldSummaryChars = 0, newSummaryChars = 12, remainingMessages = 3 };
        Assert.AreEqual(
            System.Text.Json.JsonSerializer.Serialize(anon),
            SerializeUtil.Serialize(anon));
    }

    /// <summary>options 缓存化后重复调用必须产出完全相同的结果（缓存串味回归防护）。</summary>
    [TestMethod]
    public void Serialize_RepeatedCallsWithDifferentFlags_DoNotLeakState()
    {
        var plain = """{"Name":"张三","Age":0,"Enabled":false,"CreateTime":"2026-09-16 10:20:30.456"}""";
        var camel = """{"name":"张三","age":0,"enabled":false,"createTime":"2026-09-16 10:20:30.456"}""";

        for (var i = 0; i < 3; i++)
        {
            Assert.AreEqual(plain, SerializeUtil.Serialize(NewSample()));
            Assert.AreEqual(camel, SerializeUtil.Serialize(NewSample(), camelCase: true));
            Assert.AreEqual(plain, SerializeUtil.Serialize(NewSample()));
            Assert.AreEqual(camel, SerializeUtil.Serialize(NewSample(), camelCase: true));
        }
    }

    #endregion

    #region ToJson 扩展

    /// <summary>ToJson 默认缩进，且不支持 camelCase（与 Serialize 的差异点）。</summary>
    [TestMethod]
    public void ToJson_Default_IsIndentedPascalCase()
    {
        Assert.AreEqual(SerializeUtil.Serialize(NewSample(), indented: true), NewSample().ToJson());
    }

    /// <summary>hasIndentation=false 时与 Serialize 默认输出一致。</summary>
    [TestMethod]
    public void ToJson_NoIndent_MatchesSerializeDefault()
    {
        Assert.AreEqual(SerializeUtil.Serialize(NewSample()), NewSample().ToJson(hasIndentation: false));
    }

    /// <summary>ToJson 吞异常返回空串；Serialize 不吞异常，这是两者的既有差异，不得抹平。</summary>
    [TestMethod]
    public void ToJson_SelfReference_SwallowsException()
    {
        Assert.AreEqual(string.Empty, new SelfRef().ToJson());
        Assert.Throws<System.Text.Json.JsonException>(() => SerializeUtil.Serialize(new SelfRef()));
    }

    /// <summary>null 入参返回空串。</summary>
    [TestMethod]
    public void ToJson_Null_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, ((object)null!).ToJson());
    }

    #endregion

    #region Deserialize / ToObject 读入格式

    /// <summary>属性名大小写不敏感。</summary>
    [TestMethod]
    public void Deserialize_IsCaseInsensitive()
    {
        var s = SerializeUtil.Deserialize<Sample>("""{"NAME":"李四","Age":5}""")!;
        Assert.AreEqual("李四", s.Name);
        Assert.AreEqual(5, s.Age);
        Assert.AreEqual("李四", """{"NAME":"李四"}""".ToObject<Sample>()!.Name);
    }

    /// <summary>能精确读回自己写出的 yyyy-MM-dd HH:mm:ss.fff 格式，往返无损。</summary>
    [TestMethod]
    public void Deserialize_OwnFormat_RoundTrips()
    {
        var src = NewSample();
        var back = SerializeUtil.Deserialize<Sample>(SerializeUtil.Serialize(src))!;
        Assert.AreEqual(src.CreateTime, back.CreateTime);
        Assert.AreEqual(src.Name, back.Name);
    }

    /// <summary>日期为空串时返回 default，不抛异常。</summary>
    [TestMethod]
    public void Deserialize_EmptyDateString_ReturnsDefault()
    {
        Assert.AreEqual(default(DateTime), SerializeUtil.Deserialize<Sample>("""{"CreateTime":""}""")!.CreateTime);
    }

    /// <summary>非法 JSON 与空串均返回 default（吞异常语义）。</summary>
    [TestMethod]
    public void Deserialize_InvalidOrEmpty_ReturnsDefault()
    {
        Assert.IsNull(SerializeUtil.Deserialize<Sample>("{bad json"));
        Assert.IsNull(SerializeUtil.Deserialize<Sample>(""));
        Assert.IsNull(SerializeUtil.Deserialize(""));
        Assert.IsNull(SerializeUtil.Deserialize("{bad json", typeof(Sample)));
    }

    /// <summary>非泛型 Deserialize 按 Type 反序列化。</summary>
    [TestMethod]
    public void Deserialize_ByType_Works()
    {
        var obj = SerializeUtil.Deserialize("""{"Name":"赵六"}""", typeof(Sample));
        Assert.IsInstanceOfType<Sample>(obj);
        Assert.AreEqual("赵六", ((Sample)obj!).Name);
    }

    /// <summary>
    /// 带时区标记的 ISO-8601 字符串按 RoundtripKind 解析，保留原始时区语义，不再被折算成本地时间。
    /// 这是合并 LuBan.Web.Core 转换器后的既定行为。
    /// </summary>
    [TestMethod]
    public void Deserialize_Iso8601WithZone_PreservesRoundtripKind()
    {
        // Z 结尾：Kind=Utc，墙上时间即 UTC 时间（旧实现会折算成 Kind=Local 的 18:20:30）
        var utc = SerializeUtil.Deserialize<Sample>("""{"CreateTime":"2026-09-16T10:20:30Z"}""")!.CreateTime;
        Assert.AreEqual(DateTimeKind.Utc, utc.Kind);
        Assert.AreEqual(new DateTime(2026, 9, 16, 10, 20, 30, DateTimeKind.Utc), utc);

        // 带偏移：折算到本地时间但保留时刻，ToUniversalTime 恒为 02:20:30Z，与机器时区无关
        var offset = SerializeUtil.Deserialize<Sample>("""{"CreateTime":"2026-09-16T10:20:30+08:00"}""")!.CreateTime;
        Assert.AreEqual(new DateTime(2026, 9, 16, 2, 20, 30, DateTimeKind.Utc), offset.ToUniversalTime());

        // 无时区标记：Kind=Unspecified，不做任何折算
        var naive = SerializeUtil.Deserialize<Sample>("""{"CreateTime":"2026-09-16T10:20:30"}""")!.CreateTime;
        Assert.AreEqual(DateTimeKind.Unspecified, naive.Kind);
        Assert.AreEqual(new DateTime(2026, 9, 16, 10, 20, 30), naive);

        // 完全无法解析时返回 default，不抛异常
        Assert.AreEqual(default(DateTime), SerializeUtil.Deserialize<Sample>("""{"CreateTime":"not-a-date"}""")!.CreateTime);
    }

    #endregion

    #region Convert / DeepClone

    /// <summary>Convert 通过 JSON 中转把匿名对象映射成强类型，属性名大小写不敏感。</summary>
    [TestMethod]
    public void Convert_AnonymousToTyped_Works()
    {
        var s = SerializeUtil.Convert<Sample>(new { name = "王五", age = 9 })!;
        Assert.AreEqual("王五", s.Name);
        Assert.AreEqual(9, s.Age);
    }

    /// <summary>DeepClone 产出内容相等但引用不同的副本。</summary>
    [TestMethod]
    public void DeepClone_ProducesIndependentCopy()
    {
        var src = NewSample();
        var copy = src.DeepClone()!;
        Assert.AreNotSame(src, copy);
        Assert.AreEqual(src.Name, copy.Name);
        Assert.AreEqual(src.CreateTime, copy.CreateTime);
    }

    #endregion

    /// <summary>测试样本模型。</summary>
    public class Sample
    {
        /// <summary>名称。</summary>
        public string Name { get; set; } = "";

        /// <summary>年龄，用于验证默认值忽略策略。</summary>
        public int Age { get; set; }

        /// <summary>启用标记，用于验证默认值忽略策略。</summary>
        public bool Enabled { get; set; }

        /// <summary>备注，用于验证 null 忽略策略。</summary>
        public string? Remark { get; set; }

        /// <summary>创建时间，用于验证日期格式。</summary>
        public DateTime CreateTime { get; set; }
    }

    /// <summary>自引用模型，用于触发序列化深度异常。</summary>
    public class SelfRef
    {
        /// <summary>返回自身，造成无限递归。</summary>
        public object Self => this;
    }
}
