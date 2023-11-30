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

using System.Net.Sockets;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Sessions;
using LogLevelSwan = Swan.Logging.LogLevel;
using LogLevelMicrosoft = Microsoft.Extensions.Logging.LogLevel;
using ILoggerSwan = Swan.Logging.ILogger;
// ReSharper disable InconsistentlySynchronizedField

namespace MaxRunSoftware.Utilities.Web.Server;

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

        public void Log(Swan.Logging.LogMessageReceivedEventArgs logEvent)
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
                Swan.Logging.Logger.NoLogging();
                foreach (var logLevelSwan in LOG_LEVELS_MAP.Keys)
                {
                    var loggerSwan = new SwanLogger(logLevelSwan, logger);
                    Swan.Logging.Logger.RegisterLogger(loggerSwan);
                }
            }
        }

    }

    private readonly ILogger log;
    private readonly object locker = new();

    private readonly SingleUse disposable;

    public WebServerOptions ServerOptions { get; set; }

    public EmbedIO.WebServer? Server { get; private set; }

    public Func<WebServerHttpContext, Task>? Handler { get; set;  }
    public Func<WebServerHttpContextException, Task>? HandlerException { get; set; }

    public int ResponseDelayMilliseconds { get; set; } = 100;

    public ushort Port { get; set; } = 8080;

    public ISessionManager? SessionManager { get; set; }

    public IList<string> Hostnames { get; set; }

    public WebServer(ILoggerProvider loggerProvider)
    {
        disposable = new(locker);
        log = loggerProvider.CreateLogger<WebServer>();
        SwanLogger.RegisterLoggers(loggerProvider.CreateLogger<EmbedIO.WebServer>());

        ServerOptions = new WebServerOptions()
                .WithMode(HttpListenerMode.EmbedIO)
            ;

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
            log.LogWarning(e, "Unable to obtain local IP address list for {Type}.{Property}", GetType().FullNameFormatted(), nameof(Hostnames));
        }
        hostnames.Add("localhost");
        hostnames.Add("127.0.0.1");

        Hostnames = hostnames
            .Select(o => o.TrimOrNull())
            .WhereNotNull()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(o => o, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }



    public void Start()
    {
        lock (locker)
        {
            StartInternal();
        }
    }

    private void StartInternal()
    {
        log.LogDebugMethod(new() ,"Starting");

        foreach (var hostname in Hostnames.OrderBy(o => o, StringComparer.OrdinalIgnoreCase))
        {
            var urlPrefix = $"http://{hostname}:{Port}";
            log.LogDebugMethod(new(), "Registering URL {Url}", urlPrefix);
            ServerOptions.AddUrlPrefix(urlPrefix);
        }

        EmbedIO.WebServer s = new(ServerOptions);

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
        Server.RunAsync();

        log.LogDebugMethod(new(), "Started");
    }

    private async Task HandleHttp(IHttpContext context) => await HandleHttp(context: context, exception: null);

    private async Task HandleHttpException(IHttpContext context, IHttpException exception) => await HandleHttp(context: context, exception: exception);

    private async Task HandleHttp(IHttpContext context, IHttpException? exception)
    {
        log.LogDebug("HTTP {IsException}[{HttpVerb}] request for {RequestedPath}{Exception}",
            exception == null ? "" : "Exception ",
            context.Request.HttpVerb,
            context.RequestedPath,
            exception == null ? "" : $": {exception}"
        );

        await Task.Delay(ResponseDelayMilliseconds);

        if (exception == null)
        {
            var ctx = new WebServerHttpContext(context);
            var handler = Handler;
            if (handler != null)
            {
                await handler(ctx);
            }
            else
            {
                log.LogError("No {Type}.{Handler} defined", GetType().FullNameFormatted(), nameof(Handler));
                await ctx.SendStringHtmlSimpleAsync("ERROR", $"<p>No {nameof(Handler)} defined</p>");
            }

        }
        else
        {
            var ctx = new WebServerHttpContextException(context, exception);

            var handlerException = HandlerException;
            if (handlerException != null)
            {
                await handlerException(ctx);
            }
            else
            {
                log.LogWarning("No {Type}.{Handler} defined", GetType().FullNameFormatted(), nameof(HandlerException));
                ctx.SetStatus(exception.StatusCode);
                switch (exception.StatusCode)
                {
                    case 404:
                        await ctx.SendStringHtmlSimpleAsync("404 - Not Found", $"<p>Path {context.RequestedPath} not found</p>");
                        break;
                    case 401:
                        ctx.AddHeader("WWW-Authenticate", "Basic");
                        await ctx.SendStringHtmlSimpleAsync("401 - Unauthorized", "<p>Please login to continue</p>");
                        break;
                    default:
                        await HttpExceptionHandler.Default(context, exception);
                        break;
                }
            }
        }
    }

    public void Dispose()
    {
        lock (locker)
        {
            DisposeInternal();
        }
    }

    private void DisposeInternal()
    {
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
