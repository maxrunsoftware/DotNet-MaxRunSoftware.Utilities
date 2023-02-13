// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

using System.Runtime.CompilerServices;

// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace MaxRunSoftware.Utilities.Common;

public sealed class DictionaryWeakType<TValue> : IDictionary<Type, TValue>
{
    private readonly KeyCollection keyCollection;
    private readonly ValueCollection valueCollection;
    public DictionaryWeakType()
    {
        keyCollection = new(this);
        valueCollection = new(this);
    }

    private sealed class Value
    {
        public readonly object? obj;
        public Value(object? obj) => this.obj = obj;
    }

    private readonly ConditionalWeakTable<Type, Value> table = new();

    public TValue GetOrAdd(Type type, Func<Type, TValue> funcFactory)
    {
        var v = table.GetValue(type, key => new(funcFactory(key)));
        var vv = v.obj;
        var vvv = (TValue)vv!;
        return vvv;
    }

    public IEnumerator<KeyValuePair<Type, TValue>> GetEnumerator()
    {
        foreach (var (k, v) in table)
        {
            if (ReferenceEquals(k, null)) continue;
            var vv = v?.obj;
            if (vv == null) continue;
            yield return new(k, (TValue)vv);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Add(KeyValuePair<Type, TValue> item) => Add(item.Key, item.Value);
    public void Clear() => table.Clear();
    public bool Contains(KeyValuePair<Type, TValue> item) => table.TryGetValue(item.Key, out var v) && IsEqualValue(item.Value, (TValue?)v?.obj);
    public void CopyTo(KeyValuePair<Type, TValue>[] array, int arrayIndex) => this.ToList().CopyTo(array, arrayIndex);
    public bool Remove(KeyValuePair<Type, TValue> item) => Contains(item) && table.Remove(item.Key); // TODO: Not thread safe
    public int Count => table.Count();
    public bool IsReadOnly => false;
    public void Add(Type key, TValue value) => table.Add(key, new(value));
    public bool ContainsKey(Type key) => table.TryGetValue(key, out _);
    public bool ContainsValue(TValue value) => this.Any(kvp => IsEqualValue(value, kvp.Value));

    private bool IsEqualValue(TValue x, TValue? y) => Equals(x, y);
    public bool Remove(Type key) => table.Remove(key);
    public bool TryGetValue(Type key, out TValue value)
    {
        if (table.TryGetValue(key, out var v))
        {
            var vv = v?.obj;
            if (vv != null)
            {
                value = (TValue)vv;
                return true;
            }
        }

        value = default!;
        return false;
    }

    public TValue this[Type key]
    {
        get =>
            TryGetValue(key, out var value)
                ? value
                : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
        set => throw new NotImplementedException();
    }

    public IEnumerable<Type> Keys => this.Select(o => o.Key);
    public IEnumerable<TValue> Values => this.Select(o => o.Value);

    ICollection<TValue> IDictionary<Type, TValue>.Values => valueCollection;
    ICollection<Type> IDictionary<Type, TValue>.Keys => keyCollection;

    public sealed class KeyCollection : ItemCollection<Type>
    {
        private readonly DictionaryWeakType<TValue> d;
        public KeyCollection(DictionaryWeakType<TValue> dictionary) => d = dictionary;
        public override IEnumerator<Type> GetEnumerator() => d.Select(o => o.Key).GetEnumerator();
        public override bool Contains(Type item) => d.ContainsKey(item);
        public override object SyncRoot => d;
        public override int Count => d.Count;
    }

    public sealed class ValueCollection : ItemCollection<TValue>
    {
        private readonly DictionaryWeakType<TValue> d;
        public ValueCollection(DictionaryWeakType<TValue> dictionary) => d = dictionary;
        public override IEnumerator<TValue> GetEnumerator() => d.Select(o => o.Value).GetEnumerator();
        public override bool Contains(TValue item) => d.ContainsValue(item);
        public override object SyncRoot => d;
        public override int Count => d.Count;
    }

    public abstract class ItemCollection<TItem> : ICollection<TItem>, ICollection, IReadOnlyCollection<TItem>
    {
        private static NotSupportedException CollectionActionNotSupported() =>
            new("Mutating a key collection derived from a dictionary is not allowed.");

        public abstract IEnumerator<TItem> GetEnumerator();
        public abstract int Count { get; }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(TItem item) => throw CollectionActionNotSupported();
        public void Clear() => throw CollectionActionNotSupported();
        public abstract bool Contains(TItem item);
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            array.CheckNotNull();
            arrayIndex.CheckMin(0);
            arrayIndex.CheckMax(array.Length - 1);

            var count = Count;
            if (array.Length - arrayIndex < count)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex,
                    $"{nameof(array)}.{nameof(array.Length)}={array.Length} - {nameof(arrayIndex)}={arrayIndex} < {nameof(Count)}={Count}");
            }

            var entries = this.ToList();
            entries.CopyTo(array, arrayIndex);
        }
        public bool Remove(TItem item) => throw CollectionActionNotSupported();
        public void CopyTo(Array array, int index)
        {
            if (array is TItem[] keys)
            {
                CopyTo(keys, index);
                return;
            }

            if (array is not object?[] arrayObjects) throw new ArgumentException($"Invalid array type {array.GetType().FullNameFormatted()}", nameof(array));

            arrayObjects.CheckNotNull();
            index.CheckMin(0);
            index.CheckMax(arrayObjects.Length - 1);

            var count = Count;
            if (arrayObjects.Length - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index,
                    $"{nameof(array)}.{nameof(array.Length)}={array.Length} - {nameof(index)}={index} < {nameof(Count)}={Count}");
            }

            try
            {
                var entries = this.Select(o => (object?)o).ToList();
                entries.CopyTo(arrayObjects, index);
            }
            catch (ArrayTypeMismatchException e)
            {
                throw new ArgumentException($"Invalid array type {array.GetType().FullNameFormatted()}", nameof(array), e);
            }
        }
        int ICollection.Count => Count;
        public bool IsSynchronized => false;
        public abstract object SyncRoot { get; }
        int ICollection<TItem>.Count => Count;
        public bool IsReadOnly => true;
        int IReadOnlyCollection<TItem>.Count => Count;
    }


}
