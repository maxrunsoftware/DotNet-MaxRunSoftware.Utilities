// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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

using System.Collections.Concurrent;
using System.Net;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace MaxRunSoftware.Utilities.Web;

public class GenHTTPLoggingCompanion(ILogger log) : IServerCompanion
{
    private readonly ConcurrentDictionary<IPAddress, string> cacheIPAddressString = new();
    private readonly ConcurrentDictionary<int, string> cacheKnownStatus = new();
    
    public void OnRequestHandled(IRequest request, IResponse response)
    {
        var clientAddress = request.Client.IPAddress;
        var clientAddressString = cacheIPAddressString.GetOrAdd(clientAddress, ToStringIPAddress);
        
        var contentLength = response.ContentLength ?? 0UL;
        var contentLengthString = contentLength <= 0UL ? string.Empty : $"[{contentLength}]";
        
        var status = response.Status;
        var statusString = cacheKnownStatus.GetOrAdd(status.RawStatus, ToStringStatus, status);
        
        log.LogDebug("REQUEST [{ClientAddress}] {Method} {Path} - {Status} {ContentLength}",
            clientAddressString,
            request.Method.RawMethod,
            request.Target.Path,
            statusString,
            contentLengthString
        );
        return;
        
        static string ToStringIPAddress(IPAddress address) => (IPAddress.IsLoopback(address) ? IPAddress.Loopback : address).MapToIPv4().ToString();
        
        static string ToStringStatus(int statusCode, FlexibleResponseStatus status)
        {
            var statusString = string.Empty + statusCode;
            var ks = status.KnownStatus;
            if (ks != null) statusString = statusString + "-" + ks.Value;
            return statusString;
        }
        
    }

    public void OnServerError(ServerErrorScope scope, IPAddress? client, Exception error)
    {
        log.LogError(
            error,
            "ERROR - {IPAddress} - {Scope} - {Error}",
            client == null ? "?.?.?.?" : client.MapToIPv4().ToString(),
            scope,
            error.Message
        );
    }
}

public static class GenHTTPLoggingCompanionExtensions
{
    public static T AddLogging<T>(this IServerBuilder<T> builder, ILogger log) => builder.Companion(new GenHTTPLoggingCompanion(log));
}
