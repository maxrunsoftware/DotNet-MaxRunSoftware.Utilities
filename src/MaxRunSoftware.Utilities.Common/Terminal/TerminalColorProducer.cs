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

namespace MaxRunSoftware.Utilities.Common;

public interface ITerminalColorProducer
{
    public TerminalColor Next();
}

public class TerminalColorProducerRainbow : ITerminalColorProducer
{
    private readonly double frequency;
    public int Index { get; set; }

    public TerminalColorProducerRainbow(Percent percent) : this((double)percent * 0.01) { }
    public TerminalColorProducerRainbow(double frequency) => this.frequency = frequency;

    public TerminalColor Next()
    {
        var i = Index++;

        // ReSharper disable once InlineTemporaryVariable
        var f = frequency;

        var r = Convert.ToByte(Math.Round(Math.Sin(f * i + 0) * 127 + 128));
        var g = Convert.ToByte(Math.Round(Math.Sin(f * i + 2) * 127 + 128));
        var b = Convert.ToByte(Math.Round(Math.Sin(f * i + 4) * 127 + 128));

        return TerminalColor.GetTerminalColor(r, g, b);
    }
}
