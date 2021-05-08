namespace JWLMerge.Models
{
    using System.Windows.Media;

    internal class ColourDef
    {
        public ColourDef(int colourIndex, string name, string rgb)
        {
            ColourIndex = colourIndex;
            Name = name;
            RgbString = rgb;
        }

        public int ColourIndex { get; set; }

        public string Name { get; set; }

        public string RgbString { get; set; }

        public Color Color => (Color)ColorConverter.ConvertFromString(RgbString);
    }
}
