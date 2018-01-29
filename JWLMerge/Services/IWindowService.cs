namespace JWLMerge.Services
{
    using BackupFileServices;

    internal interface IWindowService
    {
        void ShowDetailWindow(IBackupFileService backupFileService, string filePath);

        void Close(string filePath);
        
        void CloseAll();
    }
}
