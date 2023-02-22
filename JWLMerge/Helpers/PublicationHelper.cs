using System.Collections.Generic;
using System.Linq;
using JWLMerge.BackupFileServices.Models.DatabaseModels;
using JWLMerge.Models;

namespace JWLMerge.Helpers;

internal static class PublicationHelper
{
    public static PublicationDef[] GetPublications(
        List<Location> locations, List<UserMark> userMarks, bool includeAllPublicationsItem)
    {
        var locationsThatAreMarked = userMarks.Select(x => x.LocationId).ToHashSet();

        var result = locations
            .Where(x => locationsThatAreMarked.Contains(x.LocationId) && !string.IsNullOrEmpty(x.KeySymbol))
            .Select(x => x.KeySymbol).Distinct()
            .Select(x => new PublicationDef(x!, false))
            .OrderBy(x => x.KeySymbol).ToList();

        if (includeAllPublicationsItem)
        {
            result.Insert(0, new PublicationDef("All Publications", true));
        }

        return result.ToArray();
    }
}