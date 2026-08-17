# JSON 序列化紧凑格式设计

## 背景

当前项目中日志和 API 返回默认使用缩进格式的 JSON（`WriteIndented = true`），导致：
- 日志文件体积较大
- API 响应数据量较大

## 目标

将日志和 API 返回的 JSON 格式改为紧凑格式，减小数据体积。

## 修改内容

### 1. 日志序列化

**文件**：`LuBan.Logging\Serialization\LuBanJsonSerializer.cs`

**修改**：
```csharp
// 第 15 行
WriteIndented = false  // 原值: true
```

### 2. API 返回

**文件**：`LuBan.Web.Core\AspNetCore\ApiConfiguration.cs`

**修改**：
```csharp
// 第 60 行
options.JsonSerializerOptions.WriteIndented = false;  // 原值: true
```

## 不修改的内容

- `SerializeUtil.Serialize` 的 `indented` 参数已默认为 `false`
- `ToJson` 扩展方法的 `hasIndentation` 参数保持 `true`（供手动调用时使用）

## 影响

| 方面 | 影响 |
|------|------|
| 日志文件 | 体积减小，可读性略微下降 |
| API 响应 | 体积减小，更适合生产环境 |
| 开发调试 | 可通过工具格式化 JSON 查看 |

## 验证

- 检查日志输出是否为紧凑格式
- 检查 API 响应是否为紧凑格式
- 确认序列化功能正常