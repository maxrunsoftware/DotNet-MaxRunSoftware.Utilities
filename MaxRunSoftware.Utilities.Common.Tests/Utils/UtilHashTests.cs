// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Common.Tests.Utils;

public class UtilHashTests : TestBase
{
    public UtilHashTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Fact]
    public void HashString()
    {
        var hash = Util.GenerateHashSHA256(Constant.Encoding_UTF8.GetBytes("some_text"));
        output.WriteLine(hash);

        hash = Util.GenerateHashSHA256(new byte[] {});
        output.WriteLine(hash);
        // 9476537661fea5f2c41aebe7e0ea1d7e933051129ae6e9374e34d9db8d7a3be6
        // 0000000000000000000000000000000000000000000000000000000000000000
    }
}
