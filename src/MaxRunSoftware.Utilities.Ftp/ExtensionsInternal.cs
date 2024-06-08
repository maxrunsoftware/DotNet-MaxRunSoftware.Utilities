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

// ReSharper disable RedundantTypeDeclarationBody

using Renci.SshNet;

namespace MaxRunSoftware.Utilities.Ftp;

internal static class ExtensionsInternal
{
    public static bool TryGetCanonicalPathInternal(this IBaseClient client, string path, out string? canonicalPath)
    {
        var wrapper = GetCanonicalPathInternalWrapper.Get(client.GetType());
        canonicalPath = wrapper.GetCanonicalPath(client, path);
        return wrapper.IsValid;
    }
    
    private class GetCanonicalPathInternalWrapper
    {
        private static readonly DictionaryWeakType<GetCanonicalPathInternalWrapper> cacheClientType = new();
        public static GetCanonicalPathInternalWrapper Get(Type clientType) => cacheClientType.GetOrAdd(clientType, t => new(t));
        
        private readonly ISlimValueGetter? fieldSlim;
        private readonly ISlimInvokable? methodSlim;
        
        public bool IsValid => fieldSlim != null && methodSlim != null;
        
        public string? GetCanonicalPath(IBaseClient instance, string path)
        {
            if (!IsValid) return null;
            if (fieldSlim == null || methodSlim == null) return null;
            
            var sftpSession = fieldSlim.GetValue(instance);
            if (sftpSession == null) instance.Connect();
            sftpSession = fieldSlim.GetValue(instance);
            if (sftpSession == null) return null;
            
            var pathNew = methodSlim.Invoke(sftpSession, [path,]);
            return pathNew as string;
        }
        
        private GetCanonicalPathInternalWrapper(Type type)
        {
            var fm = FindFieldMethod(type);
            fieldSlim = fm?.Item1;
            methodSlim = fm?.Item2;
        }
        
        private static (ISlimValueGetter, ISlimInvokable)? FindFieldMethod(Type clientType)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = clientType.GetFieldSlims(flags);
            var properties = clientType.GetPropertySlims(flags).Where(o => o.IsGettablePublic || o.IsGettableNonPublic).ToArray();
            
            static (ISlimValueGetter, ISlimInvokable)? F(IEnumerable<FieldSlim> items) => items
                .Select(o => (Item: o, Method: FindMethod(o.Type)))
                .Where(o => o.Method != null)
                .Select(o => (o.Item, o.Method!))
                .FirstOrDefault();
            
            static (ISlimValueGetter, ISlimInvokable)? P(IEnumerable<PropertySlim> items) => items
                .Select(o => (Item: o, Method: FindMethod(o.Type)))
                .Where(o => o.Method != null)
                .Select(o => (o.Item, o.Method!))
                .FirstOrDefault();
            
            return F(fields.Where(o => o.Name.EqualsOrdinalIgnoreCase("_sftpSession")))
                   ?? F(fields.Where(o => o.Type.Name.EqualsOrdinalIgnoreCase("ISftpSession")))
                   ?? P(properties.Where(o => o.Name.EqualsOrdinalIgnoreCase("_sftpSession")))
                   ?? P(properties.Where(o => o.Type.Name.EqualsOrdinalIgnoreCase("ISftpSession")))
                   ?? F(fields)
                   ?? P(properties)
                ;
            
        }
        
        private static string FindMethodPrefix { get; } = FindMethodPrefix_Build() + ".";
        private static string FindMethodPrefix_Build()
        {
            static string? ParseTypes(Type[] types) => types.Select(ParseType).WhereNotNull().FirstOrDefault();
            static string? ParseType(Type type) => ParseString(type.Namespace) ?? ParseString(type.FullName);
            static string? ParseString(string? s)
            {
                s = s.TrimOrNull();
                if (s == null) return null;
                var parts = s.Split('.').TrimOrNull().WhereNotNull();
                if (parts.Length == 0) return null;
                s = parts[0];
                if (s.EqualsIgnoreCase("System")) return null;
                return s;
            }
            
            return ParseTypes([typeof(ISftpClient), typeof(SftpClient), typeof(IBaseClient)]) ?? "Renci";
        }
        
        private static MethodSlim? FindMethod(Type type)
        {
            static bool IsCorrectNamespace(Type type) =>
                new[] { type.Namespace, type.FullName, type.FullNameFormatted(), /* just so we are not empty */ Guid.NewGuid().ToString() }
                    .TrimOrNull()
                    .WhereNotNull()
                    .Any(o => o.StartsWith(FindMethodPrefix, StringComparison.OrdinalIgnoreCase));
            
            if (!IsCorrectNamespace(type)) return null;
            
            var methods = type.GetMethodSlims(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(o => o.Parameters.Length == 1)
                .Where(o => o.Parameters[0].Parameter.Type.Type.IsAssignableFrom(typeof(string)))
                .Where(o => o.ReturnType != null)
                .Where(o => o.ReturnType!.Type.IsAssignableFrom(typeof(string)))
                .ToArray();
            
            if (methods.Length == 0) return null;
            return methods.FirstOrDefault(o => o.Name.EqualsOrdinal("GetCanonicalPath"))
                   ?? methods.FirstOrDefault(o => o.Name.EqualsOrdinalIgnoreCase("GetCanonicalPath"))
                   ?? methods.FirstOrDefault(o => o.Name.Contains("Canonical", StringComparison.OrdinalIgnoreCase))
                ;
        }
    }
}
