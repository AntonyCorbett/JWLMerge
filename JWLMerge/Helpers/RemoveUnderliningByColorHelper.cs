namespace JWLMerge.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Models;

    internal static class RemoveUnderliningByColorHelper
    {
        public static async Task<int> ExecuteAsync(
            BackupFile backupFile, IBackupFileService backupFileService, string filePath, int[] colorIndexes, bool removeAssociatedNotes)
        {
            if (backupFile == null)
            {
                return 0;
            }

            var result = await Task.Run(() =>
            {
                var userMarkIdsToRemove = new HashSet<int>();
                var noteIdsToRemove = new HashSet<int>();
                var tagMapIdsToRemove = new HashSet<int>();

                foreach (var mark in backupFile.Database.UserMarks)
                {
                    if (colorIndexes.Contains(mark.ColorIndex))
                    {
                        userMarkIdsToRemove.Add(mark.UserMarkId);
                    }
                }

                if (userMarkIdsToRemove.Any())
                {
                    foreach (var note in backupFile.Database.Notes)
                    {
                        if (note.UserMarkId == null)
                        {
                            continue;
                        }

                        if (userMarkIdsToRemove.Contains(note.UserMarkId.Value))
                        {
                            if (removeAssociatedNotes)
                            {
                                noteIdsToRemove.Add(note.NoteId);
                            }
                            else
                            {
                                note.UserMarkId = null;
                            }
                        }
                    }

                    foreach (var tagMap in backupFile.Database.TagMaps)
                    {
                        if (tagMap.NoteId == null)
                        {
                            continue;
                        }

                        if (noteIdsToRemove.Contains(tagMap.NoteId.Value))
                        {
                            tagMapIdsToRemove.Add(tagMap.TagMapId);
                        }
                    }
                }

                backupFile.Database.UserMarks.RemoveAll(x => userMarkIdsToRemove.Contains(x.UserMarkId));
                backupFile.Database.Notes.RemoveAll(x => noteIdsToRemove.Contains(x.NoteId));
                backupFile.Database.TagMaps.RemoveAll(x => tagMapIdsToRemove.Contains(x.TagMapId));

                // Note that redundant block ranges are cleaned automatically by Cleaner.

                if (noteIdsToRemove.Count > 0 || userMarkIdsToRemove.Count > 0)
                {
                    backupFileService.WriteNewDatabaseWithClean(backupFile, filePath, filePath);
                }

                return userMarkIdsToRemove.Count;
            });

            return result;
        }
    }
}
