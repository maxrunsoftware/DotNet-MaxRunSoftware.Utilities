// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

using System.Drawing;

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
/// <summary>
/// https://stackoverflow.com/a/12340136
/// </summary>
public static partial class Constant
{
    private sealed class ColorEqualityComparerImpl : IEqualityComparer<Color>
    {
        public bool Equals(Color x, Color y)
        {
            if (x.IsEmpty && y.IsEmpty) return true;
            if (x.IsEmpty || y.IsEmpty) return false;
            
            return x.R == y.R && x.G == y.G && x.B == y.B && x.A == y.A;
        }
        
        public int GetHashCode(Color obj) => obj.IsEmpty ? 0 : HashCode.Combine(obj.R, obj.G, obj.B, obj.A);
    }
    
    public static readonly IEqualityComparer<Color> ColorEqualityComparer = new ColorEqualityComparerImpl();
    
    
    /// <summary>
    /// Case-insensitive map of Color names to Colors
    /// </summary>
    public static readonly ImmutableDictionary<string, Color> Name_Color = Name_Color_Create();

    private static ImmutableDictionary<string, Color> Name_Color_Create()
    {
        // https://stackoverflow.com/a/3821197

        var b = ImmutableDictionary.CreateBuilder<string, Color>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var colorType = typeof(Color);
            // We take only static property to avoid properties like Name, IsSystemColor ...
            var propInfos = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            foreach (var propInfo in propInfos)
            {
                if (!propInfo.CanRead) continue;
                
                var colorGetMethod = propInfo.GetGetMethod();
                if (colorGetMethod == null) continue;
                
                if (propInfo.PropertyType != colorType) continue;
                
                var colorObject = colorGetMethod.Invoke(null, null);
                if (colorObject == null) continue;

                if (colorObject.GetType() != colorType) continue;

                var color = (Color)colorObject;
                var colorName = propInfo.Name;
                b.TryAdd(colorName, color);
            }
        }
        catch (Exception e) { LogError(e); }

        return b.ToImmutable();
    }
    
    /// <summary>
    /// Case-insensitive map of hex values to known Colors
    /// </summary>
    public static readonly ImmutableDictionary<string, Color> Hex_Color = Hex_Color_Create();
    
    private static ImmutableDictionary<string, Color> Hex_Color_Create()
    {
        var b = ImmutableDictionary.CreateBuilder<string, Color>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            foreach (var color in Name_Color.Values)
            {
                var hex = ColorTranslator.ToHtml(color);
                b.TryAdd(hex, color);
            }
        }
        catch (Exception e) { LogError(e); }
        
        return b.ToImmutable();
    }
    
    public static readonly ImmutableDictionary<KnownColor, Color> KnownColor_Color = KnownColor_Color_Create();
    
    private static ImmutableDictionary<KnownColor, Color> KnownColor_Color_Create()
    {
        var b = ImmutableDictionary.CreateBuilder<KnownColor, Color>();
        
        try
        {
            foreach (var knownColor in Enum.GetValues<KnownColor>())
            {
                var color = Color.FromKnownColor(knownColor);
                b.TryAdd(knownColor, color);
            }
        }
        catch (Exception e) { LogError(e); }
        
        return b.ToImmutable();
    }
    
    public static readonly ImmutableDictionary<Color, KnownColor> Color_KnownColor = Color_KnownColor_Create();
    
    private static ImmutableDictionary<Color, KnownColor> Color_KnownColor_Create()
    {
        var b = ImmutableDictionary.CreateBuilder<Color, KnownColor>(ColorEqualityComparer);
        
        try
        {
            foreach (var (knownColor, color) in KnownColor_Color)
            {
                b.TryAdd(color, knownColor);
            }
        }
        catch (Exception e) { LogError(e); }
        
        return b.ToImmutable();
    }
}
