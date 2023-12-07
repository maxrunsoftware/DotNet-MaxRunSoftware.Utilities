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

using EmbedIO;

namespace MaxRunSoftware.Utilities.Web.Server;

public static class WebServerHttpContextExtensions
{



    public static WebServerHttpContext SetStatus(this WebServerHttpContext context, int httpStatusCode)
    {
        context.HttpContext.Response.StatusCode = httpStatusCode;
        return context;
    }

    public static async Task SendStringHtmlSimpleAsync(this WebServerHttpContext context, string? title, string? msg, string? css = null)
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

        await context.HttpContext.SendStringAsync(s, "text/html", Constant.Encoding_UTF8_Without_BOM);


    }
}
