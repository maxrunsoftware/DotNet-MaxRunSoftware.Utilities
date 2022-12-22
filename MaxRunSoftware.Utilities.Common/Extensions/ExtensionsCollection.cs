﻿// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsCollection
{
    #region Misc

    /// <summary>
    /// Wraps this object instance into an IEnumerable&lt;T&gt; consisting of a single item.
    /// https://stackoverflow.com/q/1577822
    /// </summary>
    /// <typeparam name="T">Type of the object.</typeparam>
    /// <param name="item">The instance that will be wrapped.</param>
    /// <returns>An IEnumerable&lt;T&gt; consisting of a single item.</returns>
    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }

    /// <summary>
    /// Concatenates an item to the end of a sequence
    /// </summary>
    /// <typeparam name="T">The type of the elements of the input sequence</typeparam>
    /// <param name="enumerable">The sequence to add an item to</param>
    /// <param name="item">The item to append to the end of the sequence</param>
    /// <returns>
    /// An System.Collections.Generic.IEnumerable`1 that contains the concatenated elements of the enumerable with the
    /// item appended to the end
    /// </returns>
    public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T item) => enumerable.Concat(item.Yield());

    #region IsEmpty

    public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is IReadOnlyCollection<T> readOnlyCollection) return readOnlyCollection.Count < 1;

        if (enumerable is ICollection<T> collection) return collection.Count < 1;

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (enumerable is Array array) return array.Length < 1;

        return !enumerable.Any();
    }

    public static bool IsEmpty<T>(this Span<T> span) => span.Length < 1;

    public static bool IsNotEmpty<T>(this IEnumerable<T> enumerable) => !enumerable.IsEmpty();

    public static bool IsNotEmpty<T>(this Span<T> span) => !span.IsEmpty();

    #endregion IsEmpty

    public static T[] Copy<T>(this T[] array)
    {
        //if (array == null) return null;

        var len = array.Length;
        var arrayNew = new T[len];

        Array.Copy(array, arrayNew, len);

        return arrayNew;
    }

    public static T[] Append<T>(this T[] array, T[] otherArray)
    {
        var result = new T[array.Length + otherArray.Length];

        //Buffer.BlockCopy(array, 0, result, 0, array.Length);
        //Buffer.BlockCopy(otherArray, 0, result, array.Length, otherArray.Length);
        for (var i = 0; i < array.Length; i++) result[i] = array[i];

        for (var i = 0; i < otherArray.Length; i++) result[array.Length + i] = otherArray[i];

        return result;
    }

    public static List<T> ToListReversed<T>(this IEnumerable<T> enumerable)
    {
        var l = enumerable.ToList();
        l.Reverse();
        return l;
    }

    /// <summary>
    /// Casts each item to type T
    /// </summary>
    /// <typeparam name="T">The type to cast to</typeparam>
    /// <param name="enumerable">The enumerable to cast</param>
    /// <returns>A list containing all of the casted elements of the enumerable</returns>
    public static List<T> ToList<T>(this CollectionBase enumerable)
    {
        var l = new List<T>();
        foreach (var o in enumerable)
        {
            var t = (T)o;
            l.Add(t);
        }

        return l;
    }

    /// <summary>
    /// Processes an action on each element in an enumerable
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="value">The enumerable to process</param>
    /// <param name="action">The action to execute on each item in enumerable</param>
    public static void ForEach<T>(this IEnumerable<T> value, Action<T> action)
    {
        foreach (var item in value) action(item);
    }

    /// <summary>
    /// If the array is null then returns an empty array
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="array">The array to check for null</param>
    /// <returns>The same array or if null an empty array</returns>
    public static T[] OrEmpty<T>(this T[]? array) => array ?? Array.Empty<T>();

    /// <summary>
    /// If the enumerable is null then returns an empty enumerable
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="enumerable">The enumerable to check for null</param>
    /// <returns>The same enumerable or if null an empty enumerable</returns>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? enumerable) => enumerable ?? Enumerable.Empty<T>();

    public static IReadOnlyCollection<T> OrEmpty<T>(this IReadOnlyCollection<T>? list) => list ?? EmptyReadOnlyList<T>.Instance;

    public static IReadOnlyList<T> OrEmpty<T>(this IReadOnlyList<T>? list) => list ?? EmptyReadOnlyList<T>.Instance;

    /// <summary>
    /// If the string is null then returns an empty string
    /// </summary>
    /// <param name="str">The string to check for null</param>
    /// <returns>The same string or an empty string if null</returns>
    public static string OrEmpty(this string? str) => str ?? string.Empty;

    public static T? DequeueOrDefault<T>(this Queue<T> queue) => queue.Count < 1 ? default : queue.Dequeue();

    public static void Populate<T>(this T[] array, T value)
    {
        for (var i = 0; i < array.Length; i++) array[i] = value;
    }

    public static List<object> ToList(this CollectionBase collection)
    {
        var list = new List<object>();
        foreach (var o in collection) list.Add(o);
        return list;
    }

    public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> enumerable) => new(enumerable);

    public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> enumerable, IComparer<T> comparer) => new(enumerable, comparer);

    public static int GetNumberOfCharacters(this IEnumerable<string?> array, int lengthOfNull = 0) => array.Sum(s => s?.Length ?? lengthOfNull);

    public static IEnumerable<string> ToStringsColumns(this IEnumerable<string> enumerable, int numberOfColumns, string paddingBetweenColumns = "   ", bool rightAlign = false)
    {
        var items = enumerable.ToArray();
        var width = items.MaxLength();

        var itemParts = items.SplitIntoPartSizes(numberOfColumns);
        var lines = new List<string>();
        foreach (var item in itemParts)
        {
            var list = new List<string>();
            for (var i = 0; i < numberOfColumns; i++)
            {
                var part = item.GetAtIndexOrDefault(i) ?? string.Empty;
                part = rightAlign ? part.PadLeft(width) : part.PadRight(width);
                list.Add(part);
            }

            var s = list.ToStringDelimited(paddingBetweenColumns);
            lines.Add(s);
        }

        return lines;
    }

    public static IEnumerable<T> MinusHead<T>(this IEnumerable<T> enumerable)
    {
        var first = true;
        foreach (var item in enumerable)
        {
            if (first)
                first = false;
            else
                yield return item;
        }
    }

    public static T? FirstOrNull<T>(this IEnumerable<T> enumerable) where T : struct
    {
        foreach (var o in enumerable) return o;

        return null;
    }

    public static IEnumerable<List<T?>> ToListChunked<T>(this IEnumerable<T?> enumerable, int chunkSize)
    {
        if (chunkSize < 1) chunkSize = 1;
        var list = new List<T?>(chunkSize);

        foreach (var item in enumerable)
        {
            list.Add(item);
            if (list.Count < chunkSize) continue;
            var result = list;
            list = new List<T?>(chunkSize);
            yield return result;
        }

        if (list.Count > 0)
        {
            yield return list;
        }
    }

    #endregion Misc

    #region Multidimensional Arrays

    public static List<T[]> ToList<T>(this T[,] array, ushort listPadding = 0)
    {
        // TODO: I'm sure there are tons of performance tweaks for this such as using Array.Copy somehow.
        var lenRows = array.CheckNotNull(nameof(array)).GetLength(0);
        var lenColumns = array.GetLength(1);

        var list = new List<T[]>(lenRows + listPadding);

        for (var i = 0; i < lenRows; i++)
        {
            var newRow = new T[lenColumns];
            for (var j = 0; j < lenColumns; j++) newRow[j] = array[i, j];

            list.Add(newRow);
        }

        return list;
    }

    public static T[,] InsertRow<T>(this T[,] array, int index, T[] values)
    {
        var lenColumns = array.GetLength(1);

        if (values.Length != lenColumns) throw new ArgumentException(nameof(values), "Values of length " + values.Length + " does not match row size of " + lenColumns + ".");

        var list = array.ToList(1);
        list.Insert(index, values);

        var lenRows = list.Count;
        var newArray = new T[lenRows, lenColumns];

        for (var i = 0; i < lenRows; i++)
        {
            var rr = list[i];
            for (var j = 0; j < lenColumns; j++) newArray[i, j] = rr[j];
        }

        return newArray;
    }

    public static T[] GetRow<T>(this T[,] array, int index)
    {
        var lenRows = array.CheckNotNull(nameof(array)).GetLength(0);

        var lenColumns = array.GetLength(1);

        if ((uint)index >= (uint)lenRows) throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the array rows.");

        if (typeof(T).IsPrimitive) // https://stackoverflow.com/a/27440355
        {
            var cols = array.GetUpperBound(1) + 1;
            var result = new T[cols];
            int size;
            if (typeof(T) == typeof(bool))
                size = 1;
            else if (typeof(T) == typeof(char))
                size = 2;
            else
                size = Marshal.SizeOf<T>();

            Buffer.BlockCopy(array, index * cols * size, result, 0, cols * size);
            return result;
        }

        var newArray = new T[lenColumns];
        for (var i = 0; i < lenColumns; i++) newArray[i] = array[index, i];

        return newArray;
    }

    public static void SetRow<T>(this T[,] array, int index, T[] values)
    {
        values.CheckNotNull(nameof(values));
        var lenRows = array.CheckNotNull(nameof(array)).GetLength(0);

        var lenColumns = array.GetLength(1);

        if ((uint)index >= (uint)lenRows) throw new ArgumentOutOfRangeException(nameof(index), "Index " + index + " was out of range. Must be non-negative and less than the size of the array rows " + lenRows + ".");

        if (values.Length != lenColumns) throw new ArgumentException(nameof(values), "Values of length " + values.Length + " does not match row size of " + lenColumns + ".");

        for (var i = 0; i < lenColumns; i++) array[index, i] = values[i];
    }

    public static T[] GetColumn<T>(this T[,] array, int index)
    {
        var lenRows = array.CheckNotNull(nameof(array)).GetLength(0);

        var lenColumns = array.GetLength(1);

        if ((uint)index >= (uint)lenColumns) throw new ArgumentOutOfRangeException(nameof(index), "Index " + index + " was out of range. Must be non-negative and less than the size of the array columns " + lenColumns + ".");

        var newArray = new T[lenRows];
        for (var i = 0; i < lenRows; i++) newArray[i] = array[index, i];

        return newArray;
    }

    public static void SetColumn<T>(this T[,] array, int index, T[] values)
    {
        values.CheckNotNull(nameof(values));
        var lenRows = array.CheckNotNull(nameof(array)).GetLength(0);

        var lenColumns = array.GetLength(1);

        if ((uint)index >= (uint)lenColumns) throw new ArgumentOutOfRangeException(nameof(index), "Index " + index + " was out of range. Must be non-negative and less than the size of the array columns " + lenColumns + ".");

        if (values.Length != lenRows) throw new ArgumentException(nameof(values), "Values of length " + values.Length + " does not match column size of " + lenRows + ".");

        for (var i = 0; i < lenRows; i++) array[i, index] = values[i];
    }

    #endregion Multidimensional Arrays

    #region Split

    public static T[][] SplitIntoParts<T>(this T[] array, int numberOfParts)
    {
        if (numberOfParts < 1) numberOfParts = 1;

        var arraySize = array.Length;
        var div = arraySize / numberOfParts;
        var remainder = arraySize % numberOfParts;

        var partSizes = new int[numberOfParts];
        for (var i = 0; i < numberOfParts; i++) partSizes[i] = div + (i < remainder ? 1 : 0);

        var newArray = new T[numberOfParts][];
        var counter = 0;
        for (var i = 0; i < numberOfParts; i++)
        {
            var partSize = partSizes[i];
            var newArrayItem = new T[partSize];
            Array.Copy(array, counter, newArrayItem, 0, partSize);
            counter += partSize;
            newArray[i] = newArrayItem;
        }

        return newArray;
    }

    public static T[][] SplitIntoPartSizes<T>(this T[] array, int partSize) => SplitIntoParts(array, array.Length / partSize);

    public static (T[], T[]) Split<T>(this T[] array, int index)
    {
        var array1 = new T[index];
        var array2 = new T[array.Length - index];

        //Buffer.BlockCopy(array, 0, array1, 0, array1.Length);
        //Buffer.BlockCopy(array, index, array2, 0, array2.Length);

        for (var i = 0; i < array1.Length; i++) array1[i] = array[i];

        for (var i = 0; i < array2.Length; i++) array2[i] = array[array1.Length + i];


        return (array1, array2);
    }

    #endregion Split

    #region Remove

    public static T[] RemoveAt<T>(this T[] array, int index)
    {
        var len = array.CheckNotNull(nameof(array)).Length;
        if ((uint)index >= (uint)len) throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the collection.");

        var newArray = new T[len - 1];
        var j = 0;
        for (var i = 0; i < len; i++)
        {
            if (i != index)
            {
                newArray[j] = array[i];
                j++;
            }
        }

        return newArray;
    }

    public static T[] RemoveHead<T>(this T[] array) => RemoveAt(array, 0);

    public static T[] RemoveHead<T>(this T[] array, int itemsToRemove)
    {
        for (var i = 0; i < itemsToRemove; i++) array = array.RemoveHead();

        return array;
    }

    public static T[] RemoveTail<T>(this T[] array) => RemoveAt(array, array.Length - 1);

    public static T[] RemoveTail<T>(this T[] array, int itemsToRemove)
    {
        for (var i = 0; i < itemsToRemove; i++) array = array.RemoveTail();

        return array;
    }

    #endregion Remove

    #region EqualsAt

    public static bool EqualsAt<T>(this T[]? array1, T[]? array2, int index) => EqualsAt(array1, array2, index, null);

    public static bool EqualsAt<T>(this T[]? array1, T[]? array2, int index, IEqualityComparer<T>? comparer)
    {
        if (array1 == null) return false;

        if (array2 == null) return false;

        if (index >= array1.Length) return false;

        if (index >= array2.Length) return false;

        var item1 = array1[index];
        var item2 = array2[index];

        if (EqualityComparer<T>.Default.Equals(item1, default) && EqualityComparer<T>.Default.Equals(item2, default)) return true;

        if (EqualityComparer<T>.Default.Equals(item1, default)) return false;

        if (EqualityComparer<T>.Default.Equals(item2, default)) return false;

        comparer ??= EqualityComparer<T>.Default;

        return comparer.Equals(item1, item2);
    }

    public static bool EqualsAt<T>(this T[]? array, int index, T item) => EqualsAt(array, index, null, item);

    public static bool EqualsAt<T>(this T[]? array, int index, IEqualityComparer<T>? comparer, T item)
    {
        if (array == null) return false;

        if (index >= array.Length) return false;

        var o = array[index];

        if (EqualityComparer<T>.Default.Equals(o, default) && EqualityComparer<T>.Default.Equals(item, default)) return true;

        if (EqualityComparer<T>.Default.Equals(o, default)) return false;

        if (EqualityComparer<T>.Default.Equals(item, default)) return false;

        comparer ??= EqualityComparer<T>.Default;

        return comparer.Equals(o, item);
    }

    public static bool EqualsAtAny<T>(this T[]? array, int index, params T[]? items) => EqualsAtAny(array, index, null, items);

    public static bool EqualsAtAny<T>(this T[]? array, int index, IEqualityComparer<T>? comparer, params T[]? items)
    {
        if (array == null) return false;

        foreach (var item in items ?? Array.Empty<T>())
        {
            if (EqualsAt(array, index, comparer, item)) return true;
        }

        return false;
    }

    #endregion EqualsAt

    #region Resize

    /// <summary>
    /// Resizes an array and returns a new array
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="array">The array to resize</param>
    /// <param name="newLength">The length of the new array</param>
    /// <returns>A new array of length newLength</returns>
    public static T[] Resize<T>(this T[] array, int newLength)
    {
        var newArray = new T[newLength];

        var width = Math.Min(array.Length, newLength);
        for (var i = 0; i < width; i++) newArray[i] = array[i];

        return newArray;
    }

    public static void ResizeAll<T>(this IList<T[]?> list, int newLength)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] != null) list[i] = list[i]!.Resize(newLength);
        }
    }

    public static void ResizeAll<T>(this T[]?[] list, int newLength)
    {
        for (var i = 0; i < list.Length; i++)
        {
            if (list[i] != null) list[i] = list[i]!.Resize(newLength);
        }
    }

    #endregion Resize

    #region MaxLength

    /// <summary>
    /// Determines which array is longest and returns that array's length
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="enumerable">The enumerable to search</param>
    /// <returns>The size of the longest array</returns>
    public static int MaxLength<T>(this IEnumerable<T[]?> enumerable)
    {
        var len = 0;
        foreach (var item in enumerable)
        {
            if (item != null) len = Math.Max(len, item.Length);
        }

        return len;
    }

    /// <summary>
    /// Determines which string is longest and returns that string's length
    /// </summary>
    /// <param name="enumerable">The enumerable to search</param>
    /// <returns>The size of the longest string</returns>
    public static int MaxLength(this IEnumerable<string?> enumerable)
    {
        var len = 0;
        foreach (var item in enumerable)
        {
            if (item != null) len = Math.Max(len, item.Length);
        }

        return len;
    }

    /// <summary>
    /// Determines which collection is longest and returns that collection's length
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <typeparam name="TCollection">The type of collection</typeparam>
    /// <param name="enumerable">The enumerable to search</param>
    /// <returns>The size of the longest collection</returns>
    public static int MaxLength<T, TCollection>(this IEnumerable<TCollection?> enumerable) where TCollection : ICollection<T>
    {
        var len = 0;
        foreach (var item in enumerable)
        {
            if (item != null) len = Math.Max(len, item.Count);
        }

        return len;
    }

    #endregion MaxLength

    #region Dictionary

    public static Dictionary<TKey, TValue> Copy<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        var d = new Dictionary<TKey, TValue>(dictionary.Count, dictionary.Comparer);
        foreach (var kvp in dictionary) d.Add(kvp.Key, kvp.Value);

        return d;
    }

    public static TValue? GetValueCaseInsensitive<TValue>(this IDictionary<string, TValue> dictionary, string key) => TryGetValueCaseInsensitive(dictionary, key, out var val) ? val : default;

    public static bool TryGetValueCaseInsensitive<TValue>(this IDictionary<string, TValue> dictionary, string? key, out TValue? value)
    {
        if (key == null)
        {
            value = default;
            return false;
        }

        if (dictionary.TryGetValue(key, out value)) return true;

        if (dictionary.TryGetValue(key.ToLower(), out value)) return true;

        if (dictionary.TryGetValue(key.ToUpper(), out value)) return true;

        foreach (var sc in Constant.StringComparisons)
        {
            foreach (var kvp in dictionary)
            {
                if (string.Equals(kvp.Key, key, sc))
                {
                    value = kvp.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    #endregion Dictionary

    #region Add

    public static void AddMany<T>(this BlockingCollection<T> collection, IEnumerable<T> itemsToAdd)
    {
        foreach (var item in itemsToAdd) collection.Add(item);
    }

    public static void AddManyComplete<T>(this BlockingCollection<T> collection, IEnumerable<T> itemsToAdd)
    {
        AddMany(collection, itemsToAdd);
        collection.CompleteAdding();
    }

    public static T[] AppendHead<T>(this T[] array, params T[] itemsToAdd)
    {
        // TODO: Add overload for single item for performance

        var arrayNew = new T[array.Length + itemsToAdd.Length];
        for (var i = 0; i < itemsToAdd.Length; i++) arrayNew[i] = itemsToAdd[i];

        for (var i = 0; i < array.Length; i++) arrayNew[i + itemsToAdd.Length] = array[i];

        return arrayNew;
    }

    public static T[] AppendTail<T>(this T[] array, params T[] itemsToAdd)
    {
        // TODO: Add overload for single item for performance

        var arrayNew = new T[array.Length + itemsToAdd.Length];
        for (var i = 0; i < array.Length; i++) arrayNew[i] = array[i];

        for (var i = 0; i < itemsToAdd.Length; i++) arrayNew[i + array.Length] = itemsToAdd[i];

        return arrayNew;
    }

    public static void AddIfNotNull<T>(this ICollection<T> collection, T? item) where T : class
    {
        if (item != null) collection.Add(item);
    }

    public static string? AddIfNotNullTrimmed(this ICollection<string> collection, string? item)
    {
        var str = item.TrimOrNull();
        if (str != null) collection.Add(str);

        return str;
    }

    public static TCollection AddRange<TCollection, TItem>(this TCollection collection, params TItem[] items) where TCollection : ICollection<TItem>
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }

        return collection;
    }

    public static TCollection AddRange<TCollection>(this TCollection collection, params object?[] items) where TCollection : ICollection<object?>
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }

        return collection;
    }

    /*
    public static TItem AddReturnItem<TItem, TItemCollection, TCollection>(this TCollection collection, TItem item) where TCollection : ICollection<TItemCollection> where TItem : TItemCollection
    {
        collection.Add(item);
        return item;
    }

    public static TCollection AddReturn<TItem, TItemCollection, TCollection>(this TCollection collection, TItem item) where TCollection : ICollection<TItemCollection> where TItem : TItemCollection
    {
        collection.Add(item);
        return collection;
    }

    public static TDictionary AddReturn<TKey, TKeyDictionary, TValue, TValueDictionary, TDictionary>(this TDictionary dictionary, TKey key, TValue value) where TDictionary : IDictionary<TKeyDictionary, TValueDictionary> where TKey : TKeyDictionary where TValue : TValueDictionary
    {
        dictionary.Add(key, value);
        return dictionary;
    }

    public static TDictionary SetReturn<TKey, TKeyDictionary, TValue, TValueDictionary, TDictionary>(this TDictionary dictionary, TKey key, TValue value)  where TDictionary : IDictionary<TKeyDictionary, TValueDictionary> where TKey : TKeyDictionary where TValue : TValueDictionary
    {
        dictionary[key] = value;
        return dictionary;
    }
    */

    #endregion Add

    #region Contains

    public static bool ContainsOnly<T>(this IEnumerable<T> enumerable, ICollection<T> availableItems) => enumerable.All(availableItems.Contains);

    public static bool ContainsOnly<T>(this IEnumerable<T> enumerable, Func<T, bool> contains) => enumerable.All(contains);

    #endregion Contains

    #region In

    public static bool In<T>(this T value, T possibleValue1) => In(value, EqualityComparer<T>.Default, possibleValue1);

    public static bool In<T>(this T value, T possibleValue1, T possibleValue2) => In(value, EqualityComparer<T>.Default, possibleValue1, possibleValue2);

    public static bool In<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3) => In(value, EqualityComparer<T>.Default, possibleValue1, possibleValue2, possibleValue3);

    public static bool In<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4) => In(value, EqualityComparer<T>.Default, possibleValue1, possibleValue2, possibleValue3, possibleValue4);

    public static bool In<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5) => In(value, EqualityComparer<T>.Default, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5);

    public static bool In<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6) => In(value, EqualityComparer<T>.Default, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5, possibleValue6);

    public static bool In<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6, T possibleValue7) => In(value, EqualityComparer<T>.Default, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5, possibleValue6, possibleValue7);

    public static bool In<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6, T possibleValue7, T possibleValue8) => In(value, EqualityComparer<T>.Default, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5, possibleValue6, possibleValue7, possibleValue8);

    public static bool In<T>(this T value, IEnumerable<T> possibleValues) => In(value, EqualityComparer<T>.Default, possibleValues);

    public static bool In<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1) => equalityComparer.Equals(value, possibleValue1);

    public static bool In<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2) => equalityComparer.Equals(value, possibleValue1) || equalityComparer.Equals(value, possibleValue2);

    public static bool In<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3) => equalityComparer.Equals(value, possibleValue1) || equalityComparer.Equals(value, possibleValue2) || equalityComparer.Equals(value, possibleValue3);

    public static bool In<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4) => equalityComparer.Equals(value, possibleValue1) || equalityComparer.Equals(value, possibleValue2) || equalityComparer.Equals(value, possibleValue3) || equalityComparer.Equals(value, possibleValue4);

    public static bool In<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5) => equalityComparer.Equals(value, possibleValue1) || equalityComparer.Equals(value, possibleValue2) || equalityComparer.Equals(value, possibleValue3) || equalityComparer.Equals(value, possibleValue4) || equalityComparer.Equals(value, possibleValue5);

    public static bool In<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6) => equalityComparer.Equals(value, possibleValue1) || equalityComparer.Equals(value, possibleValue2) || equalityComparer.Equals(value, possibleValue3) || equalityComparer.Equals(value, possibleValue4) || equalityComparer.Equals(value, possibleValue5) || equalityComparer.Equals(value, possibleValue6);

    public static bool In<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6, T possibleValue7) => equalityComparer.Equals(value, possibleValue1) || equalityComparer.Equals(value, possibleValue2) || equalityComparer.Equals(value, possibleValue3) || equalityComparer.Equals(value, possibleValue4) || equalityComparer.Equals(value, possibleValue5) || equalityComparer.Equals(value, possibleValue6) || equalityComparer.Equals(value, possibleValue7);

    public static bool In<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6, T possibleValue7, T possibleValue8) => equalityComparer.Equals(value, possibleValue1) || equalityComparer.Equals(value, possibleValue2) || equalityComparer.Equals(value, possibleValue3) || equalityComparer.Equals(value, possibleValue4) || equalityComparer.Equals(value, possibleValue5) || equalityComparer.Equals(value, possibleValue6) || equalityComparer.Equals(value, possibleValue7) || equalityComparer.Equals(value, possibleValue8);

    public static bool In<T>(this T value, IEqualityComparer<T> comparer, IEnumerable<T> possibleValues) => possibleValues.Any(o => comparer.Equals(o, value));

    #endregion In

    #region NotIn

    public static bool NotIn<T>(this T value, T possibleValue1) => !In(value, possibleValue1);

    public static bool NotIn<T>(this T value, T possibleValue1, T possibleValue2) => !In(value, possibleValue1, possibleValue2);

    public static bool NotIn<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3) => !In(value, possibleValue1, possibleValue2, possibleValue3);

    public static bool NotIn<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4) => !In(value, possibleValue1, possibleValue2, possibleValue3, possibleValue4);

    public static bool NotIn<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5) => !In(value, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5);

    public static bool NotIn<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6) => !In(value, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5, possibleValue6);

    public static bool NotIn<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6, T possibleValue7) => !In(value, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5, possibleValue6, possibleValue7);

    public static bool NotIn<T>(this T value, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6, T possibleValue7, T possibleValue8) => !In(value, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5, possibleValue6, possibleValue7, possibleValue8);

    public static bool NotIn<T>(this T value, IEnumerable<T> possibleValues) => !In(value, possibleValues);

    public static bool NotIn<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1) => !In(value, equalityComparer, possibleValue1);

    public static bool NotIn<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2) => !In(value, equalityComparer, possibleValue1, possibleValue2);

    public static bool NotIn<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3) => !In(value, equalityComparer, possibleValue1, possibleValue2, possibleValue3);

    public static bool NotIn<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4) => !In(value, equalityComparer, possibleValue1, possibleValue2, possibleValue3, possibleValue4);

    public static bool NotIn<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5) => !In(value, equalityComparer, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5);

    public static bool NotIn<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6) => !In(value, equalityComparer, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5, possibleValue6);

    public static bool NotIn<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6, T possibleValue7) => !In(value, equalityComparer, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5, possibleValue6, possibleValue7);

    public static bool NotIn<T>(this T value, IEqualityComparer<T> equalityComparer, T possibleValue1, T possibleValue2, T possibleValue3, T possibleValue4, T possibleValue5, T possibleValue6, T possibleValue7, T possibleValue8) => !In(value, equalityComparer, possibleValue1, possibleValue2, possibleValue3, possibleValue4, possibleValue5, possibleValue6, possibleValue7, possibleValue8);

    public static bool NotIn<T>(this T value, IEqualityComparer<T> comparer, IEnumerable<T> possibleValues) => !In(value, comparer, possibleValues);

    #endregion NotIn

    #region GetAtIndexOrDefault

    public static T? GetAtIndexOrDefault<T>(this T[] array, int index, T? defaultValue)
    {
        array.CheckNotNull(nameof(array));

        if (index < 0) return defaultValue;

        if (index >= array.Length) return defaultValue;

        return array[index];
    }

    public static T? GetAtIndexOrDefault<T>(this T[] array, int index) => GetAtIndexOrDefault(array, index, default);

    public static T? GetAtIndexOrDefault<T>(this IList<T> list, int index, T? defaultValue)
    {
        list.CheckNotNull(nameof(list));

        if (index < 0) return defaultValue;

        if (index >= list.Count) return defaultValue;

        return list[index];
    }

    public static T? GetAtIndexOrDefault<T>(this IList<T> list, int index) => GetAtIndexOrDefault(list, index, default);

    public static T? GetAtIndexOrDefault<T>(this ICollection<T> collection, int index, T? defaultValue)
    {
        collection.CheckNotNull();

        if (index < 0) return defaultValue;

        if (index >= collection.Count) return defaultValue;

        var i = 0;
        foreach (var item in collection)
        {
            if (i == index) return item;

            i++;
        }

        return defaultValue;
    }

    public static T? GetAtIndexOrDefault<T>(this ICollection<T> collection, int index) => GetAtIndexOrDefault(collection, index, default);

    public static T? GetAtIndexOrDefault<T>(this IEnumerable<T> enumerable, int index, T? defaultValue)
    {
        enumerable.CheckNotNull();

        if (index < 0) return defaultValue;

        var i = 0;
        foreach (var item in enumerable)
        {
            if (i == index) return item;

            i++;
        }

        return defaultValue;
    }

    public static T? GetAtIndexOrDefault<T>(this IEnumerable<T> enumerable, int index) => GetAtIndexOrDefault(enumerable, index, default);

    #endregion GetAtIndexOrDefault

    #region WhereNotNull

    public static T[] WhereNotNull<T>(this T?[] array) where T : class
    {
        var newArray = new T[array.Count(t => t != null)];
        var newArrayIndex = 0;
        foreach (var t in array)
        {
            if (t != null) newArray[newArrayIndex++] = t;
        }

        return newArray;
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : class
    {
        foreach (var item in enumerable)
        {
            if (item != null) yield return item;
        }
    }

    public static T?[] WhereNotNull<T>(this T?[] array) where T : struct
    {
        var newArraySize = array.Count(t => t != null);

        var newArray = new T?[newArraySize];

        newArraySize = 0;
        foreach (var t in array)
        {
            if (t != null) newArray[newArraySize++] = t;
        }

        return newArray;
    }

    public static IEnumerable<T?> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : struct
    {
        foreach (var item in enumerable)
        {
            if (item != null) yield return item;
        }
    }

    #endregion WhereNotNull

    #region Pop

    public static T PopAt<T>(this IList<T> list, int index)
    {
        var o = list[index];
        list.RemoveAt(index);
        return o;
    }

    /// <summary>
    /// Removes the first item in an IList and returns that item
    /// </summary>
    /// <typeparam name="T">List type</typeparam>
    /// <param name="list">list</param>
    /// <returns>The item that was removed</returns>
    public static T PopHead<T>(this IList<T> list) => PopAt(list, 0);

    /// <summary>
    /// Removes the last item in an IList and returns that item
    /// </summary>
    /// <typeparam name="T">List type</typeparam>
    /// <param name="list">list</param>
    /// <returns>The item that was removed</returns>
    public static T PopTail<T>(this IList<T> list) => PopAt(list, list.Count - 1);

    #endregion Pop

    #region Dictionary

    public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull => new(dictionary);

    public static TValue? GetValueNullable<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : class => dictionary.TryGetValue(key, out var value) ? value : null;

    public static TValue? GetValueNullable<TKey, TValue>(this IDictionary<TKey, TValue?> dictionary, TKey key) where TValue : struct => dictionary.TryGetValue(key, out var value) ? value : null;

    public static TValue? GetValueNullable<TKey, TValue>(this IDictionary<TKey, TValue[]> dictionary, TKey key, int index) where TValue : class => dictionary.TryGetValue(key, out var value) ? value.GetAtIndexOrDefault(index) : null;

    public static TValue? GetValueNullable<TKey, TValue>(this IDictionary<TKey, TValue?[]> dictionary, TKey key, int index) where TValue : struct => dictionary.TryGetValue(key, out var value) ? value.GetAtIndexOrDefault(index) : null;

    public static TValue? GetValueNullable<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictionary, TKey key, int index) where TValue : class => dictionary.TryGetValue(key, out var value) ? value.GetAtIndexOrDefault(index) : null;

    public static TValue? GetValueNullable<TKey, TValue>(this IDictionary<TKey, IList<TValue?>> dictionary, TKey key, int index) where TValue : struct => dictionary.TryGetValue(key, out var value) ? value.GetAtIndexOrDefault(index) : null;

    public static TValue? GetValueNullable<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, int index) where TValue : class => dictionary.TryGetValue(key, out var value) ? value.GetAtIndexOrDefault(index) : null;

    public static TValue? GetValueNullable<TKey, TValue>(this IDictionary<TKey, List<TValue?>> dictionary, TKey key, int index) where TValue : struct => dictionary.TryGetValue(key, out var value) ? value.GetAtIndexOrDefault(index) : null;

    public static TValue? GetValueNullable<TKey, TValue>(this IDictionary<TKey, IReadOnlyList<TValue>> dictionary, TKey key, int index) where TValue : class => dictionary.TryGetValue(key, out var value) ? value.GetAtIndexOrDefault(index) : null;

    public static TValue? GetValueNullable<TKey, TValue>(this IDictionary<TKey, IReadOnlyList<TValue?>> dictionary, TKey key, int index) where TValue : struct => dictionary.TryGetValue(key, out var value) ? value.GetAtIndexOrDefault(index) : null;

    /// <summary>
    /// Adds an item to a dictionary list, and creates that list if it doesn't already exist.
    /// </summary>
    /// <typeparam name="TKey">K</typeparam>
    /// <typeparam name="TValue">V</typeparam>
    /// <param name="dictionary">Dictionary</param>
    /// <param name="key">Key</param>
    /// <param name="values">Values</param>
    /// <returns>True if a new list was created, otherwise false</returns>
    public static bool AddToList<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, params TValue[]? values)
    {
        if (values == null || values.Length < 1) return false;

        var listCreated = false;
        if (!dictionary.TryGetValue(key, out var list))
        {
            list = new List<TValue>();
            dictionary.Add(key, list);
            listCreated = true;
        }

        foreach (var value in values) list.Add(value);

        return listCreated;
    }

    public static bool AddToList<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, IEnumerable<TValue> values) => AddToList(dictionary, key, values.ToArray());

    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys, TValue value) => keys.ForEach(o => dictionary.Add(o, value));

    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, params TKey[] keys) => keys.ForEach(o => dictionary.Add(o, value));

    #endregion Dictionary

    #region Set

    public static void AddRange<T>(this ISet<T> set, IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable) set.Add(item);
    }

    public static void Add<T>(this ISet<T> set, T item1, T item2, params T[]? items)
    {
        set.Add(item1);
        set.Add(item2);
        if (items != null)
        {
            foreach (var item in items) { set.Add(item); }
        }
    }

    #endregion Set

    #region Enumerator

    public static bool TryGetNext<T>(this IEnumerator<T> enumerator, out T? obj)
    {
        var moveNext = enumerator.MoveNext();
        if (moveNext)
        {
            obj = enumerator.Current;
            return true;
        }

        obj = default;
        return false;
    }

    public static bool TryGetNext(this IEnumerator enumerator, out object? obj)
    {
        var moveNext = enumerator.MoveNext();
        if (moveNext)
        {
            obj = enumerator.Current;
            return true;
        }

        obj = default;
        return false;
    }

    #endregion Enumerator

    #region ToImmutable

    public static ImmutableArray<T> AsImmutableArray<T>(this IEnumerable<T> enumerable) => ImmutableArray.Create(enumerable.OrEmpty().ToArray());

    public static ImmutableList<T> AsImmutableList<T>(this IEnumerable<T> enumerable) => ImmutableList.Create(enumerable.OrEmpty().ToArray());

    #endregion ToImmutable

    #region CompareTo

    /// <summary>
    /// Performs lexical comparison of 2 IEnumerable collections holding elements of type T.
    /// https://stackoverflow.com/a/18211470
    /// </summary>
    /// <typeparam name="T">Type of collection elements.</typeparam>
    /// <param name="obj">The first collection to compare.</param>
    /// <param name="other">The second collection to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of a and b:
    /// Less than zero: first is less than second;
    /// Zero: first is equal to second;
    /// Greater than zero: first is greater than second.
    /// </returns>
    /// <remarks>
    /// Can be called as either static method: EnumerableExtensions.Compare(a, b) or extension method: a.Compare(b).
    /// </remarks>
    public static int CompareToEnumerable<T>(this IEnumerable<T?>? obj, IEnumerable<T?>? other) where T : IComparable<T>
    {
        if (obj == null) return other == null ? 0 : -1;
        if (other == null) return 1;

        var c = 0;

        using var xEnum = obj.GetEnumerator();
        using var yEnum = other.GetEnumerator();

        do
        {
            var xHas = xEnum.MoveNext();
            var yHas = yEnum.MoveNext();

            if (!xHas) return yHas ? -1 : 0;
            if (!yHas) return 1;

            var xCurrent = xEnum.Current;
            var yCurrent = yEnum.Current;

            if (xCurrent == null)
            {
                if (yCurrent == null) continue;
                return -1;
            }

            if (yCurrent == null) return 1;

            c = xCurrent.CompareTo(yCurrent);
        } while (c == 0);

        return c;
    }

        public static IOrderedEnumerable<string> OrderByOrdinalIgnoreCaseThenOrdinal(this IEnumerable<string> obj) => obj.OrderBy(o => o, Constant.StringComparer_OrdinalIgnoreCase_Ordinal);
        public static IOrderedEnumerable<T> OrderByOrdinalIgnoreCaseThenOrdinal<T>(this IEnumerable<T> obj, Func<T, string> keySelector) => obj.OrderBy(keySelector, Constant.StringComparer_OrdinalIgnoreCase_Ordinal);

    #endregion CompareTo

    #region Equals

    // TODO: Update docs
    /// <summary>
    /// Performs lexical comparison of 2 IEnumerable collections holding elements of type T.
    /// https://stackoverflow.com/a/18211470
    /// </summary>
    /// <typeparam name="T">Type of collection elements.</typeparam>
    /// <param name="obj">The first collection to compare.</param>
    /// <param name="other">The second collection to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of a and b:
    /// Less than zero: first is less than second;
    /// Zero: first is equal to second;
    /// Greater than zero: first is greater than second.
    /// </returns>
    /// <remarks>
    /// Can be called as either static method: EnumerableExtensions.Compare(a, b) or extension method: a.Compare(b).
    /// </remarks>
    public static bool EqualsEnumerable<T>(this IEnumerable<T?>? obj, IEnumerable<T?>? other)
    {
        if (obj == null) return other == null;
        if (other == null) return false;

        using var xEnum = obj.GetEnumerator();
        using var yEnum = other.GetEnumerator();

        while (true)
        {
            var xHas = xEnum.MoveNext();
            var yHas = yEnum.MoveNext();

            if (!xHas) return !yHas;
            if (!yHas) return false;

            var xCurrent = xEnum.Current;
            var yCurrent = yEnum.Current;

            if (xCurrent == null)
            {
                if (yCurrent == null) continue;
                return false;
            }

            if (yCurrent == null) return false;

            if (!xCurrent.Equals(yCurrent)) return false;
        }
    }

    #endregion Equals
}
