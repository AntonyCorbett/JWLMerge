using JWLMerge.ImportExportServices.Models;

namespace JWLMerge.Services;

internal interface IFileOpenSaveService
{
    string? GetSaveFilePath(string title);

    string? GetBibleNotesImportFilePath(string title);

    string? GetBibleNotesExportFilePath(string title);

    ImportExportFileType GetFileType(string? fileName);
}