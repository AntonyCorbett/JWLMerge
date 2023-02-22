using JWLMerge.ImportExportServices.Models;

namespace JWLMerge.ImportExportServices;

public class TextFileService : IExportToFileService
{
    public ExportBibleNotesResult AppendToBibleNotesFile(
        string excelFilePath, 
        IReadOnlyCollection<BibleNoteForImportExport>? notes, 
        string backupFilePath)
    {
        throw new NotImplementedException();
    }
}