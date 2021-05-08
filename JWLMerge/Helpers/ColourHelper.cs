namespace JWLMerge.Helpers
{
    using JWLMerge.Models;

    internal static class ColourHelper
    {
        public static ColourDef[] GetHighlighterColours()
        {
            return new[]
            {
                new ColourDef(1, "Yellow", "#FFF3A8"),
                new ColourDef(2, "Green", "#DAF1C8"),
                new ColourDef(3, "Blue", "#CBEBFF"), 
                new ColourDef(4, "Lilac", "#DFD2F0"),
                new ColourDef(5, "Rose", "#FACBDD"),
                new ColourDef(6, "Peach", "#FFDCC4"),
            };
        }
    }
}
