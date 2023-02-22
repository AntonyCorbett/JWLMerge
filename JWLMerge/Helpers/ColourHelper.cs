using System.Collections.Generic;
using System.Linq;
using JWLMerge.BackupFileServices.Models.DatabaseModels;
using JWLMerge.Models;

namespace JWLMerge.Helpers;

internal static class ColourHelper
{
    public static ColourDef[] GetHighlighterColours(bool includeAllColoursItem)
    {
        var result = new List<ColourDef>();

        if (includeAllColoursItem)
        {
            result.Add(new ColourDef(0, "All Colours", "#FFFFFF"));
        }

        result.AddRange(new[]
        {
            new ColourDef(1, "Yellow", "#FFF3A8"),
            new ColourDef(2, "Green", "#DAF1C8"),
            new ColourDef(3, "Blue", "#CBEBFF"), 
            new ColourDef(4, "Lilac", "#DFD2F0"),
            new ColourDef(5, "Rose", "#FACBDD"),
            new ColourDef(6, "Peach", "#FFDCC4"),
        });

        return result.ToArray();
    }

    public static ColourDef[] GetHighlighterColoursInUse(List<UserMark> userMarks, bool includeAllColoursItem)
    {
        var allColors = GetHighlighterColours(includeAllColoursItem);
        var colorsInUse = userMarks.Select(x => x.ColorIndex).Distinct().ToArray();
        return allColors.Where(x => colorsInUse.Contains(x.ColourIndex) || x.ColourIndex == 0).ToArray();
    }
}