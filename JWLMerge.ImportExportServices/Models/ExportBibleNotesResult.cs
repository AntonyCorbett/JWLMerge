namespace JWLMerge.ImportExportServices.Models
{
    public class ExportBibleNotesResult
    {
        public int Row { get; set; }

        public bool SomeNotesTooLarge { get; set; }

        public bool NoNotesFound { get; set; }
    }
}
