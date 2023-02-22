namespace JWLMerge.ImportExportServices.Models;

public class ExportBibleNotesResult
{
    public int RowCount { get; set; }

    public bool SomeNotesTooLarge { get; set; }

    public bool NoNotesFound { get; set; }
}