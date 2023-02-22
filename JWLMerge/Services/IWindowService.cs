using JWLMerge.BackupFileServices;

namespace JWLMerge.Services;

internal interface IWindowService
{
    void ShowDetailWindow(IBackupFileService backupFileService, string filePath, bool notesRedacted);

    void Close(string filePath);
        
    void CloseAll();
}