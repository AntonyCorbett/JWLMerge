using JWLMerge.ImportExportServices.Models;
using JWLMerge.BackupFileServices.Models;
using JWLMerge.BackupFileServices.Exceptions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JWLMerge.BackupFileServices.Models.DatabaseModels;
using JWLMerge.ImportExportServices;

namespace JWLMerge.BackupFileServices.Helpers;

internal sealed class NotesExporter
{
    public ExportBibleNotesResult ExportBibleNotes(
        BackupFile backupFile, string bibleNotesExportFilePath, IExportToFileService exportService)
    {
        File.Delete(bibleNotesExportFilePath);
        if (File.Exists(bibleNotesExportFilePath))
        {
            throw new BackupFileServicesException(
                $"Could not delete existing file: {bibleNotesExportFilePath}");
        }

        var notesToWrite = new List<BibleNoteForImportExport>();

        var tags = backupFile.Database.TagMaps.Where(x => x.NoteId != null).ToLookup(map => map.NoteId, map => map);

        foreach (var note in backupFile.Database.Notes)
        {
            if (note.LocationId == null)
            {
                continue;
            }

            var location = backupFile.Database.FindLocation(note.LocationId.Value);

            if (location?.BookNumber == null)
            {
                // not from a Bible
                continue;
            }

            int? colorCode = null;
            int? startToken = null;
            int? endToken = null;

            if (note.UserMarkId != null)
            {
                var userMark = backupFile.Database.FindUserMark(note.UserMarkId.Value);
                if (userMark != null)
                {
                    colorCode = userMark.ColorIndex;

                    var blockRanges = backupFile.Database.FindBlockRanges(userMark.UserMarkId);
                    if (blockRanges != null)
                    {
                        startToken = blockRanges.First().StartToken;
                        endToken = blockRanges.First().EndToken;
                    }
                }
            }

            notesToWrite.Add(new BibleNoteForImportExport(location.BookNumber.Value, BibleBookNames.GetName(location.BookNumber.Value))
            {
                ChapterNumber = location.ChapterNumber,
                VerseNumber = note.BlockIdentifier,
                StartTokenInVerse = startToken,
                EndTokenInVerse = endToken,
                NoteTitle = note.Title?.Trim(),
                NoteContent = note.Content?.Trim(),
                PubSymbol = location.KeySymbol,
                MepsLanguageId = location.MepsLanguage,
                ColorCode = colorCode,
                TagsCsv = GetTagsAsCsv(tags, note.NoteId, backupFile.Database),
            });
        }

        notesToWrite.Sort(SortBibleNotes);

        return exportService.Execute(bibleNotesExportFilePath, notesToWrite, bibleNotesExportFilePath);
    }

    private static string GetTagsAsCsv(ILookup<int?, TagMap> tags, int noteId, Database database)
    {
        var t = tags[noteId].ToArray();
        if (!t.Any())
        {
            return string.Empty;
        }

        var tagNames = t.Select(x => database.FindTag(x.TagId)?.Name).Where(x => !string.IsNullOrEmpty(x));
        return string.Join(", ", tagNames);
    }

    private int SortBibleNotes(BibleNoteForImportExport x, BibleNoteForImportExport y)
    {
        if (x.BookNumber != y.BookNumber)
        {
            return x.BookNumber.CompareTo(y.BookNumber);
        }

        if (x.ChapterNumber != y.ChapterNumber)
        {
            return x.ChapterNumber ?? 0.CompareTo(y.ChapterNumber ?? 0);
        }

        if (x.VerseNumber != y.VerseNumber)
        {
            return x.VerseNumber ?? 0.CompareTo(y.VerseNumber ?? 0);
        }

        return 0;
    }
}