namespace JWLMerge.Helpers
{
    using System;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Models.ManifestFile;
    using JWLMerge.Models;

    internal static class DesignTimeFileCreation
    {
        public static JwLibraryFile CreateMockJwLibraryFile(
            IBackupFileService backupFileService, 
            int fileIndex)
        {
            var file = backupFileService.CreateBlank();
            
            file.Manifest.Name = $"File {fileIndex + 1}";
            file.Manifest.CreationDate = GenerateDateString(DateTime.Now.AddDays(-fileIndex));
            file.Manifest.UserDataBackup = new UserDataBackup
            {
                DeviceName = "MYPC",
            };

            return new JwLibraryFile
            {
                FilePath = "c:\\temp\\myfile.jwlibrary",
                BackupFile = file,
            };
        }

        private static string GenerateDateString(DateTime dateTime)
        {
            return $"{dateTime.Year}-{dateTime.Month:D2}-{dateTime.Day:D2}";
        } 
    }
}
