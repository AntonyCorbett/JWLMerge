using JWLMerge.BackupFileServices.Exceptions;
using JWLMerge.BackupFileServices.Models.DatabaseModels;

namespace JWLMerge.BackupFileServices.Helpers;

internal static class DatabaseForeignKeyChecker
{
    public static void Execute(Database database)
    {
        CheckBlockRangeValidity(database);
        CheckBookmarkValidity(database);
        CheckInputFieldValidity(database);
        CheckNoteValidity(database);
        CheckTagMapValidity(database);
        CheckUserMarkValidity(database);
    }

    private static void CheckBlockRangeValidity(Database database)
    {
        foreach (var range in database.BlockRanges)
        {
            if (database.FindUserMark(range.UserMarkId) == null)
            {
                throw new BackupFileServicesException($"Could not find user mark Id for block range {range.BlockRangeId}");
            }
        }
    }

    private static void CheckBookmarkValidity(Database database)
    {
        foreach (var bookmark in database.Bookmarks)
        {
            if (database.FindLocation(bookmark.LocationId) == null ||
                database.FindLocation(bookmark.PublicationLocationId) == null)
            {
                throw new BackupFileServicesException($"Could not find location for bookmark {bookmark.BookmarkId}");
            }
        }
    }

    private static void CheckInputFieldValidity(Database database)
    {
        foreach (var inputField in database.InputFields)
        {
            if (database.FindLocation(inputField.LocationId) == null)
            {
                throw new BackupFileServicesException($"Could not find a location for input field {inputField.Value}");
            }
        }
    }

    private static void CheckNoteValidity(Database database)
    {
        foreach (var note in database.Notes)
        {
            if (note.UserMarkId != null && database.FindUserMark(note.UserMarkId.Value) == null)
            {
                throw new BackupFileServicesException($"Could not find user mark Id for note {note.NoteId}");
            }

            if (note.LocationId != null && database.FindLocation(note.LocationId.Value) == null)
            {
                throw new BackupFileServicesException($"Could not find location for note {note.NoteId}");
            }
        }
    }

    private static void CheckTagMapValidity(Database database)
    {
        foreach (var tagMap in database.TagMaps)
        {
            if (database.FindTag(tagMap.TagId) == null)
            {
                throw new BackupFileServicesException($"Could not find tag for tag map {tagMap.TagMapId}");
            }

            if (tagMap.NoteId != null && database.FindNote(tagMap.NoteId.Value) == null)
            {
                throw new BackupFileServicesException($"Could not find note for tag map {tagMap.TagMapId}");
            }

            if (tagMap.LocationId != null && database.FindLocation(tagMap.LocationId.Value) == null)
            {
                throw new BackupFileServicesException($"Could not find location for tag map {tagMap.TagMapId}");
            }
        }
    }

    private static void CheckUserMarkValidity(Database database)
    {
        foreach (var userMark in database.UserMarks)
        {
            if (database.FindLocation(userMark.LocationId) == null)
            {
                throw new BackupFileServicesException($"Could not find location for user mark {userMark.UserMarkId}");
            }
        }
    }
}