namespace JWLMerge.Helpers
{
    using System;
    using BackupFileServices;
    using BackupFileServices.Models;

    internal static class DesignTimeFileCreation
    {
        public static BackupFile CreateMockBackupFile(
            IBackupFileService backupFileService, 
            int fileIndex)
        {
            var file = backupFileService.CreateBlank();
            file.Manifest.Name = $"File {fileIndex + 1}";
            file.Manifest.CreationDate = GenerateDateString(DateTime.Now.AddDays(-fileIndex));
            return file;
        }

        private static string GenerateDateString(DateTime dateTime)
        {
            return $"{dateTime.Year}-{dateTime.Month:D2}-{dateTime.Day:D2}";
        } 
    }
}
