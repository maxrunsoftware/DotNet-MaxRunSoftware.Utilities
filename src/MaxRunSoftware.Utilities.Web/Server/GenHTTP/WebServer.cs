using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine;
using GenHTTP.Modules.Authentication.Web.Concern;
using GenHTTP.Modules.Authentication.Web.Controllers;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Security.Cors;
using GenHTTP.Modules.Websites;
using GenHTTP.Modules.Websites.Sites;

namespace MaxRunSoftware.Utilities.Web.Server.GenHTTP;

public class WebServer(ILogger log) : IDisposable
{
    private IServerHost? host;
    
    public ushort Port { get; set; } = 8080;
    public IDictionary<string, OriginPolicy> CorsPolicies { get; set; } = new Dictionary<string, OriginPolicy>(StringComparer.OrdinalIgnoreCase);
    public bool IsEnabledCors { get; set; }
    public X509Certificate2? Certificate { get; set; }
    
    public bool Compression { get; set; } = true;
    public bool SecureUpgrade { get; set; } = true;
    public bool StrictTransport { get; set; } = true;
    public bool ClientCaching { get; set; } = true;
    public bool RangeSupport { get; set; } = false;
    public bool PreventSniffing { get; set; } = false;
    
    #region Start
    
    protected virtual IServerHost CreateServerHost()
    {
        var layout = Layout.Create();
        StartAddCors(layout);
        foreach (var o in StartAddResourcesFromAssembly()) layout.Add(o, Download.From(Resource.FromAssembly(o)));
        foreach (var o in StartAddControllers()) layout.Add(o.Path, Controller.From(o.Controller));
        StartConfigure(layout);
        
        var content = Website.Create();
        var theme = StartAddTheme();
        if (theme != null) content = content.Theme(theme);
        content = content.Content(layout);
        
        host = StartBuildHost(content);
        return host;
    }
    
    public virtual void Start() => CreateServerHost().Start();
    public virtual void Run() => CreateServerHost().Run();
    
    protected virtual IServerHost StartBuildHost(WebsiteBuilder content)
    {
        var h = Host.Create();
        h = h.Companion(new LoggingCompanion(log));
        h = h.Port(Port);
        h = h.Handler(content);
        h = h.Defaults(
            compression: Compression,
            secureUpgrade: SecureUpgrade,
            strictTransport: StrictTransport,
            clientCaching: ClientCaching,
            rangeSupport: RangeSupport,
            preventSniffing: PreventSniffing
        );
        return h;
    }
    
    protected virtual void StartAddCors(LayoutBuilder layout)
    {
        if (!IsEnabledCors) return;
        
        if (CorsPolicies.Count == 0)
        {
            layout.Add(CorsPolicy.Permissive());
            return;
        }
        
        var corsPolicy = CorsPolicy.Restrictive();
        foreach (var (origin, policy) in CorsPolicies)
        {
            corsPolicy.Add(origin, policy);
        }
        
        layout.Add(corsPolicy);
    }
    
    protected virtual List<string> StartAddResourcesFromAssembly() => [];
    
    protected virtual List<(string Path, object Controller)> StartAddControllers() => [];
    
    protected virtual ITheme? StartAddTheme() => null;
    
    protected virtual void StartConfigure(LayoutBuilder layout) {}
    
    
    #endregion Start
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // release managed resources here
            var h = host;
            host = null;
            if (h != null)
            {
                try
                {
                    h.Stop();
                }
                catch (Exception e)
                {
                    log.LogDebugMethod(new(), e, "Error disposing of host {Host}", h.GetType().FullNameFormatted());
                }
            }
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private sealed class LoggingCompanion(ILogger log) : IServerCompanion
    {
        public void OnRequestHandled(IRequest request, IResponse response)
        {
            log.LogDebug("REQUEST - {Source} - {Method} {Path} - {Status} - {ContentLength}",
                request.Client.IPAddress,
                request.Method.RawMethod,
                request.Target.Path,
                response.Status.RawStatus,
                response.ContentLength ?? 0
            );
        }
        
        public void OnServerError(ServerErrorScope scope, Exception error)
        {
            log.LogError(error, "ERROR - {Scope} - {Error}",
                scope,
                error.Message
            );
        }
        
    }
}
