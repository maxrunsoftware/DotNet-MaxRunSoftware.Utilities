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

namespace MaxRunSoftware.Utilities.Web.Server;

public class WebServer : IDisposable
{
    private readonly ILogger log;
    private readonly object locker = new();
    private object? server;

    public WebServer(ILoggerProvider loggerProvider, string host, ushort port, string? username, string? password)
    {
        log = loggerProvider.CreateLogger<WebServer>();
        log.LogDebugMethod(new(host, port, username), "Creating new {Type}", typeof(WebServer).FullNameFormatted());
    }

    public void Dispose()
    {
        object? s;
        lock (locker)
        {
            s = server;
            server = null;
        }

        if (s != null)
        {
            log.LogDebugMethod(new(), nameof(Dispose) + "() called, disposing of {Type}", typeof(WebServer).FullNameFormatted());
            //s.DisposeSafely(log);
        }
    }
}

public static class WebServerExtensions
{

}
