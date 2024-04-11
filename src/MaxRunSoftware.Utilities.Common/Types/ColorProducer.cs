using System.Drawing;

namespace MaxRunSoftware.Utilities.Common;

public interface IColorProducer
{
    public Color Next();
}

public class ColorProducerRainbow(double percent) : IColorProducer
{
    public int Index { get; set; }
    
    public ColorProducerRainbow(Percent percent) : this((double)percent * 0.01) { }
    
    public Color Next()
    {
        var i = Index++;
        
        // ReSharper disable once InlineTemporaryVariable
        var f = percent;
        
        var r = Convert.ToByte(Math.Round(Math.Sin(f * i + 0) * 127 + 128));
        var g = Convert.ToByte(Math.Round(Math.Sin(f * i + 2) * 127 + 128));
        var b = Convert.ToByte(Math.Round(Math.Sin(f * i + 4) * 127 + 128));
        
        
        return Color.FromArgb(r, g, b);
    }
}

public class ColorProducerGradiant(Color startColor, Color endColor, int steps) : IColorProducer
{
    public List<Color> Colors { get; } = startColor.Shift(endColor, steps);
    
    public int Index { get; set; }
    
    public Color Next()
    {
        if (Index >= Colors.Count) Index = 0;
        if (Index < 0) Index = 0;
        
        var c = Colors[Index];
        
        Index++;
        if (Index >= Colors.Count) Index = 0;
        
        return c;
    }
}
