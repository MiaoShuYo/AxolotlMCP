using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AxolotlMCP.Core.Protocol;

/// <summary>
/// 通过反射从 DTO 生成简单 JSON Schema。
/// 支持：基础类型、枚举、数组、对象属性，[Required] 必填。
/// </summary>
public static class JsonSchemaGenerator
{
    /// <summary>
    /// 从类型生成 Schema。
    /// </summary>
    public static JsonSchema Generate(Type type)
    {
        type = UnwrapNullable(type);

        if (type.IsEnum)
        {
            return new JsonSchema
            {
                Type = "string",
                Enum = Enum.GetNames(type)
            };
        }

        if (type == typeof(string)) return new JsonSchema { Type = "string" };
        if (type == typeof(bool)) return new JsonSchema { Type = "boolean" };
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte)) return new JsonSchema { Type = "integer" };
        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return new JsonSchema { Type = "number" };

        if (type.IsArray)
        {
            return new JsonSchema
            {
                Type = "array",
                Items = Generate(type.GetElementType()!)
            };
        }

        if (IsEnumerable(type, out var elementType))
        {
            return new JsonSchema
            {
                Type = "array",
                Items = Generate(elementType!)
            };
        }

        // object: public 可写属性
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetMethod != null && p.SetMethod != null)
            .ToArray();
        var properties = new Dictionary<string, JsonSchema>();
        var required = new List<string>();
        foreach (var p in props)
        {
            properties[p.Name] = Generate(p.PropertyType);
            if (p.GetCustomAttribute<RequiredAttribute>() != null)
            {
                required.Add(p.Name);
            }
        }
        return new JsonSchema
        {
            Type = "object",
            Properties = properties.Count > 0 ? properties : null,
            Required = required.Count > 0 ? required.ToArray() : null
        };
    }

    private static Type UnwrapNullable(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Nullable.GetUnderlyingType(t)!;
        }
        return t;
    }

    private static bool IsEnumerable(Type t, out Type? elementType)
    {
        if (t.IsArray)
        {
            elementType = t.GetElementType();
            return true;
        }
        var ienum = t.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (ienum != null)
        {
            elementType = ienum.GetGenericArguments()[0];
            return true;
        }
        elementType = null;
        return false;
    }
}


