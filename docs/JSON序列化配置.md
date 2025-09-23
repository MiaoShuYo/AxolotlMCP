# JSON 序列化配置（JsonDefaults）

本文档详细说明统一 JSON 序列化配置的设计与实现方式，以及在各项目中的使用方法，确保序列化/反序列化行为一致。

## 目标
- 统一大小写策略：统一使用小驼峰（camelCase）。
- 减少冗余：忽略空值字段输出，降低 payload。
- 兼容性增强：
  - 允许读取字符串形式的数值（兼容部分生态）
  - 允许 JSON 注释、尾随逗号（提升可读性与容错性）
- 字符处理：不强制转义中文，便于日志与调试。
- 可扩展：后续支持 Source Generator（TypeInfoResolver）进一步优化性能。

## 实现位置
- 代码文件：`src/AxolotlMCP.Core/Protocol/JsonDefaults.cs`
- 类型：`public static class JsonDefaults`

## 配置项说明
- PropertyNamingPolicy = `JsonNamingPolicy.CamelCase`
- DefaultIgnoreCondition = `JsonIgnoreCondition.WhenWritingNull`
- Encoder = `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`
- ReadCommentHandling = `JsonCommentHandling.Skip`
- AllowTrailingCommas = `true`
- NumberHandling = `JsonNumberHandling.AllowReadingFromString`
- WriteIndented = `false`

## 使用方式
- 序列化：`JsonSerializer.Serialize(obj, JsonDefaults.Options)`
- 反序列化：`JsonSerializer.Deserialize<T>(json, JsonDefaults.Options)`

## 设计权衡
- 忽略空值：减少带宽，但需要在客户端注意默认值处理。
- 允许注释与尾逗号：提升配置文件可读性，但生产环境建议保持规范化输出。
- 不转义中文：提升可读性；如需严格转义可在上层覆盖选项。

## 后续扩展
- 接入 `JsonTypeInfoResolver` 源生成，减少运行时反射开销。
- 针对大消息启用自定义 `MemoryPool`/`ArrayPool` 优化（按需）。
