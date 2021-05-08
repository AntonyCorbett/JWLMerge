namespace JWLMerge.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Models;

    internal static class RemoveNotesByTagHelper
    {
        public static async Task<int> ExecuteAsync(
            BackupFile backupFile, IBackupFileService backupFileService, string filePath, int[] tagIds, bool removeAssociatedUnderlining)
        {
            if (backupFile == null)
            {
                return 0;
            }

            var tagIdsHash = tagIds.ToHashSet();
            
            var result = await Task.Run(() =>
            {
                var tagMapIdsToRemove = new HashSet<int>();
                var noteIdsToRemove = new HashSet<int>();
                var candidateUserMarks = new HashSet<int>();

                if (tagIds.Contains(-1))
                {
                    // notes without a tag
                    var notesWithoutTag = GetNotesWithNoTag(backupFile).ToArray();
                    foreach (var i in notesWithoutTag)
                    {
                        noteIdsToRemove.Add(i);
                    }
                }

                foreach (var tagMap in backupFile.Database.TagMaps)
                {
                    if (tagIdsHash.Contains(tagMap.TagId) && tagMap.NoteId != null && tagMap.NoteId > 0)
                    {
                        tagMapIdsToRemove.Add(tagMap.TagMapId);
                        noteIdsToRemove.Add(tagMap.NoteId.Value);

                        var note = backupFile.Database.FindNote(tagMap.NoteId.Value);
                        if (note?.UserMarkId != null)
                        {
                            candidateUserMarks.Add(note.UserMarkId.Value);
                        }
                    }
                }

                backupFile.Database.TagMaps.RemoveAll(x => tagMapIdsToRemove.Contains(x.TagMapId));
                backupFile.Database.Notes.RemoveAll(x => noteIdsToRemove.Contains(x.NoteId));

                if (removeAssociatedUnderlining)
                {
                    foreach (var note in backupFile.Database.Notes)
                    {
                        if (note.UserMarkId != null && candidateUserMarks.Contains(note.UserMarkId.Value))
                        {
                            // we can't delete this user mark because it is still in use (a user mark
                            // may have multiple associated notes.
                            candidateUserMarks.Remove(note.UserMarkId.Value);
                        }
                    }

                    backupFile.Database.UserMarks.RemoveAll(x => candidateUserMarks.Contains(x.UserMarkId));
                }

                // Note that redundant block ranges are cleaned automatically by Cleaner.

                if (noteIdsToRemove.Count > 0 || tagMapIdsToRemove.Count > 0)
                {
                    backupFileService.WriteNewDatabaseWithClean(backupFile, filePath, filePath);
                }

                return noteIdsToRemove.Count;
            });

            return result;
        }

        private static IEnumerable<int> GetNotesWithNoTag(BackupFile backupFile)
        {
            var notesWithTags = backupFile.Database.TagMaps.Select(x => x.NoteId).Distinct().ToHashSet();

            foreach (var note in backupFile.Database.Notes)
            {
                if (!notesWithTags.Contains(note.NoteId))
                {
                    yield return note.NoteId;
                }
            }
        }
    }
}
