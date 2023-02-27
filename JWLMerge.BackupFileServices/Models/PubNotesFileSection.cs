namespace JWLMerge.BackupFileServices.Models;

public class PubNotesFileSection
{
    public PubNotesFileSection(PubSymbolAndLanguage symbolAndLanguage)
    {
        SymbolAndLanguage = symbolAndLanguage;
    }

    public PubSymbolAndLanguage SymbolAndLanguage { get; }

    public int ContentStartLine { get; set; }

    public int ContentEndLine { get; set; }
}