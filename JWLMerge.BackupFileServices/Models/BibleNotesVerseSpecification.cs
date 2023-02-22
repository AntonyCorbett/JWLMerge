namespace JWLMerge.BackupFileServices.Models;

internal sealed class BibleNotesVerseSpecification
{
    public int BookNumber { get; set; }

    public int ChapterNumber { get; set; }

    public int VerseNumber { get; set; }

    public int? StartWordIndex { get; set; }

    public int? EndWordIndex { get; set; }

    public int ColourIndex { get; set; }
}