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

using System.Net;
using System.Net.Http;

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    public static readonly IPAddress IPAddress_Min = IPAddress.Any;
    public static readonly IPAddress IPAddress_Max = IPAddress.Broadcast;

    /// <summary>
    /// Map of HttpRequestHeader types to the HTTP header name
    /// </summary>
    public static readonly FrozenDictionary<HttpRequestHeader, string> HttpRequestHeader_String = new[] {
        (HttpRequestHeader.CacheControl, "Cache-Control"),
        (HttpRequestHeader.Connection, "Connection"),
        (HttpRequestHeader.Date, "Date"),
        (HttpRequestHeader.KeepAlive, "Keep-Alive"),
        (HttpRequestHeader.Pragma, "Pragma"),
        (HttpRequestHeader.Trailer, "Trailer"),
        (HttpRequestHeader.TransferEncoding, "Transfer-Encoding"),
        (HttpRequestHeader.Upgrade, "Upgrade"),
        (HttpRequestHeader.Via, "Via"),
        (HttpRequestHeader.Warning, "Warning"),
        (HttpRequestHeader.Allow, "Allow"),
        (HttpRequestHeader.ContentLength, "Content-Length"),
        (HttpRequestHeader.ContentType, "Content-Type"),
        (HttpRequestHeader.ContentEncoding, "Content-Encoding"),
        (HttpRequestHeader.ContentLanguage, "Content-Language"),
        (HttpRequestHeader.ContentLocation, "Content-Location"),
        (HttpRequestHeader.ContentMd5, "Content-MD5"),
        (HttpRequestHeader.ContentRange, "Content-Range"),
        (HttpRequestHeader.Expires, "Expires"),
        (HttpRequestHeader.LastModified, "Last-Modified"),
        (HttpRequestHeader.Accept, "Accept"),
        (HttpRequestHeader.AcceptCharset, "Accept-Charset"),
        (HttpRequestHeader.AcceptEncoding, "Accept-Encoding"),
        (HttpRequestHeader.AcceptLanguage, "Accept-Language"),
        (HttpRequestHeader.Authorization, "Authorization"),
        (HttpRequestHeader.Cookie, "Cookie"),
        (HttpRequestHeader.Expect, "Expect"),
        (HttpRequestHeader.From, "From"),
        (HttpRequestHeader.Host, "Host"),
        (HttpRequestHeader.IfMatch, "If-Match"),
        (HttpRequestHeader.IfModifiedSince, "If-Modified-Since"),
        (HttpRequestHeader.IfNoneMatch, "If-None-Match"),
        (HttpRequestHeader.IfRange, "If-Range"),
        (HttpRequestHeader.IfUnmodifiedSince, "If-Unmodified-Since"),
        (HttpRequestHeader.MaxForwards, "Max-Forwards"),
        (HttpRequestHeader.ProxyAuthorization, "Proxy-Authorization"),
        (HttpRequestHeader.Referer, "Referer"),
        (HttpRequestHeader.Range, "Range"),
        (HttpRequestHeader.Te, "Te"),
        (HttpRequestHeader.Translate, "Translate"),
        (HttpRequestHeader.UserAgent, "User-Agent")
    }.ConstantToFrozenDictionaryTry();
    
    /// <summary>
    /// Map of HTTP header name to HttpRequestHeader types
    /// </summary>
    public static readonly FrozenDictionary<string, HttpRequestHeader> String_HttpRequestHeader = HttpRequestHeader_String
        .Select(kvp => (kvp.Value, kvp.Key))
        .ConstantToFrozenDictionaryTry(StringComparer.OrdinalIgnoreCase);
    
    
    /// <summary>
    /// Map of HttpResponseHeader types to the HTTP header name
    /// </summary>
    public static readonly FrozenDictionary<HttpResponseHeader, string> HttpResponseHeader_String = new[]
    {
        (HttpResponseHeader.CacheControl, "Cache-Control"),
        (HttpResponseHeader.Connection, "Connection"),
        (HttpResponseHeader.Date, "Date"),
        (HttpResponseHeader.KeepAlive, "Keep-Alive"),
        (HttpResponseHeader.Pragma, "Pragma"),
        (HttpResponseHeader.Trailer, "Trailer"),
        (HttpResponseHeader.TransferEncoding, "Transfer-Encoding"),
        (HttpResponseHeader.Upgrade, "Upgrade"),
        (HttpResponseHeader.Via, "Via"),
        (HttpResponseHeader.Warning, "Warning"),
        (HttpResponseHeader.Allow, "Allow"),
        (HttpResponseHeader.ContentLength, "Content-Length"),
        (HttpResponseHeader.ContentType, "Content-Type"),
        (HttpResponseHeader.ContentEncoding, "Content-Encoding"),
        (HttpResponseHeader.ContentLanguage, "Content-Language"),
        (HttpResponseHeader.ContentLocation, "Content-Location"),
        (HttpResponseHeader.ContentMd5, "Content-MD5"),
        (HttpResponseHeader.ContentRange, "Content-Range"),
        (HttpResponseHeader.Expires, "Expires"),
        (HttpResponseHeader.LastModified, "Last-Modified"),
        (HttpResponseHeader.AcceptRanges, "Accept-Ranges"),
        (HttpResponseHeader.Age, "Age"),
        (HttpResponseHeader.ETag, "ETag"),
        (HttpResponseHeader.Location, "Location"),
        (HttpResponseHeader.ProxyAuthenticate, "Proxy-Authenticate"),
        (HttpResponseHeader.RetryAfter, "Retry-After"),
        (HttpResponseHeader.Server, "Server"),
        (HttpResponseHeader.SetCookie, "Set-Cookie"),
        (HttpResponseHeader.Vary, "Vary"),
        (HttpResponseHeader.WwwAuthenticate, "WWW-Authenticate")
    }.ConstantToFrozenDictionaryTry();

    /// <summary>
    /// Map of HTTP header name to HttpResponseHeader types
    /// </summary>
    public static readonly FrozenDictionary<string, HttpResponseHeader> String_HttpResponseHeader = HttpResponseHeader_String
        .Select(kvp => (kvp.Value, kvp.Key))
        .ConstantToFrozenDictionaryTry(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// Map of Http Status Codes to descriptions
    /// </summary>
    public static readonly FrozenDictionary<int, string> HttpStatusCodeInt_HttpStatusDescription = new[] {
        (100, "Continue"),
        (101, "Switching Protocols"),
        (102, "Processing"),
        (103, "Early Hints"),
        (200, "OK"),
        (201, "Created"),
        (202, "Accepted"),
        (203, "Non-Authoritative Information"),
        (204, "No Content"),
        (205, "Reset Content"),
        (206, "Partial Content"),
        (207, "Multi-Status"),
        (208, "Already Reported"),
        (226, "IM Used"),
        (300, "Multiple Choices"),
        (301, "Moved Permanently"),
        (302, "Found"),
        (303, "See Other"),
        (304, "Not Modified"),
        (305, "Use Proxy"),
        (307, "Temporary Redirect"),
        (308, "Permanent Redirect"),
        (400, "Bad Request"),
        (401, "Unauthorized"),
        (402, "Payment Required"),
        (403, "Forbidden"),
        (404, "Not Found"),
        (405, "Method Not Allowed"),
        (406, "Not Acceptable"),
        (407, "Proxy Authentication Required"),
        (408, "Request Timeout"),
        (409, "Conflict"),
        (410, "Gone"),
        (411, "Length Required"),
        (412, "Precondition Failed"),
        (413, "Request Entity Too Large"),
        (414, "Request-Uri Too Long"),
        (415, "Unsupported Media Type"),
        (416, "Requested Range Not Satisfiable"),
        (417, "Expectation Failed"),
        (421, "Misdirected Request"),
        (422, "Unprocessable Entity"),
        (423, "Locked"),
        (424, "Failed Dependency"),
        (426, "Upgrade Required"), // RFC 2817
        (428, "Precondition Required"),
        (429, "Too Many Requests"),
        (431, "Request Header Fields Too Large"),
        (451, "Unavailable For Legal Reasons"),
        (500, "Internal Server Error"),
        (501, "Not Implemented"),
        (502, "Bad Gateway"),
        (503, "Service Unavailable"),
        (504, "Gateway Timeout"),
        (505, "Http Version Not Supported"),
        (506, "Variant Also Negotiates"),
        (507, "Insufficient Storage"),
        (508, "Loop Detected"),
        (510, "Not Extended"),
        (511, "Network Authentication Required")
    }.ConstantToFrozenDictionaryTry();


    /// <summary>
    /// Map of HttpRequestMethod to HttpMethod
    /// </summary>
    public static readonly FrozenDictionary<HttpRequestMethod, HttpMethod> HttpRequestMethod_HttpMethod = new[] {
        (HttpRequestMethod.Get, HttpMethod.Get),
        (HttpRequestMethod.Put, HttpMethod.Put),
        (HttpRequestMethod.Post, HttpMethod.Post),
        (HttpRequestMethod.Delete, HttpMethod.Delete),
        (HttpRequestMethod.Head, HttpMethod.Head),
        (HttpRequestMethod.Options, HttpMethod.Options),
        (HttpRequestMethod.Trace, HttpMethod.Trace),
        (HttpRequestMethod.Patch, HttpMethod.Patch),
        (HttpRequestMethod.Connect, HttpMethod.Connect)
    }.ConstantToFrozenDictionaryTry();

    /// <summary>
    /// Map of HttpMethod to HttpRequestMethod
    /// </summary>
    public static readonly FrozenDictionary<HttpMethod, HttpRequestMethod> HttpMethod_HttpRequestMethod = HttpRequestMethod_HttpMethod
        .Select(kvp => (kvp.Value, kvp.Key))
        .ConstantToFrozenDictionaryTry();
}

public enum HttpRequestMethod
{
    Get,
    Put,
    Post,
    Delete,
    Head,
    Options,
    Trace,
    Patch,
    Connect,
}
