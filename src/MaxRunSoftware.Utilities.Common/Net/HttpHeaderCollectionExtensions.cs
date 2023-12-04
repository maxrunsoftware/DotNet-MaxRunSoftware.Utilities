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

using System.Net;

namespace MaxRunSoftware.Utilities.Common;

public static class HttpHeaderCollectionExtensions
{
    public static HttpHeaderCollection Add(this HttpHeaderCollection collection, HttpRequestHeader header, params string[] values) => collection.Add(header.GetName(), values);

    public static HttpHeaderCollection Add(this HttpHeaderCollection collection, HttpResponseHeader header, params string[] values) => collection.Add(header.GetName(), values);

    public static HttpHeaderCollection Remove(this HttpHeaderCollection collection, params HttpRequestHeader[] headers) => collection.Remove(headers.Select(o => o.GetName()).ToArray());

    public static HttpHeaderCollection Remove(this HttpHeaderCollection collection, params HttpResponseHeader[] headers) => collection.Remove(headers.Select(o => o.GetName()).ToArray());

    public static bool Contains(this HttpHeaderCollection collection, HttpRequestHeader header) => collection.Contains(header.GetName());

    public static bool Contains(this HttpHeaderCollection collection, HttpResponseHeader header) => collection.Contains(header.GetName());
}
