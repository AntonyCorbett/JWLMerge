using JWLMerge.ImportExportServices.Models;

namespace JWLMerge.ImportExportServices;

public interface IExportToFileService
{
    ExportBibleNotesResult Execute(
        string exportFilePath,
        IReadOnlyCollection<BibleNoteForImportExport>? notes,
        string backupFilePath);
}