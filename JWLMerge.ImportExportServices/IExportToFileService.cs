using JWLMerge.ImportExportServices.Models;

namespace JWLMerge.ImportExportServices;

public interface IExportToFileService
{
    ExportBibleNotesResult AppendToBibleNotesFile(
        string excelFilePath,
        IReadOnlyCollection<BibleNoteForImportExport>? notes,
        string backupFilePath);
}