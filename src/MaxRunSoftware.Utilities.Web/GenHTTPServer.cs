// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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

using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Security.Cors;

namespace MaxRunSoftware.Utilities.Web;

public class GenHTTPServer(ILogger log) : IDisposable
{
    protected readonly ILogger log = log;
    
    public IServerHost? Host { get; protected set; }
    
    #region Options
    
    public virtual bool DebugIsEnabled { get; set; } = Constant.IsDebug;
    
    public virtual ushort Port { get; set; } = 8080;
    
    public virtual bool CorsIsEnabled { get; set; }
    public virtual OriginPolicy? CorsPolicyDefault { get; set; }
    public virtual IDictionary<string, OriginPolicy> CorsPolicies { get; set; } = new Dictionary<string, OriginPolicy>(StringComparer.OrdinalIgnoreCase);
    
    public virtual X509Certificate2? Certificate { get; set; }
    
    public virtual bool Compression { get; set; } = true;
    public virtual bool SecureUpgrade { get; set; } = true;
    public virtual bool StrictTransport { get; set; } = true;
    public virtual bool ClientCaching { get; set; } = true;
    public virtual bool RangeSupport { get; set; } = false;
    public virtual bool PreventSniffing { get; set; } = false;
    
    public virtual TimeSpan RequestReadTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public virtual uint RequestMemoryLimit { get; set; } = (uint)(Constant.Bytes_Mebi * 100L);
    public virtual uint TransferBufferSize { get; set; } = Constant.BufferSize_Optimal;
    
    #endregion Options
    
    #region Setup
    
    protected virtual T AddCors<T>(T builder) where T : IServerBuilder<T>
    {
        if (!CorsIsEnabled) return builder;
        
        if (CorsPolicies.Count == 0)
        {
            builder.Add(CorsPolicy.Permissive());
            return builder;
        }
        
        var corsPolicy = CorsPolicy.Restrictive();
        foreach (var (origin, policy) in CorsPolicies)
        {
            corsPolicy.Add(origin, policy);
        }
        
        builder.Add(corsPolicy);
        
        return builder;
    }
    
    protected virtual IServerHost CreateHost()
    {
        var h = GenHTTP.Engine.Host.Create();
        h = h.Development(developmentMode: DebugIsEnabled);
        h = h.AddLogging(log);
        h = AddCors(h);
        
        h = h.Port(Port);
        h = h.Defaults(
            compression: Compression,
            secureUpgrade: SecureUpgrade,
            strictTransport: StrictTransport,
            clientCaching: ClientCaching,
            rangeSupport: RangeSupport,
            preventSniffing: PreventSniffing
        );
        h = h.RequestMemoryLimit(RequestMemoryLimit);
        h = h.RequestReadTimeout(RequestReadTimeout);
        h = h.TransferBufferSize(TransferBufferSize);
        return h;
    }

    #endregion Setup
    
    public virtual void Start()
    {
        if (Host != null) throw new InvalidOperationException("Already running");
        Host = CreateHost();
        try
        {
            Host.Start();
        }
        catch (Exception)
        {
            Dispose();
            throw;
        }
    }
    
    public virtual void Run(CancellationToken? cancellationToken = null) {
        if (Host != null) throw new InvalidOperationException("Already running");
        Host = CreateHost();
        
        var manualResetEvent = new ManualResetEvent(false);
        try
        {
            AppDomain.CurrentDomain.ProcessExit += (_, __) => { manualResetEvent.Set(); };
        }
        catch (Exception e)
        {
            log.LogWarning(e, "Could not set " + nameof(AppDomain) + "." + nameof(AppDomain.CurrentDomain) + "." + nameof(AppDomain.CurrentDomain.ProcessExit) + " event to Dispose");
        }
        
        var cancellationTokenRegistration = cancellationToken?.Register(() => manualResetEvent.Set());
        
        try
        {

            Host.Start();
            
            try
            {
                if (cancellationToken != null) cancellationToken.Value.ThrowIfCancellationRequested();
                manualResetEvent.WaitOne();
            }
            finally
            {
                try
                {
                    if (cancellationTokenRegistration != null) cancellationTokenRegistration.Value.Dispose();
                }
                catch (Exception ee)
                {
                    log.LogWarning(ee, "Error " + nameof(CancellationTokenRegistration) + "." + nameof(CancellationTokenRegistration.Dispose));
                }
                
                Dispose();
            }
        }
        catch (OperationCanceledException)
        {
            log.LogInformation("Stopping");
        }
        catch (Exception e)
        {
            log.LogError(e, "ERROR");
            if (Debugger.IsAttached) Debugger.Break();
            var companion = Host.Instance?.Companion;
            if (companion is not null)
            {
                companion.OnServerError(ServerErrorScope.General, null, e);
            }
            else
            {
                Console.WriteLine(e);
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // release managed resources here
            var h = Host;
            Host = null;
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
}
