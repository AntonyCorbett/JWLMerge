using System.Windows.Media;

namespace JWLMerge.Models;

internal sealed class ColourDef
{
    public ColourDef(int colourIndex, string name, string rgb)
    {
        ColourIndex = colourIndex;
        Name = name;
        RgbString = rgb;
    }

    public int ColourIndex { get; }

    public string Name { get; }

    public string RgbString { get; }

    public Color Color => (Color)ColorConverter.ConvertFromString(RgbString);
}