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
    
    public void OnServerError(ServerErrorScope scope, Exception error)
    {
        log.LogError(
            error,
            "ERROR - {Scope} - {Error}",
            scope,
            error.Message
        );
    }
    
}

public static class GenHTTPLoggingCompanionExtensions
{
    public static T AddLogging<T>(this IServerBuilder<T> builder, ILogger log) => builder.Companion(new GenHTTPLoggingCompanion(log));
}
