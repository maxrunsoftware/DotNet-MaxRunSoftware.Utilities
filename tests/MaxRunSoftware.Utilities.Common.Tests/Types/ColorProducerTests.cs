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

using System.Drawing;

namespace MaxRunSoftware.Utilities.Common.Tests.Types;

public class ColorProducerTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
    [SkippableFact]
    public void ColorProducerRainbow_Works()
    {
        foreach (var percent in Percent.ValuesInt)
        {
            testOutputHelperWrapper.WriteLine(string.Empty);
            var p = new ColorProducerRainbow(percent);
            for (var i = 0; i < 10; i++)
            {
                var color = p.Next();
                var text = (TestMethodName ?? "???") + $" {percent.ToString(),3}%   R: {color.R.ToString(),3}   G: {color.G.ToString(),3}   B: {color.B.ToString(),3}";
                text = text.FormatTerminal(color, null);
                testOutputHelperWrapper.WriteLine(text);
            }
        }
    }
    
    public static TheoryData<Color, Color, int> ColorProducerGradiant_Works_Data
    {
        get
        {
            var stepList = Enumerable.Range(1, 10).ToList();
            List<Tuple<Color, Color>> colorsList = [
                new(Color.Red, Color.Green),
                new(Color.Red, Color.Blue),
            ];
            
            var data = new TheoryData<Color, Color, int>();
            foreach (var (startColor, endColor) in colorsList)
            {
                foreach (var step in stepList)
                {
                    data.Add(startColor, endColor, step);
                }
            }
            
            return data;
        }
    }
    
    [SkippableTheory]
    [MemberData(nameof(ColorProducerGradiant_Works_Data))]
    public void ColorProducerGradiant_Works(Color startColor, Color endColor, int steps)
    {
        var cp = new ColorProducerGradiant(startColor, endColor, steps);
        for (var i = 0; i < steps; i++)
        {
            IEnumerable<Color> c;
        }
        
        
        foreach (var percent in Percent.ValuesInt)
        {
            testOutputHelperWrapper.WriteLine(string.Empty);
            var p = new ColorProducerRainbow(percent);
            for (var i = 0; i < 10; i++)
            {
                var color = p.Next();
                var text = (TestMethodName ?? "???") + $" {percent.ToString(),3}%   R: {color.R.ToString(),3}   G: {color.G.ToString(),3}   B: {color.B.ToString(),3}";
                text = text.FormatTerminal(color, null);
                testOutputHelperWrapper.WriteLine(text);
            }
        }
    }
    
    
}
