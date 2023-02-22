using System.Collections.Generic;
using System.Linq;
using JWLMerge.BackupFileServices.Models.DatabaseModels;

namespace JWLMerge.Helpers;

internal static class TagHelper
{
    public static Tag[] GetTags(List<Tag> tags)
    {
        return tags.Where(x => x.Type == 1).OrderBy(x => x.Name).ToArray();
    }

    public static Tag[] GetTagsInUseByNotes(Database database)
    {
        var tagIds = database.TagMaps.Where(x => x.NoteId != null).Select(x => x.TagId).ToHashSet();
        return database.Tags.Where(x => x.Type == 1 && tagIds.Contains(x.TagId)).OrderBy(x => x.Name).ToArray();
    }

    public static bool AnyNotesHaveNoTag(Database database)
    {
        var notesThatAreTagged = database.TagMaps.Where(x => x.NoteId != null).Select(x => x.NoteId).ToHashSet();
        return database.Notes.Any(x => !notesThatAreTagged.Contains(x.NoteId));
    }
}