using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

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
}