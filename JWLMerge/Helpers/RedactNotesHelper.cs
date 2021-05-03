namespace JWLMerge.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;
    using JWLMerge.Services;

    internal static class RedactNotesHelper
    {
        public static Task ExecuteAsync(
            List<Note> notes, IRedactService redactService, BackupFile backupFile, IBackupFileService backupFileService, string filePath)
        {
            return Task.Run(() =>
            {
                foreach (var note in notes)
                {
                    if (!string.IsNullOrEmpty(note.Title))
                    {
                        note.Title = redactService.GetNoteTitle(note.Title.Length);
                    }

                    if (!string.IsNullOrEmpty(note.Content))
                    {
                        note.Content = redactService.GenerateNoteContent(note.Content.Length);
                    }
                }

                backupFileService.WriteNewDatabase(backupFile, filePath, filePath);
            });
        }
    }
}
