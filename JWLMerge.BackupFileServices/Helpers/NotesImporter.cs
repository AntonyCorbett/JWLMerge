namespace JWLMerge.BackupFileServices.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;

    internal sealed class NotesImporter
    {
        private readonly Database _targetDatabase;
        private readonly string _bibleKeySymbol;
        private readonly int _mepsLanguageId;
        private readonly ImportBibleNotesParams _options;
        private int _maxNoteId;
        private int _maxTagMapId;
        private int _maxLocationId;
        private int _maxUserMarkId;
        private int _maxBlockRangeId;

        public NotesImporter(
            Database targetDatabase, 
            string bibleKeySymbol, 
            int mepsLanguageId,
            ImportBibleNotesParams options)
        {
            _targetDatabase = targetDatabase;
            _bibleKeySymbol = bibleKeySymbol;
            _mepsLanguageId = mepsLanguageId;
            _options = options;

            _maxNoteId = !_targetDatabase.Notes.Any()
                ? 0 
                : _targetDatabase.Notes.Max(x => x.NoteId);

            _maxTagMapId = !_targetDatabase.TagMaps.Any()
                ? 0
                : _targetDatabase.TagMaps.Max(x => x.TagMapId);

            _maxLocationId = !_targetDatabase.Locations.Any()
                ? 0
                : _targetDatabase.Locations.Max(x => x.LocationId);

            _maxUserMarkId = !_targetDatabase.UserMarks.Any()
                ? 0
                : _targetDatabase.UserMarks.Max(x => x.UserMarkId);

            _maxBlockRangeId = !_targetDatabase.BlockRanges.Any()
                ? 0
                : _targetDatabase.BlockRanges.Max(x => x.BlockRangeId);
        }

        public NotesImportResults Import(IEnumerable<BibleNote> notes)
        {
            var result = new NotesImportResults();
            
            foreach (var note in notes)
            {
                var existingNote = FindExistingNote(_targetDatabase, note);
                if (existingNote == null)
                {
                    result.BibleNotesAdded++;
                    InsertNote(note);
                }
                else
                {
                    if (!existingNote.Content.Equals(note.NoteContent))
                    {
                        // need to update the note.
                        result.BibleNotesUpdated++;
                        existingNote.Content = note.NoteContent;
                    }
                    else
                    {
                        result.BibleNotesUnchanged++;
                    }
                }
            }
            
            _targetDatabase.CheckValidity();

            return result;
        }

        private void InsertNote(BibleNote note)
        {
            var book = note.BookChapterAndVerse.BookNumber;
            var chapter = note.BookChapterAndVerse.ChapterNumber;

            var location = _targetDatabase.FindLocationByBibleChapter(_bibleKeySymbol, book, chapter) ?? 
                           InsertLocation(book, chapter);

            UserMark userMark = null;
            if (note.StartTokenInVerse != null && note.EndTokenInVerse != null)
            {
                // the note should be associated with some
                // highlighted text in the verse.
                userMark = FindExistingUserMark(
                                   location.LocationId, 
                                   note.StartTokenInVerse.Value,
                                   note.EndTokenInVerse.Value) ??
                               InsertUserMark(
                                   location.LocationId, 
                                   note.ColourIndex, 
                                   note.StartTokenInVerse.Value, 
                                   note.EndTokenInVerse.Value,
                                   note.BookChapterAndVerse.VerseNumber);
            }

            var newNote = new Note
            {
                NoteId = ++_maxNoteId,
                Guid = Guid.NewGuid().ToString().ToLower(),
                UserMarkId = userMark?.UserMarkId,
                LocationId = location.LocationId,
                Title = note.NoteTitle,
                Content = note.NoteContent,
                BlockType = 2,
                BlockIdentifier = note.BookChapterAndVerse.VerseNumber,
                LastModified = Database.GetDateTimeUtcAsDbString(DateTime.UtcNow),
            };

            var newTagMapEntry = _options.TagId == 0
                ? null
                : CreateTagMapEntryForImportedBibleNote(newNote.NoteId, _options.TagId);

            _targetDatabase.AddBibleNoteAndUpdateIndex(
                note.BookChapterAndVerse, 
                newNote, 
                newTagMapEntry);
        }

        private TagMap CreateTagMapEntryForImportedBibleNote(int noteId, int tagId)
        {
            return new TagMap
            {
                TagMapId = ++_maxTagMapId,
                TagId = tagId,
                NoteId = noteId,
            };
        }

        private UserMark InsertUserMark(
            int locationId, 
            int colourIndex, 
            int startToken, 
            int endToken, 
            int verseNumber)
        {
            var userMark = new UserMark
            {
                UserMarkId = ++_maxUserMarkId,
                LocationId = locationId,
                UserMarkGuid = Guid.NewGuid().ToString().ToLower(),
                Version = 1,
                ColorIndex = colourIndex,
            };

            _targetDatabase.AddUserMarkAndUpdateIndex(userMark);

            // now add the block range...
            var blockRange = new BlockRange
            {
                BlockRangeId = ++_maxBlockRangeId,
                BlockType = 2,
                Identifier = verseNumber,
                StartToken = startToken,
                EndToken = endToken,
                UserMarkId = userMark.UserMarkId,
            };

            _targetDatabase.AddBlockRangeAndUpdateIndex(blockRange);

            return userMark;
        }

        private UserMark FindExistingUserMark(int locationId, int startToken, int endToken)
        {
            var userMarksForLocation = _targetDatabase.FindUserMarks(locationId);
            if (userMarksForLocation == null)
            {
                return null;
            }

            foreach (var userMark in userMarksForLocation)
            {
                var ranges = _targetDatabase.FindBlockRanges(userMark.UserMarkId);
                if (ranges != null)
                {
                    foreach (var range in ranges)
                    {
                        if (range != null && range.StartToken == startToken && range.EndToken == endToken)
                        {
                            return userMark;
                        }
                    }
                }
            }

            return null;
        }

        private Location InsertLocation(int book, int chapter)
        {
            var location = new Location
            {
                LocationId = ++_maxLocationId,
                BookNumber = book,
                ChapterNumber = chapter,
                KeySymbol = _bibleKeySymbol,
                MepsLanguage = _mepsLanguageId,
                Title = $"{BibleBookNames.GetName(book)} {chapter}",
            };

            _targetDatabase.AddLocationAndUpdateIndex(location);

            return location;
        }

        private Note FindExistingNote(Database database, BibleNote note)
        {
            var existingVerseNotes = database.FindNotes(note.BookChapterAndVerse);
            return existingVerseNotes?.FirstOrDefault(verseNote => verseNote.Title == note.NoteTitle);
        }
    }
}
