namespace JWLMerge.BackupFileServices.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;

    public class BibleNotesFile
    {
        private const int MaxTitleLength = 50;
        private const string BibleKeySymbolToken = @"[BibleKeySymbol";
        private const string MepsLanguageIdToken = @"[MepsLanguageId";

        private readonly List<BibleNote> _notes = new List<BibleNote>();
        private readonly string _path;
        private string _bibleKeySymbol;
        private int _mepsLanguageId;
        private bool _initialised;

        public BibleNotesFile(string path)
        {
            _path = path;
        }

        public IEnumerable<BibleNote> GetNotes()
        {
            Init();
            return _notes;
        }

        public string GetBibleKeySymbol()
        {
            Init();
            return _bibleKeySymbol;
        }

        public int GetMepsLanguageId()
        {
            Init();
            return _mepsLanguageId;
        }

        private void Init()
        {
            if (!_initialised)
            {
                ParseFile();
            }
        }

        private void ParseFile()
        {
            _initialised = true;

            var lines = ReadLinesFromFile();

            ParseParameters(lines);
            RemoveParamLines(lines);
            ParseNotes(lines);
        }

        private void RemoveParamLines(string[] lines)    
        {
            RemoveLineStarting(BibleKeySymbolToken, lines);
            RemoveLineStarting(MepsLanguageIdToken, lines);
        }

        private void RemoveLineStarting(string token, string[] lines)
        {
            for (int n = 0; n < lines.Length; ++n)
            {
                if (lines[n].Trim().StartsWith(token, StringComparison.OrdinalIgnoreCase))
                {
                    lines[n] = string.Empty;
                    break;
                }
            }
        }

        private string[] ReadLinesFromFile()
        {
            if (string.IsNullOrEmpty(_path))
            {
                throw new Exception("Bible notes file not specified");
            }

            if (!File.Exists(_path))
            {
                throw new Exception("Bible notes file does not exist");
            }

            return File.ReadAllLines(_path);
        }

        private void ParseNotes(string[] lines)
        {
            var linesInNote = new List<string>();

            BibleNotesVerseSpecification currentVerseSpec = null;

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
                    StoreNote(linesInNote, currentVerseSpec);
                    currentVerseSpec = verseSpec;
                    linesInNote.Clear();
                }
            }

            StoreNote(linesInNote, currentVerseSpec);
        }

        private void StoreNote(IReadOnlyList<string> lines, BibleNotesVerseSpecification currentVerseSpec)
        {
            if (currentVerseSpec == null)
            {
                return;
            }

            var titleAndContent = ParseTitleAndContent(lines);
            if (titleAndContent == null)
            {
                return;
            }
            
            _notes.Add(new BibleNote
            {
                BookChapterAndVerse = new BibleBookChapterAndVerse(
                    currentVerseSpec.BookNumber, 
                    currentVerseSpec.ChapterNumber, 
                    currentVerseSpec.VerseNumber),

                NoteTitle = titleAndContent.Title,
                NoteContent = titleAndContent.Content,
                ColourIndex = currentVerseSpec.ColourIndex,
                StartTokenInVerse = currentVerseSpec.StartWordIndex,
                EndTokenInVerse = currentVerseSpec.EndWordIndex,
            });
        }

        private NoteTitleAndContent ParseTitleAndContent(IReadOnlyList<string> lines)
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

        private BibleNotesVerseSpecification GetVerseSpecification(string line)
        {
            var trimmed = line.Trim();

            if (!trimmed.StartsWith("[") || !trimmed.EndsWith("]"))
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
                startWord >= 0)
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

        private void ParseParameters(string[] lines)
        {
            var bibleKeySymbol = FindValue(lines, BibleKeySymbolToken);
            if (string.IsNullOrEmpty(bibleKeySymbol))
            {
                throw new Exception("Could not find Bible Key Symbol");
            }

            _bibleKeySymbol = bibleKeySymbol.Trim('"');
            
            var mepsLanguageId = FindValue(lines, MepsLanguageIdToken);
            if (string.IsNullOrEmpty(mepsLanguageId))
            {
                throw new Exception("Could not find Meps Language Id");
            }

            if (!int.TryParse(mepsLanguageId.Trim('"'), out _mepsLanguageId))
            {
                throw new Exception("Could not parse Meps Language Id");
            }
        }

        private string FindValue(string[] lines, string token)
        {
            var line = lines.FirstOrDefault(x =>
                x.Trim().StartsWith(token, StringComparison.OrdinalIgnoreCase));

            if (line == null)
            {
                return null;
            }

            var equalsPos = line.IndexOf('=', token.Length);
            if (equalsPos < 0)
            {
                return null;
            }

            return line.Substring(equalsPos + 1).TrimEnd(']').Trim();
        }
    }
}
