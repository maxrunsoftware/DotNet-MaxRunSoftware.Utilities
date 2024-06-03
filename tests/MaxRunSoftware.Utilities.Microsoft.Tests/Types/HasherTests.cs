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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MaxRunSoftware.Utilities.Microsoft.Tests;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute
public class HasherTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    [SkippableFact]
    public void Can_Hash()
    {
        
        var hashers = Hasher.Factories
                .Select(o => o.Key)
                .Select(Hasher.GetHasher)
                .OrderBy(o => o.Name, Constant.StringComparer_OrdinalIgnoreCase_Ordinal)
                .ToArray()
            ;
        
        foreach (var hasher in hashers)
        {
            WriteLine("");    
        }

    }

   
}
