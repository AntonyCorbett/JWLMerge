using JWLMerge.BackupFileServices.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JWLMerge.BackupFileServices.Models;
using JWLMerge.BackupFileServices.Models.DatabaseModels;

namespace JWLMerge.BackupFileServices.Helpers;

public class BibleNotesFile
{
    private const int MaxTitleLength = 50;
    private const string BibleKeySymbolToken = "[BibleKeySymbol";
    private const string MepsLanguageIdToken = "[MepsLanguageId";

    private Lazy<string[]> FileContents { get; }

    private readonly Lazy<PubNotesFileSection[]> _sections;
       
    public BibleNotesFile(string path)
    {
        FileContents = new Lazy<string[]>(() => FileContentsFactory(path));
        _sections = new Lazy<PubNotesFileSection[]>(SectionsFactory);
    }

    public BibleNotesFile(string[] fileContents)
    {
        FileContents = new Lazy<string[]>(() => FileContentsFactory(fileContents));
        _sections = new Lazy<PubNotesFileSection[]>(SectionsFactory);
    }

    public PubSymbolAndLanguage[] GetPubSymbolsAndLanguages()
        => _sections.Value.Select(x => x.SymbolAndLanguage).ToArray();
    
    public IEnumerable<BibleNote> GetNotes(PubSymbolAndLanguage symbolAndLanguage)
    {
        var sections = _sections.Value.Where(x => x.SymbolAndLanguage == symbolAndLanguage).ToArray();

        if (!sections.Any())
        {
            yield break;
        }

        foreach (var section in sections)
        {
            foreach (var note in GetNotes(section))
            {
                yield return note;
            }
        }
    }

    private PubNotesFileSection[] SectionsFactory()
    {
        var lineCount = FileContents.Value.Length;

        var result = new List<PubNotesFileSection>();

        PubNotesFileSection? currentSection = null;

        for (var n = 0; n < lineCount; ++n)
        {
            var pubSymbolLine = FileContents.Value[n].TrimStart();
            if (n == lineCount - 1 || !pubSymbolLine.StartsWith(BibleKeySymbolToken, StringComparison.OrdinalIgnoreCase))
            {
                if (currentSection != null)
                {
                    currentSection.ContentEndLine = n;
                }

                continue;
            }

            if (currentSection != null)
            {
                result.Add(currentSection);
            }
            
            var equalsPos = pubSymbolLine.IndexOf('=', BibleKeySymbolToken.Length);
            if (equalsPos < 0)
            {
                throw new BackupFileServicesException($"Could not find Publication Key Symbol in line {n+1}");
            }

            var pubSymbol = pubSymbolLine[(equalsPos + 1)..].TrimEnd(']').Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(pubSymbol))
            {
                throw new BackupFileServicesException($"Could not find Publication Key Symbol in line {n + 1}");
            }

            var languageIdLine = FileContents.Value[n + 1].TrimStart();
            if (!languageIdLine.StartsWith(MepsLanguageIdToken, StringComparison.OrdinalIgnoreCase))
            {
                throw new BackupFileServicesException($"Could not find MEPS Language Id in line {n + 2}");
            }

            equalsPos = languageIdLine.IndexOf('=', MepsLanguageIdToken.Length);
            var languageIdStr = languageIdLine[(equalsPos + 1)..].TrimEnd(']').Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(languageIdStr) || !int.TryParse(languageIdStr, out var languageId))
            {
                throw new BackupFileServicesException($"Could not find MEPS Language Id in line {n + 2}");
            }

            ++n;

            currentSection = new PubNotesFileSection(new PubSymbolAndLanguage(pubSymbol, languageId))
            {
                ContentStartLine = n + 1
            };
        }

        if (currentSection != null)
        {
            result.Add(currentSection);
        }

        return result.ToArray();
    }

    private static string[] FileContentsFactory(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new BackupFileServicesException("Bible notes file not specified");
        }

        if (!File.Exists(filePath))
        {
            throw new BackupFileServicesException("Bible notes file does not exist");
        }

        return File.ReadAllLines(filePath);
    }

    private static string[] FileContentsFactory(string[] fileContents)
    {
        return fileContents;
    }

    private IEnumerable<BibleNote> GetNotes(PubNotesFileSection section)
    {
        return ParseNotes(GetLines(section));
    }

    private IEnumerable<string> GetLines(PubNotesFileSection section)
    {
        for (var pos = section.ContentStartLine; pos <= section.ContentEndLine; ++pos)
        {
            yield return FileContents.Value[pos];
        }
    }

    private static IEnumerable<BibleNote> ParseNotes(IEnumerable<string> lines)
    {
        var linesInNote = new List<string>();

        BibleNotesVerseSpecification? currentVerseSpec = null;

        foreach (var line in lines)
        {
            var verseSpec = GetVerseSpecification(line);
            if (verseSpec == null)
            {
                if (linesInNote.Count > 0 || !string.IsNullOrWhiteSpace(line))
                {
                    linesInNote.Add(line);
                }
            }
            else
            {
                // start of a new verse note
                var note1 = CreateNote(linesInNote, currentVerseSpec);
                if (note1 != null)
                {
                    yield return note1;
                }

                currentVerseSpec = verseSpec;
                linesInNote.Clear();
            }
        }

        var note2 = CreateNote(linesInNote, currentVerseSpec);
        if (note2 != null)
        {
            yield return note2;
        }
    }

    private static BibleNote? CreateNote(IReadOnlyList<string> lines, BibleNotesVerseSpecification? currentVerseSpec)
    {
        if (currentVerseSpec == null)
        {
            return null;
        }

        var titleAndContent = ParseTitleAndContent(lines);
        if (titleAndContent == null)
        {
            return null;
        }
            
        return new BibleNote
        {
            BookChapterAndVerse = new BibleBookChapterAndVerse(
                currentVerseSpec.BookNumber, 
                currentVerseSpec.ChapterNumber, 
                currentVerseSpec.VerseNumber),

            NoteTitle = titleAndContent.Title?.Trim(),
            NoteContent = titleAndContent.Content?.Trim(),
            ColourIndex = currentVerseSpec.ColourIndex,
            StartTokenInVerse = currentVerseSpec.StartWordIndex,
            EndTokenInVerse = currentVerseSpec.EndWordIndex,
        };
    }

    private static NoteTitleAndContent? ParseTitleAndContent(IReadOnlyList<string> lines)
    {
        if (lines.Count == 0)
        {
            return null;
        }

        var result = new NoteTitleAndContent();

        // use first line as title if length is reasonable
        if (lines[0].Length <= MaxTitleLength)
        {
            result.Title = lines[0];
            result.Content = string.Join(Environment.NewLine, lines.Skip(1).SkipWhile(string.IsNullOrWhiteSpace));
        }
        else
        {
            result.Title = string.Empty;
            result.Content = string.Join(Environment.NewLine, lines);
        }

        return result;
    }

    private static BibleNotesVerseSpecification? GetVerseSpecification(string line)
    {
        var trimmed = line.Trim();

        if (!trimmed.StartsWith("[", StringComparison.Ordinal) || !trimmed.EndsWith("]", StringComparison.Ordinal))
        {
            return null;
        }

        var digits = trimmed.TrimStart('[').TrimEnd(']').Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
        if (digits.Length < 3 || digits.Length > 6)
        {
            return null;
        }

        if (!int.TryParse(digits[0], out var bibleBook))
        {
            return null;
        }

        if (!int.TryParse(digits[1], out var chapter))
        {
            return null;
        }

        if (!int.TryParse(digits[2], out var verse))
        {
            return null;
        }

        var result = new BibleNotesVerseSpecification
        {
            BookNumber = bibleBook,
            ChapterNumber = chapter,
            VerseNumber = verse,
        };

        if (digits.Length > 4 && 
            int.TryParse(digits[3], out var startWord) && 
            int.TryParse(digits[4], out var endWord) && 
            endWord >= startWord && 
            startWord >= 0 && (startWord != 0 || endWord != 0))
        {
            result.StartWordIndex = startWord;
            result.EndWordIndex = endWord;

            if (digits.Length > 5 && int.TryParse(digits[5], out var colourIndex) && colourIndex >= 0)
            {
                result.ColourIndex = colourIndex;
            }
        }

        return result;
    }
}