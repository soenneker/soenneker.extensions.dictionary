using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Diagnostics.CodeAnalysis;
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
        foreach (KeyValuePair<TKey, IList<TValue>> kvp in dictionary)
        {
            if (kvp.Value is not null)
                totalCapacity += kvp.Value.Count;
        }

        // Create the result list with the pre-calculated capacity
        var result = new List<TValue>(totalCapacity);

        // Populate the result list with values from the dictionary
        foreach (KeyValuePair<TKey, IList<TValue>> kvp in dictionary)
        {
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
    /// <returns>Flattens all of the values in a dictionary and returns a new list with all of them.</returns>
    [Pure]
    public static List<TValue> ToFlattenedValuesList<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary) where TKey : notnull
    {
        // Pre-calculate total capacity for the resulting list
        var totalCapacity = 0;
        foreach (KeyValuePair<TKey, List<TValue>> kvp in dictionary)
        {
            if (kvp.Value is not null)
                totalCapacity += kvp.Value.Count;
        }

        // Create the result list with the pre-calculated capacity
        var result = new List<TValue>(totalCapacity);

        // Populate the result list with values from the dictionary
        foreach (KeyValuePair<TKey, List<TValue>> kvp in dictionary)
        {
            if (kvp.Value is not null)
            {
                // Use AddRange to minimize per-item calls
                result.AddRange(kvp.Value);
            }
        }

        return result;
    }


    /// <summary>
    /// Adds or updates values by compiling the supplied key selector once for this call.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<TValue> toAdd, Expression<Func<TValue, TKey>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        source.AddRangeCompiled(toAdd, selector.Compile());
    }

    /// <summary>
    /// Adds or updates values using a precompiled key selector. Prefer this overload in repeated or hot paths.
    /// </summary>
    public static void AddRangeCompiled<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<TValue> toAdd, Func<TValue, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(toAdd);
        ArgumentNullException.ThrowIfNull(keySelector);

        foreach (TValue item in toAdd)
        {
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
        foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
        {
            source[kvp.Key] = kvp.Value; // Adds or updates the key-value pair
        }
    }

    /// <summary>
    /// Iterates through each one of the keys in the dictionary to build a new T by looking up property names, and setting that to value of the key value pair.
    /// </summary>
    /// <returns>Iterates through each one of the keys in the dictionary to build a new T by looking up property names, and setting that to value of the key value pair.</returns>
    [Pure]
    public static T ToObject<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(this IDictionary<string, object> source) where T : class, new()
    {
        // Create an instance of the target type
        var someObject = new T();

        Dictionary<string, PropertyInfo> properties = DictionaryPropertyMap<T>.Value;

        // Iterate through the dictionary
        foreach (KeyValuePair<string, object> item in source)
        {
            if (!properties.TryGetValue(item.Key, out PropertyInfo? property))
                continue;

            Type propertyType = property.PropertyType;
            object? value = item.Value;
            if (value is null)
            {
                if (!propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) is not null)
                    property.SetValue(someObject, null, BindingFlags.DoNotWrapExceptions, null, null, null);
            }
            else if (propertyType.IsInstanceOfType(value))
                property.SetValue(someObject, value, BindingFlags.DoNotWrapExceptions, null, null, null);
            else if (TryConvertValue(value, propertyType, out object? convertedValue))
                property.SetValue(someObject, convertedValue, BindingFlags.DoNotWrapExceptions, null, null, null);
        }

        return someObject;
    }

    /// <summary>Maps dictionary entries with explicit setters, without discovering model members.</summary>
    public static T ToObject<T>(this IDictionary<string, object> source, IReadOnlyDictionary<string, Action<T, object?>> setters) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setters);
        var result = new T();
        foreach (KeyValuePair<string, object> entry in source)
            if (setters.TryGetValue(entry.Key, out Action<T, object?>? setter))
                setter(result, entry.Value);
        return result;
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
        catch (InvalidCastException)
        {
            convertedValue = null;
            return false;
        }
        catch (FormatException)
        {
            convertedValue = null;
            return false;
        }
        catch (OverflowException)
        {
            convertedValue = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to retrieve a key from a particular value in the dictionary. If there are multiple of the same value, it returns the first key.
    /// </summary>
    /// <returns>Tries to retrieve a key from a particular value in the dictionary. If there are multiple of the same value, it returns the first key.</returns>
    [Pure]
    public static bool TryGetKeyFromValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, out TKey? result) where TValue : class
    {
        // Iterate through the dictionary
        foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
        {
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
