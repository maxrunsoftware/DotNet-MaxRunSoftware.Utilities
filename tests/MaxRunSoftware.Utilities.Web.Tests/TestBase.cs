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

using System.Net;
using System.Net.Http;
using MaxRunSoftware.Utilities.Web.Server;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute

namespace MaxRunSoftware.Utilities.Web.Tests;

public class TestBase(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper, TestConfig.IGNORED_TESTS)
{
    protected readonly HttpIO httpIO = new();

    protected WebServer CreateWebServer()
    {
        var ws = new WebServer(LoggerProvider);
        ws.Port = TestConfig.DEFAULT_PORT;
        return ws;
    }

    protected WebServer StartWebServer(Func<WebServerHttpContext, Task> handler)
    {
        var ws = CreateWebServer();
        ws.Handler = handler;
        ws.Start();
        return ws;
    }

    /// <summary>
    /// https://stackoverflow.com/a/27108442
    /// </summary>
    protected class HttpIO
    {
        public HttpClient Client { get; }
        public CookieContainer CookieContainer { get; }
        public HttpIO()
        {
            CookieContainer = new();
            Client = new(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                CheckCertificateRevocationList = false,
                ClientCertificateOptions = ClientCertificateOption.Manual,
                MaxRequestContentBufferSize = 0,
                ServerCertificateCustomValidationCallback = (
                        /* message */ _,
                        /* certificate2 */ _,
                        /* chain */ _,
                        /* sslPolicyErrors */ _)
                    => true,
                UseCookies = true,
                AllowAutoRedirect = false,
                CookieContainer = CookieContainer,
            });
        }


        public async Task<string> GetAsync(string uri)
        {
            using var response = await Client.GetAsync(uri);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PostAsync(string uri, string data, string contentType)
        {
            using HttpContent content = new StringContent(data, Encoding.UTF8, contentType);

            var requestMessage = new HttpRequestMessage
            {
                Content = content,
                Method = HttpMethod.Post,
                RequestUri = new(uri),
            };

            using var response = await Client.SendAsync(requestMessage);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
