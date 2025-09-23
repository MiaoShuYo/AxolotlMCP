# DTO → JsonSchema 自动生成

提供 `JsonSchemaGenerator`（反射版），从 DTO 生成基础 JsonSchema。

## 支持范围
- 基础类型：string/boolean/integer/number
- Enum：输出为字符串类型并含 `enum`
- Array/List：生成 `type=array` 与 `items`
- Object：公共可写属性映射到 `properties`
- 必填：识别 `[Required]` 写入 `required`

## 使用示例
```
var schema = JsonSchemaGenerator.Generate(typeof(MyDto));
```

## 约束与后续
- 暂未支持字典、联合类型、自定义格式与注释扩展。
- 后续可引入 Source Generator 提升性能与可维护性。
