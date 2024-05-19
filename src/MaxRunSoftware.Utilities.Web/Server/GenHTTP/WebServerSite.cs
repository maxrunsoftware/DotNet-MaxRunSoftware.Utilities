using System.Diagnostics.CodeAnalysis;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Web;
using GenHTTP.Modules.Authentication.Web.Concern;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Placeholders;
using GenHTTP.Modules.Razor;
using GenHTTP.Modules.Razor.Providers;
namespace MaxRunSoftware.Utilities.Web.Server.GenHTTP;

public record WebServerSiteAuthenticationModel
(
    string ButtonCaption,
    string? Username = null,
    string? ErrorMessage = null
);

public abstract partial class WebServerSite<TUser> : WebServer, IWebAuthIntegration<TUser>
    where TUser : class, IUser
{
    private readonly ILogger log;
    
    public ISessionHandling SessionTokenHandler { get; set; } = new WebServerSiteSessionTokenHandler();
    public IWebServerSiteSessionHandler SessionHandler { get; set; } = new WebServerSiteSessionHandler<TUser>();
    
    protected WebServerSite(ILogger log) : base(log)
    {
        this.log = log;
        SetupHandler = Controller.From(new SetupController(this));
        LoginHandler = Controller.From(new LoginController(this));
        LogoutHandler = Controller.From(new LogoutController());
        resourceHandlerLazy = Lzy.Create(ResourceCreate);
    }
    
    protected override void StartConfigure(LayoutBuilder layout)
    {
        base.StartConfigure(layout);
        var auth = new WebAuthenticationBuilder<TUser>(this).SessionHandling(SessionTokenHandler);
        layout.Add(auth);
    }
    

    #region IWebAuthIntegration<TUser>
    
    public bool AllowAnonymous { get; set; } = false;
    
    public virtual ValueTask<bool> CheckSetupRequired(IRequest request) => new(false);
    
    #region Session
    
    public ValueTask<TUser?> VerifyTokenAsync(IRequest request, string sessionToken)
    {
        log.LogTraceMethod(new(request, sessionToken), string.Empty);
        TUser? userTyped = null;
        var user = SessionHandler.GetSession(request, sessionToken);
        
        if (user == null)
        {
            log.LogDebug("No existing session for session[{SessionToken}]", sessionToken);
        }
        else
        {
            log.LogDebug("Found existing session for session[{SessionToken}]: {User}", sessionToken, user);
            userTyped = (TUser?)user;
        }
        
        return new(userTyped);
    }
    
    public ValueTask<string> StartSessionAsync(IRequest request, TUser user)
    {
        log.LogTraceMethod(new(request, user), string.Empty);
        var sessionToken = SessionHandler.NewSession(request, user);
        log.LogDebug("Started session: {SessionId} for user '{User}'", sessionToken, user);
        return new(sessionToken);
    }
    
    #endregion Session
    
    #endregion IWebAuthIntegration
    
    public virtual IHandlerBuilder RenderAuthentication(
        IBuilder<IResource> resource,
        string title,
        string buttonCaption,
        string? username = null,
        string? errorMessage = null,
        ResponseStatus? status = null
    )
    {
        var model = new WebServerSiteAuthenticationModel(buttonCaption, username, errorMessage);
        var response = ModRazor.Page(resource, (request, handler) => new ViewModel<WebServerSiteAuthenticationModel>(request, handler, model));
        response = response.Title(title);
        RenderAuthenticationConfigure(response);
        //response = response.AddStyle("{web-auth-resources}/style.css");
        
        if (status != null) response.Status(status.Value);
        
        return response;
    }
    
    protected virtual void RenderAuthenticationConfigure(RazorPageProviderBuilder<ViewModel<WebServerSiteAuthenticationModel>> builder) {}
}


#region Handlers


#region Setup

public abstract partial class WebServerSite<TUser> where TUser : class, IUser
{
    public required IBuilder<IResource> SetupResource { get; set; }
    public IHandlerBuilder SetupHandler { get; set; }
    public string SetupRoute { get; set; } = "setup";
    
    protected abstract void SetupExecute(IRequest request, string username, string password);
    
    protected virtual IHandlerBuilder SetupRender(string? username = null, string? errorMessage = null, ResponseStatus? status = null) =>
        RenderAuthentication(SetupResource, "Setup", "Create Account", username, errorMessage, status);
    
    protected virtual string? SetupValidateNewUser(string username, string password) => null;
    
    public class SetupController(WebServerSite<TUser> server)
    {
        [ControllerAction(RequestMethod.GET)]
        public IHandlerBuilder Index() => server.SetupRender();
        
        [ControllerAction(RequestMethod.POST)]
        public IHandlerBuilder Index(string username, string password, IRequest request)
        {
            var error = server.SetupValidateNewUser(username, password);
            if (error != null) return server.SetupRender(username, error, ResponseStatus.BadRequest);
            server.SetupExecute(request, username, password);
            return Redirect.To("{login}/", true);
        }
    }
    
}

#endregion Setup


#region Login

public abstract partial class WebServerSite<TUser> where TUser : class, IUser
{
    
    public required IBuilder<IResource> LoginResource { get; set; }
    public IHandlerBuilder LoginHandler { get; set; }
    public string LoginRoute { get; set; } = "login";
    
    protected record LoginUserResult(TUser? User, string? ErrorMessage, ResponseStatus? Status);
    
    protected abstract LoginUserResult LoginUser(IRequest request, string username, string password);
    
    protected virtual IHandlerBuilder LoginRender(string? username = null, string? errorMessage = null, ResponseStatus? status = null) =>
        RenderAuthentication(LoginResource, "Login", "Login", username, errorMessage, status);
    
    public class LoginController(WebServerSite<TUser> server)
    {
        [ControllerAction(RequestMethod.GET)]
        public IHandlerBuilder Index(IRequest request) =>
            request.GetUser<IUser>() == null
                ? server.LoginRender(status: ResponseStatus.Unauthorized)
                : Page.From("Login", "You are already logged in.");
        
        [ControllerAction(RequestMethod.POST)]
        public IHandlerBuilder Index(string username, string password, IRequest request)
        {
            // return server.LoginRender(username, "Please enter username and password.", status: ResponseStatus.BadRequest);
            // return server.LoginRender(username, "Invalid username or password.", status: ResponseStatus.Forbidden);
            
            var loginResult = server.LoginUser(request, username, password);
            if (loginResult.ErrorMessage != null || loginResult.User == null)
            {
                return server.LoginRender(username, loginResult.ErrorMessage, loginResult.Status);
            }
            
            request.SetUser(loginResult.User);
            return Redirect.To("{web-auth}/", true);
        }
    }
    
}

#endregion Login


#region Logout

public abstract partial class WebServerSite<TUser> where TUser : class, IUser
{
    
    public IHandlerBuilder LogoutHandler { get; set; }
    public string LogoutRoute { get; set; } = "logout";
    
    public class LogoutController
    {
        [ControllerAction(RequestMethod.GET)]
        public IHandlerBuilder Index(IRequest request)
        {
            if (request.GetUser<IUser>() == null) return Page.From("Logout", "You are already logged out.");
            request.ClearUser();
            return Page.From("Logout", "You have been successfully logged out.");
        }
    }
    
}

#endregion Logout


#region Resource

public abstract partial class WebServerSite<TUser> where TUser : class, IUser
{
    
    private readonly Lzy<IHandlerBuilder> resourceHandlerLazy;
    private IHandlerBuilder? resourceHandler;
    
    [AllowNull]
    public IHandlerBuilder ResourceHandler
    {
        get => resourceHandler ?? resourceHandlerLazy.Value;
        set => resourceHandler = value;
    }
    
    public string ResourceRoute { get; set; } = "auth-resources";
    
    protected virtual IHandlerBuilder ResourceCreate() => Resources.From(ResourceTree.FromAssembly("Resources"));
    

}

#endregion Resource


#endregion Handlers
