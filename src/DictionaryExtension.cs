using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Soenneker.Extensions.Dictionary;

/// <summary>
/// A collection of helpful Dictionary extension methods
/// </summary>
public static class DictionaryExtension
{
    /// <summary>
    /// Flattens the values of a dictionary, where each key maps to a list of values, into a single list.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to flatten.</param>
    /// <returns>A single list containing all values from the dictionary.</returns>
    [Pure]
    public static List<TValue> ToFlattenedValuesList<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictionary) where TKey : notnull
    {
        // Pre-calculate the total capacity for the resulting list
        var totalCapacity = 0;
        using IEnumerator<KeyValuePair<TKey, IList<TValue>>> enumerator = dictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            KeyValuePair<TKey, IList<TValue>> kvp = enumerator.Current;
            if (kvp.Value is not null)
                totalCapacity += kvp.Value.Count;
        }

        // Create the result list with the pre-calculated capacity
        var result = new List<TValue>(totalCapacity);

        // Populate the result list with values from the dictionary
        using IEnumerator<KeyValuePair<TKey, IList<TValue>>> valueEnumerator = dictionary.GetEnumerator();
        while (valueEnumerator.MoveNext())
        {
            KeyValuePair<TKey, IList<TValue>> kvp = valueEnumerator.Current;
            if (kvp.Value is not null)
            {
                // Add items manually to avoid calling AddRange (avoids potential overhead in large datasets)
                for (var i = 0; i < kvp.Value.Count; i++)
                {
                    result.Add(kvp.Value[i]);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Flattens all of the values in a dictionary and returns a new list with all of them
    /// </summary>
    [Pure]
    public static List<TValue> ToFlattenedValuesList<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary) where TKey : notnull
    {
        // Pre-calculate total capacity for the resulting list
        var totalCapacity = 0;
        using IEnumerator<KeyValuePair<TKey, List<TValue>>> enumerator = dictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            KeyValuePair<TKey, List<TValue>> kvp = enumerator.Current;
            if (kvp.Value is not null)
                totalCapacity += kvp.Value.Count;
        }

        // Create the result list with the pre-calculated capacity
        var result = new List<TValue>(totalCapacity);

        // Populate the result list with values from the dictionary
        using IEnumerator<KeyValuePair<TKey, List<TValue>>> valueEnumerator = dictionary.GetEnumerator();
        while (valueEnumerator.MoveNext())
        {
            KeyValuePair<TKey, List<TValue>> kvp = valueEnumerator.Current;
            if (kvp.Value is not null)
            {
                // Use AddRange to minimize per-item calls
                result.AddRange(kvp.Value);
            }
        }

        return result;
    }


    /// <summary>
    /// Adds (or updates!) an enumerable to a dictionary without a loop in managed code. <para/>
    /// Compiles the expression and loops over the enumerable, adding to the dictionary via the expression selector.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<TValue> toAdd, Expression<Func<TValue, TKey>> selector)
    {
        Func<TValue, TKey> keySelector = selector.Compile();

        using IEnumerator<TValue> enumerator = toAdd.GetEnumerator();
        while (enumerator.MoveNext())
        {
            TValue item = enumerator.Current;

            // Compute the key and add/update the dictionary
            TKey key = keySelector(item);
            source[key] = item;
        }
    }


    /// <summary>
    /// Loops over the target and adds each of the items into the source. Useful for readonly scenarios.
    /// </summary>
    public static void AddDictionary<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> dictionary)
    {
        using IEnumerator<KeyValuePair<TKey, TValue>> enumerator = dictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            KeyValuePair<TKey, TValue> kvp = enumerator.Current;
            source[kvp.Key] = kvp.Value; // Adds or updates the key-value pair
        }
    }

    /// <summary>
    /// Iterates through each one of the keys in the dictionary to build a new T by looking up property names, and setting that to value of the key value pair.
    /// </summary>
    [Pure]
    public static T ToObject<T>(this IDictionary<string, object> source) where T : class, new()
    {
        // Create an instance of the target type
        var someObject = new T();
        Type someObjectType = typeof(T);

        // Cache the properties for the type
        var properties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (PropertyInfo property in someObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.CanWrite) // Only consider writable properties
            {
                properties[property.Name] = property;
            }
        }

        // Iterate through the dictionary using an enumerator
        using IEnumerator<KeyValuePair<string, object>> enumerator = source.GetEnumerator();

        while (enumerator.MoveNext())
        {
            KeyValuePair<string, object> item = enumerator.Current;

            if (!properties.TryGetValue(item.Key, out PropertyInfo? property))
                continue;

            // Attempt to set the value if types match or can be converted
            if (item.Value is null || property.PropertyType.IsInstanceOfType(item.Value))
            {
                property.SetValue(someObject, item.Value);
            }
            else if (TryConvertValue(item.Value, property.PropertyType, out object? convertedValue))
            {
                property.SetValue(someObject, convertedValue);
            }
        }

        return someObject;
    }

    /// <summary>
    /// Attempts to convert a value to the target type.
    /// </summary>
    private static bool TryConvertValue(object value, Type targetType, out object? convertedValue)
    {
        try
        {
            convertedValue = Convert.ChangeType(value, targetType);
            return true;
        }
        catch
        {
            convertedValue = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to retrieve a key from a particular value in the dictionary. If there are multiple of the same value, it returns the first key.
    /// </summary>
    [Pure]
    public static bool TryGetKeyFromValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, out TKey? result) where TValue : class
    {
        // Get the enumerator for the dictionary
        using IEnumerator<KeyValuePair<TKey, TValue>> enumerator = dictionary.GetEnumerator();

        // Iterate using the enumerator
        while (enumerator.MoveNext())
        {
            KeyValuePair<TKey, TValue> kvp = enumerator.Current;

            // Check for matching value using reference equality first, then value equality
            if (ReferenceEquals(kvp.Value, value) || (kvp.Value?.Equals(value) ?? false))
            {
                result = kvp.Key;
                return true;
            }
        }

        // If no match is found, set result to default
        result = default;
        return false;
    }
}