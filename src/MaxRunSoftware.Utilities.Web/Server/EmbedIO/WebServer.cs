// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

using System.Net.Sockets;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Sessions;
using Swan.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevelSwan = Swan.Logging.LogLevel;
using LogLevelMicrosoft = Microsoft.Extensions.Logging.LogLevel;
using ILoggerSwan = Swan.Logging.ILogger;
using EmbedIOWebServer = EmbedIO.WebServer;
using MaxRunSoftware.Utilities.Web.Server;

// ReSharper disable InconsistentlySynchronizedField

namespace MaxRunSoftware.Utilities.Web.Server.EmbedIO;

public class WebServer : IDisposable
{
    private sealed class SwanLogger : ILoggerSwan
    {
        public LogLevelSwan LogLevel { get; }
        private readonly ILogger logger;
        private static readonly object loggerRegistrationLocker = new();
        private static readonly SingleUse loggerRegistrationIsComplete = new();

        private static readonly ImmutableDictionary<LogLevelSwan, LogLevelMicrosoft> LOG_LEVELS_MAP = new Dictionary<LogLevelSwan, LogLevelMicrosoft>
        {
            { LogLevelSwan.Trace, LogLevelMicrosoft.Trace },
            { LogLevelSwan.Debug, LogLevelMicrosoft.Debug },
            { LogLevelSwan.Info, LogLevelMicrosoft.Information },
            { LogLevelSwan.Warning, LogLevelMicrosoft.Warning },
            { LogLevelSwan.Error, LogLevelMicrosoft.Error },
            { LogLevelSwan.Fatal, LogLevelMicrosoft.Critical },
            { LogLevelSwan.None, LogLevelMicrosoft.None },
        }.ToImmutableDictionary();

        private SwanLogger(LogLevelSwan logLevel, ILogger logger)
        {
            LogLevel = logLevel;
            this.logger = logger;
        }

        public void Dispose() { }

        public void Log(LogMessageReceivedEventArgs logEvent)
        {
            if (logEvent.MessageType != LogLevel) return;
            var logLevelMicrosoft = LOG_LEVELS_MAP[logEvent.MessageType];
            if (logEvent.Exception == null)
            {
                logger.Log(logLevelMicrosoft, "{Message}", logEvent.Message);
            }
            else
            {
                logger.Log(logLevelMicrosoft, logEvent.Exception, "{Message}", logEvent.Message);
            }
        }

        public static void RegisterLoggers(ILogger logger)
        {
            lock (loggerRegistrationLocker)
            {
                if (!loggerRegistrationIsComplete.TryUse()) return;
                logger.LogDebug("Registering once, static logging for web server {Type}", typeof(EmbedIO.WebServer).FullNameFormatted());
                Logger.NoLogging();
                foreach (var logLevelSwan in LOG_LEVELS_MAP.Keys)
                {
                    var loggerSwan = new SwanLogger(logLevelSwan, logger);
                    Logger.RegisterLogger(loggerSwan);
                }
            }
        }
    }

    private readonly ILoggerFactory loggerProvider;
    private readonly ILogger log;
    private readonly object locker = new();
    private readonly SingleUse disposable;

    public WebServerOptions ServerOptions { get; set; }
    public EmbedIOWebServer? Server { get; private set; }
    public int ResponseDelayMilliseconds { get; set; } = 100;

    public Func<WebServerHttpContext, Task>? Handler { get; set; }

    public ushort Port { get; set; } = 8080;

    public ISessionManager? SessionManager { get; set; }

    public IWebServerAuthenticationBasicHandler? BasicAuthenticationHandler { get; set; }

    public IList<string> Hostnames { get; set; }

    private static List<string> HostnamesDefaults(ILogger log)
    {
        var hostnames = new List<string>();
        try
        {
            foreach (var ip in Util.NetGetIPAddresses().Where(o => o.AddressFamily == AddressFamily.InterNetwork))
            {
                hostnames.Add(ip.ToString());
            }
        }
        catch (Exception e)
        {
            log.LogWarning(e, "Unable to obtain local IP address list for {Type}.{Property}", typeof(WebServer).FullNameFormatted(), nameof(Hostnames));
        }

        hostnames.Add("localhost");
        hostnames.Add("127.0.0.1");

        var hostnames2 = hostnames
            .Select(o => o.TrimOrNull())
            .WhereNotNull()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(o => o, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return hostnames2;
    }

    public WebServer(ILoggerFactory loggerProvider)
    {
        disposable = new(locker);
        this.loggerProvider = loggerProvider;
        log = loggerProvider.CreateLogger<WebServer>();
        SwanLogger.RegisterLoggers(loggerProvider.CreateLogger<EmbedIO.WebServer>());

        ServerOptions = new WebServerOptions()
                .WithMode(HttpListenerMode.EmbedIO)
            ;

        Hostnames = HostnamesDefaults(log);
    }

    public Task Start()
    {
        log.LogDebugMethod(new(), "Starting");

        foreach (var hostname in Hostnames.OrderBy(o => o, StringComparer.OrdinalIgnoreCase))
        {
            var urlPrefix = $"http://{hostname}:{Port}";
            ServerOptions.AddUrlPrefix(urlPrefix);
        }

        foreach (var urlPrefix in ServerOptions.UrlPrefixes.OrderBy(o => o, StringComparer.OrdinalIgnoreCase))
        {
            log.LogDebugMethod(new(), "Registering URL {Url}", urlPrefix);
        }
        
        
        EmbedIOWebServer s = new EmbedIOWebServer(ServerOptions);

        var authBasic = new WebServerAuthenticationBasicModule(loggerProvider, () => BasicAuthenticationHandler);
        s.WithModule(authBasic);

        var sm = SessionManager;
        if (sm != null)
        {
            log.LogDebugMethod(new(), "Registering SessionManager {SessionManagerType}", sm.GetType().FullNameFormatted());
            s.SessionManager = sm;
        }


        var am = new ActionModule(HandleHttp);
        s = s.WithModule(am);

        s.StateChanged += (_, e) => log.LogDebug("{Type}: New State - {WebServerState}", typeof(EmbedIO.WebServer).FullNameFormatted(), e.NewState);

        s.HandleHttpException(HandleHttpException);

        Server = s;
        var task = Server.RunAsync();
        log.LogDebugMethod(new(), "Started");

        return task;
    }

    private Task HandleHttp(IHttpContext context) => HandleHttp(context, null);

    private Task HandleHttpException(IHttpContext context, IHttpException exception) => HandleHttp(context, exception);

    private async Task HandleHttp(IHttpContext context, IHttpException? exception)
    {
        log.LogDebug("HTTP {IsException}[{HttpVerb}] request for {RequestedPath}{Exception}",
            exception == null ? "" : "Exception ",
            context.Request.HttpVerb,
            context.RequestedPath,
            exception == null ? "" : $": {exception}"
        );

        await Task.Delay(ResponseDelayMilliseconds);

        var ctx = new WebServerHttpContext(context, exception);
        if (exception != null)
        {
            if (exception.StatusCode == 404)
            {
                await ctx.SendHtmlSimpleAsync("404 - Not Found", $"<p>Path {context.RequestedPath} not found</p>");
            }
            else if (exception.StatusCode == 401)
            {
                ctx.AddResponseHeader("WWW-Authenticate", "Basic");
                await ctx.SendHtmlSimpleAsync("401 - Unauthorized", "<p>Please login to continue</p>");
            }
            else
            {
                await HttpExceptionHandler.Default(context, exception);
            }
        }
        else
        {
            var handler = Handler;
            if (handler != null)
            {
                await handler(ctx);
            }
            else
            {
                log.LogError("No {Type}.{Handler} defined", GetType().FullNameFormatted(), nameof(Handler));
                await ctx.SendHtmlSimpleAsync("ERROR", $"<p>No {nameof(Handler)} defined</p>");
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;


        if (!disposable.TryUse()) return;

        log.LogDebug("Shutting down web server");
        var s = Server;
        Server = null;
        if (s == null) return;

        try
        {
            s.Dispose();
        }
        catch (Exception e)
        {
            log.LogWarning(e, "Error disposing of {Type}", s.GetType().FullNameFormatted());
        }

        log.LogDebug("Web server shut down");
    }
}
