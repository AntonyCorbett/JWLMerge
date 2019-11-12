namespace JWLMerge.BackupFileServices.Models.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;

    public class Database
    {
        private readonly Dictionary<int, int> _bookmarkSlots = new Dictionary<int, int>();
        
        private Lazy<Dictionary<string, Note>> _notesGuidIndex;
        private Lazy<Dictionary<int, Note>> _notesIdIndex;
        private Lazy<Dictionary<BibleBookChapterAndVerse, List<Note>>> _notesVerseIndex;
        private Lazy<Dictionary<string, UserMark>> _userMarksGuidIndex;
        private Lazy<Dictionary<int, UserMark>> _userMarksIdIndex;
        private Lazy<Dictionary<int, List<UserMark>>> _userMarksLocationIdIndex;
        private Lazy<Dictionary<int, Location>> _locationsIdIndex;
        private Lazy<Dictionary<string, Location>> _locationsValueIndex;
        private Lazy<Dictionary<string, Location>> _locationsBibleChapterIndex;
        private Lazy<Dictionary<string, Tag>> _tagsNameIndex;
        private Lazy<Dictionary<int, Tag>> _tagsIdIndex;
        private Lazy<Dictionary<string, TagMap>> _tagMapIndex;
        private Lazy<Dictionary<int, List<BlockRange>>> _blockRangesUserMarkIdIndex;
        private Lazy<Dictionary<string, Bookmark>> _bookmarksIndex;

        public Database()
        {
            ReinitializeIndexes();
        }

        public void InitBlank()
        {
            LastModified = new LastModified();
            Locations = new List<Location>();
            Notes = new List<Note>();
            Tags = new List<Tag>();
            TagMaps = new List<TagMap>();
            BlockRanges = new List<BlockRange>();
            Bookmarks = new List<Bookmark>();
            UserMarks = new List<UserMark>();
        }

        public void CheckValidity()
        {
            ReinitializeIndexes();

            CheckBlockRangeValidity();
            CheckBookmarkValidity();
            CheckNoteValidity();
            CheckTagMapValidity();
            CheckUserMarkValidity();
        }

        public LastModified LastModified { get; set; }
        
        public List<Location> Locations { get; set; }
        
        public List<Note> Notes { get; set; }
        
        public List<Tag> Tags { get; set; }

        public List<TagMap> TagMaps { get; set; }
        
        public List<BlockRange> BlockRanges { get; set; }

        public List<Bookmark> Bookmarks { get; set; }

        public List<UserMark> UserMarks { get; set; }

        public void AddBibleNoteAndUpdateIndex(BibleBookChapterAndVerse verseRef, Note value)
        {
            Notes.Add(value);

            if (_notesGuidIndex.IsValueCreated)
            {
                _notesGuidIndex.Value.Add(value.Guid, value);
            }

            if (_notesIdIndex.IsValueCreated)
            {
                _notesIdIndex.Value.Add(value.NoteId, value);
            }

            if (_notesVerseIndex.IsValueCreated)
            {
                if (!_notesVerseIndex.Value.TryGetValue(verseRef, out var notes))
                {
                    notes = new List<Note>();
                    _notesVerseIndex.Value.Add(verseRef, notes);
                }

                notes.Add(value);
            }
        }

        public static string GetDateTimeUtcAsDbString(DateTime dateTime)
        {
            return $"{dateTime:s}Z";
        }

        public void AddBlockRangeAndUpdateIndex(BlockRange value)
        {
            BlockRanges.Add(value);

            if (_blockRangesUserMarkIdIndex.IsValueCreated)
            {
                if (!_blockRangesUserMarkIdIndex.Value.TryGetValue(value.UserMarkId, out var blockRangeList))
                {
                    blockRangeList = new List<BlockRange>();
                    _blockRangesUserMarkIdIndex.Value.Add(value.UserMarkId, blockRangeList);

                }

                blockRangeList.Add(value);
            }
        }

        public void AddUserMarkAndUpdateIndex(UserMark value)
        {
            UserMarks.Add(value);

            if (_userMarksGuidIndex.IsValueCreated)
            {
                _userMarksGuidIndex.Value.Add(value.UserMarkGuid, value);
            }

            if (_userMarksIdIndex.IsValueCreated)
            {
                _userMarksIdIndex.Value.Add(value.UserMarkId, value);
            }

            if (_userMarksLocationIdIndex.IsValueCreated)
            {
                if (!_userMarksLocationIdIndex.Value.TryGetValue(value.LocationId, out var marks))
                {
                    marks = new List<UserMark>();
                    _userMarksLocationIdIndex.Value.Add(value.LocationId, marks);
                }

                marks.Add(value);
            }
        }

        public void AddLocationAndUpdateIndex(Location value)
        {
            Locations.Add(value);

            if (_locationsIdIndex.IsValueCreated)
            {
                _locationsIdIndex.Value.Add(value.LocationId, value);
            }

            if (_locationsValueIndex.IsValueCreated)
            {
                var key = GetLocationByValueKey(value);
                if (!_locationsValueIndex.Value.ContainsKey(key))
                {
                    _locationsValueIndex.Value.Add(key, value);
                }
            }

            if (_locationsBibleChapterIndex.IsValueCreated)
            {
                if (value.BookNumber != null && value.ChapterNumber != null)
                {
                    var key = GetLocationByBibleChapterKey(
                        value.BookNumber.Value,
                        value.ChapterNumber.Value,
                        value.KeySymbol);

                    if (!_locationsBibleChapterIndex.Value.ContainsKey(key))
                    {
                        _locationsBibleChapterIndex.Value.Add(key, value);
                    }
                }
            }
        }

        public Note FindNote(string guid)
        {
            return _notesGuidIndex.Value.TryGetValue(guid, out var note) ? note : null;
        }

        public Note FindNote(int noteId)
        {
            return _notesIdIndex.Value.TryGetValue(noteId, out var note) ? note : null;
        }

        public IEnumerable<Note> FindNotes(BibleBookChapterAndVerse verseRef)
        {
            return _notesVerseIndex.Value.TryGetValue(verseRef, out var notes) ? notes : null;
        }

        public UserMark FindUserMark(string guid)
        {
            return _userMarksGuidIndex.Value.TryGetValue(guid, out var userMark) ? userMark : null;
        }

        public UserMark FindUserMark(int userMarkId)
        {
            return _userMarksIdIndex.Value.TryGetValue(userMarkId, out var userMark) ? userMark : null;
        }

        public IEnumerable<UserMark> FindUserMarks(int locationId)
        {
            return _userMarksLocationIdIndex.Value.TryGetValue(locationId, out var userMarks) ? userMarks : null;
        }

        public Tag FindTag(string tagName)
        {
            return _tagsNameIndex.Value.TryGetValue(tagName, out var tag) ? tag : null;
        }

        public Tag FindTag(int tagId)
        {
            return _tagsIdIndex.Value.TryGetValue(tagId, out var tag) ? tag : null;
        }

        public TagMap FindTagMap(int tagId, int noteId)
        {
            return _tagMapIndex.Value.TryGetValue(GetTagMapKey(tagId, noteId), out var tag) ? tag : null;
        }

        public Location FindLocation(int locationId)
        {
            return _locationsIdIndex.Value.TryGetValue(locationId, out var location) ? location : null;
        }

        public Location FindLocationByValues(Location locationValues)
        {
            var key = GetLocationByValueKey(locationValues);
            return _locationsValueIndex.Value.TryGetValue(key, out var location) ? location : null;
        }

        public Location FindLocationByBibleChapter(string bibleKeySymbol, int bibleBookNumber, int bibleChapter)
        {
            var key = GetLocationByBibleChapterKey(bibleBookNumber, bibleChapter, bibleKeySymbol);
            return _locationsBibleChapterIndex.Value.TryGetValue(key, out var location) ? location : null;
        }

        public IReadOnlyCollection<BlockRange> FindBlockRanges(int userMarkId)
        {
            return _blockRangesUserMarkIdIndex.Value.TryGetValue(userMarkId, out var ranges) ? ranges : null;
        }

        public Bookmark FindBookmark(int locationId, int publicationLocationId)
        {
            string key = GetBookmarkKey(locationId, publicationLocationId);
            return _bookmarksIndex.Value.TryGetValue(key, out var bookmark) ? bookmark : null;
        }

        public int GetNextBookmarkSlot(int publicationLocationId)
        {
            // there are only 10 slots available for each publication...
            if (_bookmarkSlots.TryGetValue(publicationLocationId, out var slot))
            {
                ++slot;
            }
            
            _bookmarkSlots[publicationLocationId] = slot;
            return slot;
        }

        private Dictionary<string, Note> NoteIndexValueFactory()
        {
            return Notes.ToDictionary(note => note.Guid);
        }

        private Dictionary<int, Note> NoteIdIndexValueFactory()
        {
            return Notes.ToDictionary(note => note.NoteId);
        }

        private Dictionary<BibleBookChapterAndVerse, List<Note>> NoteVerseIndexValueFactory()
        {
            Dictionary<BibleBookChapterAndVerse, List<Note>> result = 
                new Dictionary<BibleBookChapterAndVerse, List<Note>>();

            foreach (var note in Notes)
            {
                if (note.BlockType == 2 && // A note on a Bible verse
                    note.LocationId != null && 
                    note.BlockIdentifier != null) 
                {
                    var location = FindLocation(note.LocationId.Value);
                    if (location?.BookNumber != null && location.ChapterNumber != null)
                    {
                        var verseRef = new BibleBookChapterAndVerse
                        {
                            BookNumber = location.BookNumber.Value,
                            ChapterNumber = location.ChapterNumber.Value,
                            VerseNumber = note.BlockIdentifier.Value
                        };

                        if (!result.TryGetValue(verseRef, out var notesOnVerse))
                        {
                            notesOnVerse = new List<Note>();
                            result.Add(verseRef, notesOnVerse);
                        }

                        notesOnVerse.Add(note);
                    }
                }
            }

            return result;
        }

        private Dictionary<string, UserMark> UserMarkIndexValueFactory()
        {
            return UserMarks.ToDictionary(userMark => userMark.UserMarkGuid);
        }

        private Dictionary<int, UserMark> UserMarkIdIndexValueFactory()
        {
            return UserMarks.ToDictionary(userMark => userMark.UserMarkId);
        }

        private Dictionary<int, List<UserMark>> UserMarksLocationIdIndexValueFactory()
        {
            var result = new Dictionary<int, List<UserMark>>();

            foreach (var userMark in UserMarks)
            {
                if (!result.TryGetValue(userMark.LocationId, out var marks))
                {
                    marks = new List<UserMark>();
                    result.Add(userMark.LocationId, marks);
                }

                marks.Add(userMark);
            }

            return result;
        }

        private Dictionary<int, Location> LocationsIndexValueFactory()
        {
            return Locations.ToDictionary(location => location.LocationId);
        }

        private Dictionary<string, Location> LocationsByValueIndexValueFactory()
        {
            var result = new Dictionary<string, Location>();

            foreach (var location in Locations)
            {
                var key = GetLocationByValueKey(location);
                if (!result.ContainsKey(key)) 
                {
                    result.Add(key, location);
                }
            }

            return result;
        }

        private Dictionary<string, Location> LocationsByBibleChapterIndexValueFactory()
        {
            var result = new Dictionary<string, Location>();

            foreach (var location in Locations)
            {
                if (location.BookNumber != null && location.ChapterNumber != null)
                {
                    var key = GetLocationByBibleChapterKey(
                        location.BookNumber.Value, 
                        location.ChapterNumber.Value,
                        location.KeySymbol);

                    if (!result.ContainsKey(key))
                    {
                        result.Add(key, location);
                    }
                }
            }

            return result;
        }
        
        private Dictionary<int, List<BlockRange>> BlockRangeIndexValueFactory()
        {
            var result = new Dictionary<int, List<BlockRange>>();

            foreach (var range in BlockRanges)
            {
                if (!result.TryGetValue(range.UserMarkId, out var blockRangeList))
                {
                    blockRangeList = new List<BlockRange>();
                    result.Add(range.UserMarkId, blockRangeList);
                }

                blockRangeList.Add(range);
            }
            
            return result;
        }

        private string GetBookmarkKey(int locationId, int publicationLocationId)
        {
            return $"{locationId}-{publicationLocationId}";
        }

        private string GetLocationByValueKey(Location location)
        {
            return $"{location.KeySymbol}|{location.IssueTagNumber}|{location.MepsLanguage}|{location.Type}|{location.BookNumber ?? -1}|{location.ChapterNumber ?? -1}|{location.DocumentId ?? -1}|{location.Track ?? -1}";
        }

        private string GetLocationByBibleChapterKey(int bibleBookNumber, int chapterNumber, string bibleKeySymbol)
        {
            return $"{bibleBookNumber}-{chapterNumber}-{bibleKeySymbol}";
        }

        private Dictionary<string, Bookmark> BookmarkIndexValueFactory()
        {
            var result = new Dictionary<string, Bookmark>();

            foreach (var bookmark in Bookmarks)
            {
                string key = GetBookmarkKey(bookmark.LocationId, bookmark.PublicationLocationId);
                if (!result.ContainsKey(key))
                {
                    result.Add(key, bookmark);
                }
            }

            return result;
        }

        private Dictionary<string, Tag> TagIndexValueFactory()
        {
            return Tags.ToDictionary(tag => tag.Name);
        }

        private Dictionary<int, Tag> TagIdIndexValueFactory()
        {
            return Tags.ToDictionary(tag => tag.TagId);
        }

        private string GetTagMapKey(int tagId, int noteId)
        {
            return $"{tagId}-{noteId}";
        }

        private Dictionary<string, TagMap> TagMapIndexValueFactory()
        {
            var result = new Dictionary<string, TagMap>();

            foreach (var tagMap in TagMaps)
            {
                string key = GetTagMapKey(tagMap.TagId, tagMap.TypeId);
                result.Add(key, tagMap);
            }

            return result;
        }

        private void CheckUserMarkValidity()
        {
            foreach (var userMark in UserMarks)
            {
                if (FindLocation(userMark.LocationId) == null)
                {
                    throw new BackupFileServicesException($"Could not find location for user mark {userMark.UserMarkId}");
                }
            }
        }

        private void CheckTagMapValidity()
        {
            foreach (var tagMap in TagMaps)
            {
                if (tagMap.Type == 1)
                {
                    if (FindTag(tagMap.TagId) == null)
                    {
                        throw new BackupFileServicesException($"Could not find tag for tag map {tagMap.TagMapId}");
                    }

                    if (FindNote(tagMap.TypeId) == null)
                    {
                        throw new BackupFileServicesException($"Could not find note for tag map {tagMap.TagMapId}");
                    }
                }
            }
        }

        private void CheckNoteValidity()
        {
            foreach (var note in Notes)
            {
                if (note.UserMarkId != null && FindUserMark(note.UserMarkId.Value) == null)
                {
                    throw new BackupFileServicesException($"Could not find user mark Id for note {note.NoteId}");
                }

                if (note.LocationId != null && FindLocation(note.LocationId.Value) == null)
                {
                    throw new BackupFileServicesException($"Could not find location for note {note.NoteId}");
                }
            }
        }

        private void CheckBookmarkValidity()
        {
            foreach (var bookmark in Bookmarks)
            {
                if (FindLocation(bookmark.LocationId) == null ||
                    FindLocation(bookmark.PublicationLocationId) == null)
                {
                    throw new BackupFileServicesException($"Could not find location for bookmark {bookmark.BookmarkId}");
                }
            }
        }

        private void CheckBlockRangeValidity()
        {
            foreach (var range in BlockRanges)
            {
                if (FindUserMark(range.UserMarkId) == null)
                {
                    throw new BackupFileServicesException($"Could not find user mark Id for block range {range.BlockRangeId}");
                }
            }
        }

        private void ReinitializeIndexes()
        {
            _notesGuidIndex = new Lazy<Dictionary<string, Note>>(NoteIndexValueFactory);
            _notesIdIndex = new Lazy<Dictionary<int, Note>>(NoteIdIndexValueFactory);
            _notesVerseIndex = new Lazy<Dictionary<BibleBookChapterAndVerse, List<Note>>>(NoteVerseIndexValueFactory);
            _userMarksGuidIndex = new Lazy<Dictionary<string, UserMark>>(UserMarkIndexValueFactory);
            _userMarksIdIndex = new Lazy<Dictionary<int, UserMark>>(UserMarkIdIndexValueFactory);
            _userMarksLocationIdIndex = new Lazy<Dictionary<int, List<UserMark>>>(UserMarksLocationIdIndexValueFactory);
            _locationsIdIndex = new Lazy<Dictionary<int, Location>>(LocationsIndexValueFactory);
            _locationsValueIndex = new Lazy<Dictionary<string, Location>>(LocationsByValueIndexValueFactory);
            _locationsBibleChapterIndex = new Lazy<Dictionary<string, Location>>(LocationsByBibleChapterIndexValueFactory);
            _tagsNameIndex = new Lazy<Dictionary<string, Tag>>(TagIndexValueFactory);
            _tagsIdIndex = new Lazy<Dictionary<int, Tag>>(TagIdIndexValueFactory);
            _tagMapIndex = new Lazy<Dictionary<string, TagMap>>(TagMapIndexValueFactory);
            _blockRangesUserMarkIdIndex = new Lazy<Dictionary<int, List<BlockRange>>>(BlockRangeIndexValueFactory);
            _bookmarksIndex = new Lazy<Dictionary<string, Bookmark>>(BookmarkIndexValueFactory);
        }
    }
}
