using System.Text;
using JWLMerge.ImportExportServices.Models;

namespace JWLMerge.ImportExportServices;

public class TextFileService : IExportToFileService
{
    /// <summary>
    /// Exports Bible notes to a text file
    /// </summary>
    /// <param name="exportFilePath">Text file path.</param>
    /// <param name="notes">Notes.</param>
    /// <param name="backupFilePath">Path to backup file.</param>
    /// <returns>Results.</returns>
    public ExportBibleNotesResult Execute(
        string exportFilePath, 
        IReadOnlyCollection<BibleNoteForImportExport>? notes, 
        string backupFilePath)
    {
        var result = new ExportBibleNotesResult();

        if (string.IsNullOrEmpty(exportFilePath))
        {
            throw new ArgumentNullException(nameof(exportFilePath));
        }

        if (notes == null || !notes.Any())
        {
            result.NoNotesFound = true;
            return result;
        }

        using var writer = new StreamWriter(exportFilePath);
        var section = 0;

        var notesGroupedByPubSymbolAndLanguage = notes.GroupBy(x => (x.PubSymbol, x.MepsLanguageId));
        foreach (var grouping in notesGroupedByPubSymbolAndLanguage)
        {
            var pubSymbol = grouping.Key.PubSymbol;
            var languageId = grouping.Key.MepsLanguageId ?? 0;

            if (grouping.All(x => string.IsNullOrWhiteSpace(x.NoteContent)))
            {
                continue;
            }

            if (section > 0)
            {
                writer.WriteLine();
            }
                
            writer.WriteLine($"[BibleKeySymbol={pubSymbol}]");
            writer.WriteLine($"[MepsLanguageId={languageId}]");

            foreach (var item in grouping)
            {
                if (string.IsNullOrWhiteSpace(item.NoteContent))
                {
                    continue;
                }

                writer.WriteLine();

                // [BOOK:CHAP:VS:WORD1:WORD2:COL]

                // BOOK = the 1 - based Bible book index, e.g. 1 = Genesis, 66 = Revelation.
                // CHAP = the chapter.
                // VS = the verse.
                // WORD1 = the 0 - based index of the first token in the verse to which the note applies.
                // WORD2 = the 0 - based index of the last token in the verse to which the note applies.
                // COL = the colour index to use.

                writer.WriteLine(item is {StartTokenInVerse: 0, EndTokenInVerse: 0, ColorCode: 0}
                    ? $"[{item.BookNumber}:{item.ChapterNumber ?? 0}:{item.VerseNumber ?? 0}]"
                    : $"[{item.BookNumber}:{item.ChapterNumber ?? 0}:{item.VerseNumber ?? 0}:{item.StartTokenInVerse ?? 0}:{item.EndTokenInVerse ?? 0}:{item.ColorCode ?? 0}]");

                if (string.IsNullOrWhiteSpace(item.NoteTitle))
                {
                    writer.WriteLine(item.NoteTitle);
                }

                writer.WriteLine(item.NoteContent);
            }

            ++section;
        }

        return result;
    }
}