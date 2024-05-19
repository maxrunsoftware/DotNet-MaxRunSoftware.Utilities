using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication.Web;

namespace MaxRunSoftware.Utilities.Web.Server.GenHTTP;

public class WebServerSiteSessionTokenHandler : ISessionHandling
{
    #region ISessionHandling
    
    // <see href="https://github.com/Kaliumhexacyanoferrat/GenHTTP/blob/master/Modules/Authentication.Web/Integration/CookieSessionHandling.cs" />
    
    public string CookieName { get; set; } = "wa_session";
    public TimeSpan CookieTimeout { get; set; } = TimeSpan.FromDays(30);
    
    public virtual string? ReadToken(IRequest request) => request.Cookies.TryGetValue(CookieName, out var token) ? new(token.Value) : null;
    
    public virtual void WriteToken(IResponse response, string sessionToken) => response.SetCookie(new(CookieName, sessionToken, Convert.ToUInt64(CookieTimeout.TotalSeconds)));
    
    public virtual void ClearToken(IResponse response) => response.SetCookie(new(CookieName, "", 0));
    
    #endregion ISessionHandling

}
