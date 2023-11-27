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

using System.Threading.Tasks;
using EmbedIO;

namespace MaxRunSoftware.Utilities.Web.Server;

public static class WebServerExtensions
{
    public static SortedDictionary<string, string> GetParameters(this IHttpContext context)
    {
        var d = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var parameters = context.GetRequestQueryData();
        foreach (var key in parameters.AllKeys)
        {
            var k = key.TrimOrNull();
            if (k == null) continue;

            var v = parameters[key].TrimOrNull();
            if (v == null) continue;

            d[k] = v;
        }

        return d;
    }

    public static string? GetParameterString(this IHttpContext context, string parameterName) => GetParameters(context).GetValueNullable(parameterName);

    public static string GetParameterString(this IHttpContext context, string parameterName, string defaultValue) => GetParameterString(context, parameterName) ?? defaultValue;

    public static int? GetParameterInt(this IHttpContext context, string parameterName)
    {
        var s = GetParameterString(context, parameterName);
        if (s == null) return null;

        if (int.TryParse(s, out var o)) return o;

        return null;
    }

    public static int GetParameterInt(this IHttpContext context, string parameterName, int defaultValue) => GetParameterInt(context, parameterName) ?? defaultValue;

    public static bool HasParameter(this IHttpContext context, string parameterName)
    {
        var parameters = context.GetRequestQueryData();
        foreach (var key in parameters.AllKeys)
        {
            if (string.Equals(key, parameterName, StringComparison.OrdinalIgnoreCase)) { return true; }
        }

        return false;
    }

    public static void AddHeader(this IHttpContext context, string name, params string[] values) => context.Response.Headers.Add(name + ": " + values.ToStringDelimited("; "));

    public static void SendFile(this IHttpContext context, byte[] bytes, string fileName)
    {
        context.AddHeader("Content-Disposition", "attachment", "filename=\"" + fileName + "\"");

        using var stream = context.OpenResponseStream();
        stream.Write(bytes, 0, bytes.Length);
    }

    public static void SendFile(this IHttpContext context, string data, string fileName, Encoding? encoding = null)
    {
        encoding ??= Constant.Encoding_UTF8_Without_BOM;
        var bytes = encoding.GetBytes(data);
        SendFile(context, bytes, fileName);
    }

    public static async Task SendStringHtmlSimpleAsync(this IHttpContext context, string? title, string? msg, string? css = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("  <head>");
        sb.AppendLine("    <meta charset=\"utf-8\">");
        if (title != null) sb.AppendLine($"    <title>{title}</title>");

        if (css != null)
        {
            sb.AppendLine("    <style>");
            sb.AppendLine($"    {css}");
            sb.AppendLine("    </style>");
        }

        sb.AppendLine("  </head>");
        sb.AppendLine("  <body>");
        if (title != null) sb.AppendLine($"    <h1>{title}</h1>");

        if (msg != null) sb.AppendLine($"    {msg}");

        sb.AppendLine("  </body>");
        sb.AppendLine("</html>");

        var s = sb.ToString();

        await context.SendStringAsync(s, "text/html", Constant.Encoding_UTF8_Without_BOM);


    }
}
