namespace JWLMerge.Helpers
{
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Helpers;
    using JWLMerge.BackupFileServices.Models;

    internal static class ImportBibleNotesHelper
    {
        public static async Task ExecuteAsync(
            BackupFile backupFile, 
            IBackupFileService backupFileService,
            string backupFilePath,
            string importFilePath,
            ImportBibleNotesParams options)
        {
            await Task.Run(() =>
            {
                var file = new BibleNotesFile(importFilePath);

                backupFileService.ImportBibleNotes(
                    backupFile,
                    file.GetNotes(),
                    file.GetBibleKeySymbol(),
                    file.GetMepsLanguageId(),
                    options);

                backupFileService.WriteNewDatabase(backupFile, backupFilePath, backupFilePath);
            });
        }
    }
}
