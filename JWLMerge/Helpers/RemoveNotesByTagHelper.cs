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
            BackupFile backupFile, IBackupFileService backupFileService, string filePath, int[] tagIds)
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
                var userMarksToRemove = new HashSet<int>();

                foreach (var tagMap in backupFile.Database.TagMaps)
                {
                    if (tagIdsHash.Contains(tagMap.TagId) && tagMap.NoteId != null && tagMap.NoteId > 0)
                    {
                        tagMapIdsToRemove.Add(tagMap.TagMapId);
                        noteIdsToRemove.Add(tagMap.NoteId.Value);

                        var note = backupFile.Database.FindNote(tagMap.NoteId.Value);
                        if (note?.UserMarkId != null)
                        {
                            userMarksToRemove.Add(note.UserMarkId.Value);
                        }
                    }
                }

                backupFile.Database.TagMaps.RemoveAll(x => tagMapIdsToRemove.Contains(x.TagMapId));
                backupFile.Database.Notes.RemoveAll(x => noteIdsToRemove.Contains(x.NoteId));
                backupFile.Database.UserMarks.RemoveAll(x => userMarksToRemove.Contains(x.UserMarkId));

                // Note that redundant block ranges are cleaned automatically by Cleaner.

                if (noteIdsToRemove.Count > 0 || tagMapIdsToRemove.Count > 0)
                {
                    backupFileService.WriteNewDatabaseWithClean(backupFile, filePath, filePath);
                }

                return noteIdsToRemove.Count;
            });

            return result;
        }
    }
}
