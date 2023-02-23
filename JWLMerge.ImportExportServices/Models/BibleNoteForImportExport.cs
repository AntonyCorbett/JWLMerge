namespace JWLMerge.ImportExportServices.Models;

public class BibleNoteForImportExport
{
    public BibleNoteForImportExport(int bookNumber, string bookName)
    {
        BookNumber = bookNumber;
        BookName = bookName;
    }

    public int BookNumber { get; }

    public string BookName { get; }

    public int? ChapterNumber { get; set; }

    public int? VerseNumber { get; set; }

    public int? StartTokenInVerse { get; set; }

    public int? EndTokenInVerse { get; set; }

    public string? NoteTitle { get; set; }

    public string? NoteContent { get; set; }

    public string? TagsCsv { get; set; }

    public int? ColorCode { get; set; }

    public string? PubSymbol { get; set; }

    public int? MepsLanguageId { get; set; }

    public string ChapterAndVerseString
    {
        get
        {
            if (ChapterNumber == null)
            {
                return string.Empty;
            }

            if (VerseNumber == null)
            {
                return ChapterNumber.ToString()!;
            }

            return $"{ChapterNumber}:{VerseNumber}";
        }
    }

    public string BookNameChapterAndVerseString => $"{BookName} {ChapterAndVerseString}";
}