using System.Collections.Concurrent;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace MaxRunSoftware.Utilities.Web.Server.GenHTTP;

public interface IWebServerSiteSessionHandler
{
    public object? GetSession(IRequest request, string sessionId);
    public string NewSession(IRequest request, object user);
}

public interface IWebServerSiteSessionHandler<TUser> : IWebServerSiteSessionHandler where TUser : class, IUser
{
    public new TUser? GetSession(IRequest request, string sessionId);
    public string NewSession(IRequest request, TUser user);
}

public abstract class WebServerSiteSessionHandlerBase<TUser> : IWebServerSiteSessionHandler<TUser> where TUser : class, IUser
{
    public abstract TUser? GetSession(IRequest request, string sessionId);
    
    public abstract string NewSession(IRequest request, TUser user);
    
    object? IWebServerSiteSessionHandler.GetSession(IRequest request, string sessionId) => GetSession(request, sessionId);
    string IWebServerSiteSessionHandler.NewSession(IRequest request, object user) => NewSession(request, (TUser)user);
    
    TUser? IWebServerSiteSessionHandler<TUser>.GetSession(IRequest request, string sessionId) => GetSession(request, sessionId);
    string IWebServerSiteSessionHandler<TUser>.NewSession(IRequest request, TUser user) => NewSession(request, user);
} 

public class WebServerSiteSessionHandler<TUser>: WebServerSiteSessionHandlerBase<TUser> where TUser : class, IUser
{
    public ConcurrentDictionary<string, TUser> Sessions { get; set; } = new();
    
    public override TUser? GetSession(IRequest request, string sessionId) => Sessions.GetValueOrDefault(sessionId);
    
    public override string NewSession(IRequest request, TUser user)
    {
        var sessionId = Guid.NewGuid().ToString();
        Sessions[sessionId] = user;
        return sessionId;
    }
    
}
