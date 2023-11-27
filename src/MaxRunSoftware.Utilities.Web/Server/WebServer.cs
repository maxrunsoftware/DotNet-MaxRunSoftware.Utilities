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
using LogLevelSwan = Swan.Logging.LogLevel;
using LogLevelMicrosoft = Microsoft.Extensions.Logging.LogLevel;
using ILoggerSwan = Swan.Logging.ILogger;

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
    private EmbedIO.WebServer? server;
    private readonly SingleUse disposable = new();
    private Func<WebServerHttpContext, Task> Handler { get; }
    public int ResponseDelayMilliseconds { get; set; } = 100;

    public WebServer(ILoggerProvider loggerProvider, ushort port, Func<WebServerHttpContext, Task> handler)
    {
        log = loggerProvider.CreateLogger<WebServer>();
        Handler = handler;
        SwanLogger.RegisterLoggers(log);

        log.LogDebug("{Type}: Starting", GetType().FullNameFormatted());

        var urlPrefixes = Util.NetGetIPAddresses()
            .Where(o => o.AddressFamily == AddressFamily.InterNetwork)
            .Select(ip => ip.ToString())
            .Concat(new[] {"localhost", "127.0.0.1"})
            .Select(o => o.TrimOrNull())
            .WhereNotNull()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(o => o, StringComparer.OrdinalIgnoreCase)
            .Select(o => $"http://{o}:{port}")
            .ToList();

        foreach (var urlPrefix in urlPrefixes)
        {
            log.LogDebug("{Type}: Registering URL {Url}", GetType().FullNameFormatted(), urlPrefix);
        }

        EmbedIO.WebServer s = new(o => o
            .WithUrlPrefixes(urlPrefixes)
            .WithMode(HttpListenerMode.EmbedIO)
        );

        s = s.WithLocalSessionManager();

        var am = new ActionModule(HandleHttp);
        s = s.WithModule(am);

        s.StateChanged += (_, e) => log.LogDebug("WebServer New State - {WebServerState}", e.NewState);

        s.HandleHttpException(HandleHttpException);

        server = s;
        server.RunAsync();

        log.LogDebug("{Type}: Started", GetType().FullNameFormatted());
    }

    private async Task HandleHttp(IHttpContext context)
    {
        log.LogDebug("HTTP [{HttpVerb}] request for {RequestedPath}", context.Request.HttpVerb, context.RequestedPath);
        await Task.Delay(ResponseDelayMilliseconds);

        var c = new WebServerHttpContext(context);
        await Handler(c);
    }

    private async Task HandleHttpException(IHttpContext context, IHttpException exception)
    {
        log.LogDebug("HTTP Exception for {RequestedPath}  {Exception}", context.RequestedPath, exception);
        await Task.Delay(ResponseDelayMilliseconds);

        context.Response.StatusCode = exception.StatusCode;
        switch (exception.StatusCode)
        {
            case 404:
                await context.SendStringHtmlSimpleAsync("404 - Not Found", $"<p>Path {context.RequestedPath} not found</p>");
                break;
            case 401:
                context.AddHeader("WWW-Authenticate", "Basic");
                await context.SendStringHtmlSimpleAsync("401 - Unauthorized", "<p>Please login to continue</p>");
                break;
            default:
                await HttpExceptionHandler.Default(context, exception);
                break;
        }
    }

    public void Dispose()
    {
        if (!disposable.TryUse()) return;

        log.LogDebug("Shutting down web server");
        var s = server;
        server = null;
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
