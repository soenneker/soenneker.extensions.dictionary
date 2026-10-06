using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Soenneker.Extensions.Dictionary;

internal static class DictionaryPropertyMap<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    internal static readonly Dictionary<string, PropertyInfo> Value = Create();

    private static Dictionary<string, PropertyInfo> Create()
    {
        var result = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            MethodInfo? setter = property.SetMethod;
            if (setter is null || !setter.IsPublic || property.GetIndexParameters().Length != 0 ||
                Array.IndexOf(setter.ReturnParameter.GetRequiredCustomModifiers(), typeof(IsExternalInit)) >= 0)
                continue;
            result[property.Name] = property;
        }
        return result;
    }
}
