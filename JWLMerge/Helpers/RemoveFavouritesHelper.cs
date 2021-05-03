namespace JWLMerge.Helpers
{
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Models;

    internal static class RemoveFavouritesHelper
    {
        public static Task ExecuteAsync(BackupFile backupFile, IBackupFileService backupFileService, string filePath)
        {
            return Task.Run(() =>
            {
                backupFile?.Database.TagMaps.RemoveAll(x => x.TagId == 1);
                backupFileService.WriteNewDatabase(backupFile, filePath, filePath);
            });
        }
    }
}
