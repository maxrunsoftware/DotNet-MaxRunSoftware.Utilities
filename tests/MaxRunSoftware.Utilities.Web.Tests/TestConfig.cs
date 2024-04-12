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

namespace MaxRunSoftware.Utilities.Web.Tests;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo
public static class TestConfig
{
    public static readonly ImmutableArray<SkippedTest> IGNORED_TESTS = new[]
    {
        SkippedTest.Create<WebServerTests>(nameof(WebServerTests.WebServer_Run), "just runs the web server"),
    }.ToImmutableArray();


    public static readonly string DEFAULT_HOST = "localhost";
    public static readonly ushort DEFAULT_PORT = 32913;

    public static readonly string DEFAULT_USERNAME = "test";
    public static readonly string DEFAULT_PASSWORD = "testPass1!";

    public static readonly string URL_BASE = $"http://{DEFAULT_HOST}:{DEFAULT_PORT}";
}
