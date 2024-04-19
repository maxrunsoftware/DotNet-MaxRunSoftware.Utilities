using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Security.Cors;

namespace MaxRunSoftware.Utilities.Web.Server.GenHTTP;

public class WebServerConfig
{
    public ushort Port { get; set; } = 8080;
    public IDictionary<string, OriginPolicy> CorsPolicies { get; set; } = new Dictionary<string, OriginPolicy>(StringComparer.OrdinalIgnoreCase);
    public bool IsEnabledCors { get; set; }
    public X509Certificate2? Certificate { get; set; }
}

public class WebServer : IDisposable
{
    private readonly ILogger log;
    private IServerHost? host;
    
    public WebServer(WebServerConfig config, ILogger log)
    {
        this.log = log;
        
        var lb = Layout.Create();
        if (config.IsEnabledCors)
        {
            if (config.CorsPolicies.Count == 0)
            {
                lb.Add(CorsPolicy.Permissive());
            }
            else
            {
                var corsPolicy = CorsPolicy.Restrictive();
                foreach (var (origin, policy) in config.CorsPolicies)
                {
                    corsPolicy.Add(origin, policy);
                }
                
                lb.Add(corsPolicy);
            }
        }
        
        host = Host.Create()
            .Port(config.Port)
            .Handler(lb)
            .Defaults()
            .Start();
    }
    
    public void Dispose()
    {
        var h = host;
        host = null;
        if (h == null) return;
        
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
