namespace JWLMerge.Helpers
{
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.ExcelServices;

    internal static class ExportBibleNotesHelper
    {
        public static Task ExecuteAsync(
            BackupFile backupFile,
            IBackupFileService backupFileService,
            string bibleNotesExportFilePath, 
            IExcelService excelService)
        {
            return Task.Run(() =>
            {
                backupFileService.ExportBibleNotesToExcel(
                    backupFile,
                    bibleNotesExportFilePath,
                    excelService);
            });
        }
    }
}
