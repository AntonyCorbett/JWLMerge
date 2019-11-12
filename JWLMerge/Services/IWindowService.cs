namespace JWLMerge.Services
{
    using JWLMerge.BackupFileServices;

    internal interface IWindowService
    {
        void ShowDetailWindow(IBackupFileService backupFileService, string filePath, bool notesRedacted);

        void Close(string filePath);
        
        void CloseAll();
    }
}
