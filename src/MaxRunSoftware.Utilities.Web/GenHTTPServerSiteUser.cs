using System.Collections.Concurrent;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Web;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Razor;

namespace MaxRunSoftware.Utilities.Web;

public record GenHTTPServerSiteAuthModel(
    string Title,
    string? ButtonCaption = null,
    string? Text = null,
    string? Username = null,
    string? ErrorMessage = null,
    Exception? Error = null,
    ResponseStatus? ResponseStatus = null
    
);

public class GenHTTPServerSiteAuthUser : IUser
{
    public virtual required string Username { get; set; } 
    public virtual string? Password { get; set; }
    public virtual string DisplayName => ToString();
    public override string ToString() => Username;
}

public class GenHTTPServerSiteAuth(ILogger log) : GenHTTPServerSite(log)
{

    
    private class WebAuthIntegration : IWebAuthIntegration<GenHTTPServerSiteAuthUser>
    {
        private readonly GenHTTPServerSiteAuth server;
        
        public WebAuthIntegration(GenHTTPServerSiteAuth server)
        {
            this.server = server;
            var t = this;
            
            setupHandler = Lzy.Create<IHandlerBuilder>(() => Controller.From(new SetupController(t)));
            loginHandler = Lzy.Create<IHandlerBuilder>(() => Controller.From(new LoginController(t)));
            logoutHandler = Lzy.Create<IHandlerBuilder>(() => Controller.From(new LogoutController(t)));
            resourceHandler = Lzy.Create<IHandlerBuilder>(() => GenHTTP.Modules.IO.Resources.From(ResourceTree.FromAssembly("Resources")));
        }
        
        #region IWebAuthIntegration
        
        public virtual bool AllowAnonymous => server.AllowAnonymous;
        
        public virtual ValueTask<bool> CheckSetupRequired(IRequest request) => new(server.IsUserSetupNeeded());
        public virtual ValueTask<GenHTTPServerSiteAuthUser?> VerifyTokenAsync(IRequest request, string sessionToken) => new(server.SessionGet(request, sessionToken));
        public virtual ValueTask<string> StartSessionAsync(IRequest request, GenHTTPServerSiteAuthUser user) => new(server.SessionNew(request, user));
        
        private readonly Lzy<IHandlerBuilder> setupHandler;
        public virtual IHandlerBuilder SetupHandler => setupHandler.Value;
        public virtual string SetupRoute => "setup";
        
        private readonly Lzy<IHandlerBuilder> loginHandler;
        public virtual IHandlerBuilder LoginHandler => loginHandler.Value;
        public virtual string LoginRoute => "login";
        
        private readonly Lzy<IHandlerBuilder> logoutHandler;
        public virtual IHandlerBuilder LogoutHandler => logoutHandler.Value;
        public virtual string LogoutRoute => "logout";
        
        private readonly Lzy<IHandlerBuilder> resourceHandler;
        public virtual IHandlerBuilder ResourceHandler => resourceHandler.Value;
        public virtual string ResourceRoute => "auth-resources";
        
        #endregion IWebAuthIntegration
        
        protected class SetupController(WebAuthIntegration server)
        {
            [ControllerAction(RequestMethod.GET)] public virtual IHandlerBuilder Index(IRequest request) => server.server.RenderUserSetup(request);
            [ControllerAction(RequestMethod.POST)] public virtual IHandlerBuilder Index(IRequest request, string username, string password) => server.server.RenderUserSetupPost(request, username, password);
        }
        
        protected class LoginController(WebAuthIntegration server)
        {
            [ControllerAction(RequestMethod.GET)] public virtual IHandlerBuilder Index(IRequest request) => server.server.RenderUserLogin(request);
            [ControllerAction(RequestMethod.POST)] public virtual IHandlerBuilder Index(IRequest request, string username, string password) => server.server.RenderUserLoginPost(request, username, password);
        }
        
        protected class LogoutController(WebAuthIntegration server)
        {
            [ControllerAction(RequestMethod.GET)] public virtual IHandlerBuilder Index(IRequest request) => server.server.RenderUserLogout(request);
        }
    }
    
    #region Session
    
    protected virtual IDictionary<string, GenHTTPServerSiteAuthUser> Sessions { get; set; } = new ConcurrentDictionary<string, GenHTTPServerSiteAuthUser>();
    public virtual string SessionIdCookieName { get; set; } = "wa_session";
    public virtual TimeSpan SessionIdCookieTimeout { get; set; } = TimeSpan.FromDays(30);

    protected virtual string SessionNew(IRequest request, GenHTTPServerSiteAuthUser session)
    {
        string sessionId;
        while (!Sessions.TryAdd(sessionId = Util.GuidNewSecure().ToString(), session)) { }
        log.LogDebugMethod(new(request, session), "Created new session[{SessionId}]", sessionId);
        return sessionId;
    }
    
    protected virtual GenHTTPServerSiteAuthUser? SessionGet(IRequest request, string sessionId)
    {
        log.LogDebugMethod(new(request, sessionId), string.Empty);
        if (Sessions.TryGetValue(sessionId, out var session))
        {
            log.LogDebug("Found existing session[{SessionId}]: {Session}", sessionId, session);
            return session;
        }
        
        log.LogDebug("No existing session[{SessionId}]", sessionId);
        return null;
    }
  
    protected virtual string? SessionIdRead(IRequest request)
    {
        log.LogDebugMethod(new(request), string.Empty);
        if (request.Cookies.TryGetValue(SessionIdCookieName, out var sessionId))
        {
            log.LogDebug("Found session cookie '{Cookie}' with value: {SessionId}", SessionIdCookieName, sessionId);
            return sessionId.Value;
        }
        
        log.LogDebug("Not found session cookie '{Cookie}'", SessionIdCookieName);
        return null;
    }
    
    protected virtual void SessionIdWrite(IResponse response, string sessionId)
    {
        var maxAge = Convert.ToUInt64(SessionIdCookieTimeout.TotalSeconds);
        log.LogDebugMethod(new(response, sessionId), "Writing session cookie ({Cookie}, {SessionId}, {SessionLength})", SessionIdCookieName, sessionId, maxAge);
        var cookie = new Cookie(SessionIdCookieName, sessionId, maxAge);
        response.SetCookie(cookie);
    }
    
    protected virtual void SessionIdClear(IResponse response)
    {
        log.LogDebugMethod(new(response), string.Empty);
        var cookie = new Cookie(SessionIdCookieName, string.Empty, 0UL);
        response.SetCookie(cookie);
    }
    
    #endregion Session
    
    #region Auth
    
    #region User
    
    protected IDictionary<string, GenHTTPServerSiteAuthUser> Users { get; set; } = new ConcurrentDictionary<string, GenHTTPServerSiteAuthUser>(StringComparer.OrdinalIgnoreCase); 
    
    protected virtual void UserCreate(IRequest request, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException($"{nameof(username)} cannot be empty", nameof(username));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException($"{nameof(password)} cannot be empty", nameof(password));
        var o = new GenHTTPServerSiteAuthUser { Username = username, Password = password, };
        if (!Users.TryAdd(username, o)) throw new ArgumentException($"{nameof(username)} '{username}' already exists", nameof(username));
    }
    
    protected virtual GenHTTPServerSiteAuthUser UserLogin(IRequest request, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException($"{nameof(username)} cannot be empty", nameof(username));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException($"{nameof(password)} cannot be empty", nameof(password));
        
        if (!Users.TryGetValue(username, out var o)) throw new ArgumentException($"{nameof(username)} '{username}' does not exist", nameof(username));
        
        if (!o.Password.EqualsOrdinal(password)) throw new ArgumentException($"invalid {nameof(password)} for {nameof(username)} '{username}'", nameof(password));
        
        return o;
    } 
    
    #endregion User
    
    public virtual bool AllowAnonymous { get; set; } = false;
    
    public virtual string RenderUserPageCSHTML { get; set; } =
        """
        <form id="credentials" method="post" action=".">
            <div class="container">
                <div class="inputs">
                    <div class="username_container">
                        <label for="username" class="form-label">Username: </label>
                        <input type="text" class="form-control" id="username" name="username" placeholder="username" value="@Model.Data.Username" />
                    </div>
                    <div class="password_container">
                        <label for="password" class="form-label">Password: </label>
                        <input type="password" class="form-control" id="password" name="password" placeholder="password" />
                    </div>
                    @if (@Model.Data.ErrorMessage != null)
                    {
                    <div class="alert alert-danger" role="alert">
                        @Model.Data.ErrorMessage
                    </div>
                    }
                    <button class="btn btn-primary" type="submit">@Model.Data.ButtonCaption</button>
                </div>
            </div>
        </form>
        """;
    
    protected virtual IHandlerBuilder RenderUserPage(IRequest request, GenHTTPServerSiteAuthModel model)
    {
        var response = ModRazor.Page(
                    Resource.FromString(RenderUserPageCSHTML),
                    (r, h) => new ViewModel(r, h, model)
                )
                .Title(model.Title)
            ;
        
        response = AddPageAdditions(response);
        
        if (model.ResponseStatus != null) response.Status(model.ResponseStatus.Value);
        return response;
    }
    
    #region Setup
    
    protected virtual bool IsUserSetupNeeded() => false;
    
    protected virtual IHandlerBuilder RenderUserSetup(IRequest request)
    {
        return RenderUserPage(request, new(
            Title: "Setup",
            ButtonCaption: "Create Account"
        ));
    }
    
    protected virtual IHandlerBuilder RenderUserSetupPost(IRequest request, string username, string password)
    {
        try
        {
            UserCreate(request, username, password);
            return Redirect.To("{login}/", true);
        }
        catch (Exception e)
        {
            return RenderUserPage(request, new(
                Title: "Setup",
                ButtonCaption: "Create Account",
                Username: username,
                Error: e,
                ResponseStatus: ResponseStatus.BadRequest
            ));
        }
    }
    
    public virtual IHandlerBuilder RenderUserLogin(IRequest request)
    {
        var u = request.GetUser<IUser>();
        if (u == null)
        {
            return RenderUserPage(request, new(
                Title: "Login",
                ButtonCaption: "Login",
                ResponseStatus: ResponseStatus.Unauthorized
            ));
        }
        else
        {
            return RenderUserPage(request, new(
                Title: "Login",
                Text: "You are already logged in."
            ));
        }
    }
    
    public virtual IHandlerBuilder RenderUserLoginPost(IRequest request, string username, string password)
    {
        // return server.LoginRender(username, "Please enter username and password.", status: ResponseStatus.BadRequest);
        // return server.LoginRender(username, "Invalid username or password.", status: ResponseStatus.Forbidden);
        var u = request.GetUser<IUser>();
        if (u != null)
        {
            return RenderUserPage(request, new(
                Title: "Login",
                Text: "You are already logged in."
            ));
        }
        
        try
        {
            var user = UserLogin(request, username, password);
            request.SetUser(user);
            return Redirect.To("{web-auth}/", true);
        }
        catch (Exception e)
        {
            return RenderUserPage(request, new(
                Title: "Login",
                ButtonCaption: "Login",
                Username: username,
                Error: e,
                ResponseStatus: ResponseStatus.Forbidden
            ));
        }
    }
    
    public virtual IHandlerBuilder RenderUserLogout(IRequest request)
    {
        var u = request.GetUser<IUser>();
        if (u == null)
        {
            return RenderUserPage(request, new(
                Title: "Logout",
                Text: "You are already logged out."
            ));
        }
        else
        {
            request.ClearUser();
            return RenderUserPage(request, new(
                Title: "Logout",
                Text: "You have been successfully logged out."
            ));
        }
    }
    
    #endregion Setup
    
    #endregion Auth
    
    

    
    
}
