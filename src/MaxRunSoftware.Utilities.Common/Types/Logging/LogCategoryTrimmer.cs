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

namespace MaxRunSoftware.Utilities.Common;

public class LogCategoryTrimmer
{
    private readonly ImmutableArray<string> baseCategoryPartsReversed;

    private readonly Dictionary<string, string> categoryCodes = new();

    public string Trim(string category)
    {
        if (category.Length == 0) return category;

        if (categoryCodes.TryGetValue(category, out var categoryClean)) return categoryClean;

        var classNameParts = new Stack<string>(baseCategoryPartsReversed);
        var categoryParts = new Stack<string>(category.Split('.').Reverse());
        while (categoryParts.Count > 1 && classNameParts.Count > 0)
        {
            var classNamePart = classNameParts.Pop();
            if (categoryParts.Peek() != classNamePart) break;
            categoryParts.Pop();
        }

        categoryClean = categoryParts.ToStringDelimited(".");
        categoryCodes[category] = categoryClean;
        return categoryClean;
    }

    public LogCategoryTrimmer(string baseCategory) => baseCategoryPartsReversed = baseCategory.Split('.').Reverse().ToImmutableArray();

    public LogCategoryTrimmer(Type baseCategory) : this(LogTypeNameHelper.GetTypeDisplayName(
        baseCategory,
        includeGenericParameters: false,
        nestedTypeDelimiter: '.'
    )) { }
}
