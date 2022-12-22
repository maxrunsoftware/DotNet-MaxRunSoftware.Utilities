// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Common;

[PublicAPI]
public sealed class EnumItem : IEquatable<EnumItem>, IComparable<EnumItem>, IComparable
{
    public TypeSlim TypeSlim { get; }
    public Type Type => TypeSlim.Type;
    public string Name { get; }
    public object Item { get; }
    public int Index { get; }
    private readonly int getHashCode;
    private readonly FieldInfo info;
    public IEnumerable<Attribute> Attributes => info.GetCustomAttributes();

    public EnumItem(Type enumType, string itemName)
    {
        enumType.CheckIsEnum();
        TypeSlim = new TypeSlim(enumType);

        var possibleNames = Enum.GetNames(enumType).ToHashSet();
        var actualItemName = possibleNames.FirstOrDefault(o => string.Equals(itemName, o, StringComparison.Ordinal));

        if (actualItemName == null)
        {
            var d = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var possibleName in possibleNames) d.AddToList(possibleName, possibleName);
            if (d.TryGetValue(itemName, out var list2))
            {
                if (list2.Count > 1) throw new ArgumentException($"{enumType.FullNameFormatted()} contains multiple possible '{itemName}' values but none matching exact case: " + list2.ToStringDelimited(", "), nameof(itemName));
                actualItemName = list2[0];
            }
        }

        Name = actualItemName ?? throw new ArgumentException($"{enumType.FullNameFormatted()} does not contain item '{itemName}'", nameof(itemName));
        Item = Enum.Parse(enumType, Name);

        // https://stackoverflow.com/questions/6819348/enum-getnames-results-in-unexpected-order-with-negative-enum-constants
        var itemInfo = enumType
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(fieldInfo => possibleNames.Contains(fieldInfo.Name))
                .Select((fieldInfo, i) => (FieldName: fieldInfo.Name, FieldInfo: fieldInfo, Index: i))
                .First(o => o.FieldName == Name);
            ;

        Index = itemInfo.Index;
        info = itemInfo.FieldInfo;
        getHashCode = Util.Hash(TypeSlim.GetHashCode(), Index, StringComparer.Ordinal.GetHashCode(Name));
    }

    #region Override

    public override int GetHashCode() => getHashCode;

    public override bool Equals(object? obj) => Equals(obj as EnumItem);

    public bool Equals(EnumItem? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (getHashCode != other.getHashCode) return false;
        if (Index != other.Index) return false;
        if (!TypeSlim.Equals(other.TypeSlim)) return false;
        if (!StringComparer.Ordinal.Equals(Name, other.Name)) return false;

        return true;
    }

    public int CompareTo(object? obj) => CompareTo(obj as EnumItem);

    public int CompareTo(EnumItem? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;
        if (0 != (c = TypeSlim.CompareTo(other.TypeSlim))) return c;
        if (0 != (c = Index.CompareTo(other.Index))) return c;
        if (0 != (c = Name.CompareToOrdinalIgnoreCaseThenOrdinal(other.Name))) return c;
        if (0 != (c = getHashCode.CompareTo(other.getHashCode))) return c;
        return c;
    }

    public override string ToString() => Name;

    #endregion Override

    #region Static



    #endregion Static


}
