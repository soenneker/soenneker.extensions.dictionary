using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Soenneker.Extensions.Dictionary;

/// <summary>
/// A collection of helpful Dictionary extension methods
/// </summary>
public static class DictionaryExtension
{
    /// <inheritdoc cref="ToFlattenedValuesList{TKey,TValue}(IDictionary{TKey,List{TValue}})"/>
    [Pure]
    public static List<TValue> ToFlattenedValuesList<TKey, TValue>(this IDictionary<TKey, IList<TValue>> value) where TKey : notnull
    {
        List<TValue> result = value.SelectMany(item => item.Value).ToList();
        return result;
    }

    /// <summary>
    /// Flattens all of the values in a dictionary and returns a new list with all of them
    /// </summary>
    [Pure]
    public static List<TValue> ToFlattenedValuesList<TKey, TValue>(this IDictionary<TKey, List<TValue>> value) where TKey : notnull
    {
        List<TValue> result = value.SelectMany(item => item.Value).ToList();
        return result;
    }

    /// <summary>
    /// Loops over the target and adds each of the items into the source. Useful for readonly scenarios.
    /// </summary>
    public static void AddDictionary<TKey, TValue>(this IDictionary<TKey, TValue> value, IDictionary<TKey, TValue> dictionary)
    {
        foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
        {
            value.Add(kvp);
        }
    }

    /// <summary>
    /// Iterates through each one of the keys in the dictionary to build a new T by looking up property names, and setting that to value of the key value pair.
    /// </summary>
    [Pure]
    public static T ToObject<T>(this IDictionary<string, object> source) where T : class, new()
    {
        var someObject = new T();
        Type someObjectType = someObject.GetType();

        foreach (KeyValuePair<string, object> item in source)
        {
            PropertyInfo? property = someObjectType.GetProperty(item.Key);

            if (property != null)
                property.SetValue(someObject, item.Value, null);
        }

        return someObject;
    }
}