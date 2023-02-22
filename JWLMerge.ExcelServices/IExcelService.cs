using System.Collections.Generic;
using JWLMerge.ImportExportServices.Models;

namespace JWLMerge.ExcelServices;

public interface IExcelService
{
    ExportBibleNotesResult AppendToBibleNotesFile(
        string excelFilePath, 
        IReadOnlyCollection<BibleNoteForImportExport>? notes, 
        int startRow, 
        string backupFilePath);
}