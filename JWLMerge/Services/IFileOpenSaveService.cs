namespace JWLMerge.Services
{
    internal interface IFileOpenSaveService
    {
        string GetSaveFilePath(string title);

        string GetBibleNotesImportFilePath(string title);
    }
}
