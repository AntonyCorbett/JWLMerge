namespace JWLMerge.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;
    using JWLMerge.Models;

    internal class PublicationHelper
    {
        public static PublicationDef[] GetPublications(List<Location> locations, List<UserMark> userMarks)
        {
            var locationsThatAreMarked = userMarks.Select(x => x.LocationId).ToHashSet();

            return locations
                .Where(x => locationsThatAreMarked.Contains(x.LocationId))
                .Select(x => x.KeySymbol).Distinct().Select(
                    x => new PublicationDef
                    {
                        KeySymbol = x,
                    }).OrderBy(x => x.KeySymbol).ToArray();
        }
    }
}
