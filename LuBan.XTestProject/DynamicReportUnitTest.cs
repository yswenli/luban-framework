/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： DynamicReportUnitTest
*版本号： V1.0.0.0
*唯一标识：
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/5
*描述：动态报表单元测试
*
*=================================================
*修改标记
*修改时间：2026/6/5
*修改人： yswenli
*版本号： V1.0.0.0
*描述：动态报表单元测试
*
*****************************************************************************/

using LuBan.Reporting.Dynamic.Models;
using LuBan.Reporting.Dynamic.Services;
using System.Text.Json;

namespace LuBan.XTestProject;

/// <summary>
/// 动态报表单元测试
/// </summary>
[TestClass]
public class DynamicReportUnitTest
{
    /// <summary>
    /// 测试 Lua 脚本引擎 SQL 生成
    /// </summary>
    [TestMethod]
    public void TestLuaScriptEngine_GenerateSql()
    {
        using var engine = new LuaScriptEngine();

        var sqlTemplate = @"
            local sql = 'SELECT * FROM users WHERE 1=1'
            if params.status then
                sql = sql .. ' AND status = @status'
            end
            if params.keyword then
                sql = sql .. ' AND name LIKE @keyword'
            end
            return sql
        ";

        var parameters = new Dictionary<string, object>
        {
            { "status", 1 },
            { "keyword", "%test%" }
        };

        var result = engine.GenerateSql(sqlTemplate, parameters);

        Assert.IsTrue(result.Contains("status = @status"));
        Assert.IsTrue(result.Contains("name LIKE @keyword"));
    }

    /// <summary>
    /// 测试 Lua 脚本引擎值转换
    /// </summary>
    [TestMethod]
    public void TestLuaScriptEngine_ConvertValue()
    {
        using var engine = new LuaScriptEngine();

        var converterScript = @"
            function GetStatusText(value)
                local map = {['1']='启用', ['0']='禁用'}
                return map[tostring(value)] or '未知'
            end
        ";

        var result = engine.ConvertValue(converterScript, "GetStatusText(value)", "1");

        Assert.AreEqual("启用", result);
    }

    /// <summary>
    /// 测试 ValueMap 转换
    /// </summary>
    [TestMethod]
    public void TestValueMap_ConvertValue()
    {
        var converterConfig = "{\"1\":\"是\",\"0\":\"否\"}";
        var map = JsonSerializer.Deserialize<Dictionary<string, string>>(converterConfig);

        Assert.IsNotNull(map);
        Assert.AreEqual("是", map["1"]);
        Assert.AreEqual("否", map["0"]);
    }

    /// <summary>
    /// 测试 Lua 脚本引擎 JSON 解析
    /// </summary>
    [TestMethod]
    public void TestLuaScriptEngine_JsonParse()
    {
        using var engine = new LuaScriptEngine();

        var script = @"
            local data = json_parse('{""name"":""test"",""value"":123}')
            return json_stringify(data)
        ";

        var result = engine.Execute(script);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("test"));
    }

    /// <summary>
    /// 测试 Lua 脚本引擎字符串处理
    /// </summary>
    [TestMethod]
    public void TestLuaScriptEngine_StringFunctions()
    {
        using var engine = new LuaScriptEngine();

        var script = @"
            local parts = string_split('a,b,c', ',')
            return parts[1]
        ";

        var result = engine.Execute(script);

        Assert.AreEqual("a", result);
    }

    /// <summary>
    /// 测试 Lua 脚本引擎加密函数
    /// </summary>
    [TestMethod]
    public void TestLuaScriptEngine_CryptoFunctions()
    {
        using var engine = new LuaScriptEngine();

        var script = @"
            return md5('test')
        ";

        var result = engine.Execute(script);

        Assert.AreEqual("098f6bcd4621d373cade4e832627b4f6", result);
    }

    /// <summary>
    /// 测试 Lua 脚本引擎日期函数
    /// </summary>
    [TestMethod]
    public void TestLuaScriptEngine_DateFunctions()
    {
        using var engine = new LuaScriptEngine();

        var script = @"
            return date_now()
        ";

        var result = engine.Execute(script);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    /// <summary>
    /// 测试 Lua 脚本引擎正则函数
    /// </summary>
    [TestMethod]
    public void TestLuaScriptEngine_RegexFunctions()
    {
        using var engine = new LuaScriptEngine();

        var script = @"
            return regex_match('hello123world', '\\d+')
        ";

        var result = engine.Execute(script);

        Assert.AreEqual("123", result);
    }

    /// <summary>
    /// 测试 Lua 脚本引擎 Base64 函数
    /// </summary>
    [TestMethod]
    public void TestLuaScriptEngine_Base64Functions()
    {
        using var engine = new LuaScriptEngine();

        var script = @"
            local encoded = base64_encode('test')
            local decoded = base64_decode(encoded)
            return decoded
        ";

        var result = engine.Execute(script);

        Assert.AreEqual("test", result);
    }

    /// <summary>
    /// 测试 Lua 脚本引擎 SHA256 函数
    /// </summary>
    [TestMethod]
    public void TestLuaScriptEngine_Sha256Function()
    {
        using var engine = new LuaScriptEngine();

        var script = @"
            return sha256('test')
        ";

        var result = engine.Execute(script);

        Assert.IsNotNull(result);
        Assert.AreEqual(64, result.Length); // SHA256 产生 64 个十六进制字符
    }

    /// <summary>
    /// 测试 Lua 脚本引擎 JSON 序列化
    /// </summary>
    [TestMethod]
    public void TestLuaScriptEngine_JsonStringify()
    {
        using var engine = new LuaScriptEngine();

        var script = @"
            local obj = {name='test', value=123}
            return json_stringify(obj)
        ";

        var result = engine.Execute(script);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }
}
