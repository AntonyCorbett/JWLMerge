namespace JWLMerge.BackupFileServices.Models.DatabaseModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JWLMerge.BackupFileServices.Exceptions;
    using Serilog;

    public class Database
    {
        private readonly Dictionary<int, int> _bookmarkSlots = new Dictionary<int, int>();
        
        private Lazy<Dictionary<string, Note>> _notesGuidIndex;
        private Lazy<Dictionary<int, Note>> _notesIdIndex;
        private Lazy<Dictionary<int, List<InputField>>> _inputFieldsIndex;
        private Lazy<Dictionary<BibleBookChapterAndVerse, List<Note>>> _notesVerseIndex;
        private Lazy<Dictionary<string, UserMark>> _userMarksGuidIndex;
        private Lazy<Dictionary<int, UserMark>> _userMarksIdIndex;
        private Lazy<Dictionary<int, List<UserMark>>> _userMarksLocationIdIndex;
        private Lazy<Dictionary<int, Location>> _locationsIdIndex;
        private Lazy<Dictionary<string, Location>> _locationsValueIndex;
        private Lazy<Dictionary<string, Location>> _locationsBibleChapterIndex;
        private Lazy<Dictionary<TagTypeAndName, Tag>> _tagsNameIndex;
        private Lazy<Dictionary<int, Tag>> _tagsIdIndex;
        private Lazy<Dictionary<string, TagMap>> _tagMapNoteIndex;
        private Lazy<Dictionary<string, TagMap>> _tagMapLocationIndex;
        private Lazy<Dictionary<int, List<BlockRange>>> _blockRangesUserMarkIdIndex;
        private Lazy<Dictionary<string, Bookmark>> _bookmarksIndex;

        public Database()
        {
            ReinitializeIndexes();
        }

        public LastModified LastModified { get; } = new LastModified();

        public List<Location> Locations { get; } = new List<Location>();

        public List<Note> Notes { get; } = new List<Note>();

        public List<InputField> InputFields { get; } = new List<InputField>();

        public List<Tag> Tags { get; } = new List<Tag>();

        public List<TagMap> TagMaps { get; } = new List<TagMap>();

        public List<BlockRange> BlockRanges { get; } = new List<BlockRange>();

        public List<Bookmark> Bookmarks { get; } = new List<Bookmark>();

        public List<UserMark> UserMarks { get; } = new List<UserMark>();

        public static string GetDateTimeUtcAsDbString(DateTime dateTime)
        {
            return $"{dateTime:s}Z";
        }

        public void InitBlank()
        {
            LastModified.Reset();
            Locations.Clear();
            Notes.Clear();
            InputFields.Clear();
            Tags.Clear();
            TagMaps.Clear();
            BlockRanges.Clear();
            Bookmarks.Clear();
            UserMarks.Clear();
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

        public void FixupAnomalies()
        {
            var count = 0;

            count += FixupBlockRangeValidity();
            count += FixupBookmarkValidity();
            count += FixupNoteValidity();
            count += FixupTagMapValidity();
            count += FixupUserMarkValidity();

            if (count > 0)
            {
                ReinitializeIndexes();
            }
        }

        public void AddBibleNoteAndUpdateIndex(
            BibleBookChapterAndVerse verseRef, 
            Note value,
            TagMap tagMap)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Notes.Add(value);

            if (tagMap != null)
            {
                TagMaps.Add(tagMap);
            }

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

        public void AddBlockRangeAndUpdateIndex(BlockRange value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

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
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

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
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

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

            if (_locationsBibleChapterIndex.IsValueCreated && 
                value.BookNumber != null && 
                value.ChapterNumber != null)
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

        public Note FindNote(string noteGuid)
        {
            return _notesGuidIndex.Value.TryGetValue(noteGuid, out var note) ? note : null;
        }

        public Note FindNote(int noteId)
        {
            return _notesIdIndex.Value.TryGetValue(noteId, out var note) ? note : null;
        }

        public IEnumerable<Note> FindNotes(BibleBookChapterAndVerse verseRef)
        {
            return _notesVerseIndex.Value.TryGetValue(verseRef, out var notes) ? notes : null;
        }

        public UserMark FindUserMark(string userMarkGuid)
        {
            return _userMarksGuidIndex.Value.TryGetValue(userMarkGuid, out var userMark) ? userMark : null;
        }

        public UserMark FindUserMark(int userMarkId)
        {
            return _userMarksIdIndex.Value.TryGetValue(userMarkId, out var userMark) ? userMark : null;
        }

        public IEnumerable<UserMark> FindUserMarks(int locationId)
        {
            return _userMarksLocationIdIndex.Value.TryGetValue(locationId, out var userMarks) ? userMarks : null;
        }

        public Tag FindTag(int tagType, string tagName)
        {
            var key = new TagTypeAndName(tagType, tagName);
            return _tagsNameIndex.Value.TryGetValue(key, out var tag) ? tag : null;
        }

        public Tag FindTag(int tagId)
        {
            return _tagsIdIndex.Value.TryGetValue(tagId, out var tag) ? tag : null;
        }

        public TagMap FindTagMapForNote(int tagId, int noteId)
        {
            return _tagMapNoteIndex.Value.TryGetValue(GetTagMapNoteKey(tagId, noteId), out var tag) ? tag : null;
        }

        public TagMap FindTagMapForLocation(int tagId, int locationId)
        {
            return _tagMapLocationIndex.Value.TryGetValue(GetTagMapLocationKey(tagId, locationId), out var tag) ? tag : null;
        }

        public Location FindLocation(int locationId)
        {
            return _locationsIdIndex.Value.TryGetValue(locationId, out var location) ? location : null;
        }

        public InputField FindInputField(int locationId, string textTag)
        {
            if (!_inputFieldsIndex.Value.TryGetValue(locationId, out var list))
            {
                return null;
            }

            return list.SingleOrDefault(x => x.TextTag.Equals(textTag, StringComparison.OrdinalIgnoreCase));
        }

        public Location FindLocationByValues(Location locationValues)
        {
            if (locationValues == null)
            {
                throw new ArgumentNullException(nameof(locationValues));
            }

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

        private Dictionary<int, List<InputField>> InputFieldsIndexValueFactory()
        {
            var result = new Dictionary<int, List<InputField>>();

            foreach (var fld in InputFields)
            {
                if (!result.TryGetValue(fld.LocationId, out var list))
                {
                    list = new List<InputField>();
                    result.Add(fld.LocationId, list);
                }

                list.Add(fld);
            }

            return result;
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
                        var verseRef = new BibleBookChapterAndVerse(
                            location.BookNumber.Value,
                            location.ChapterNumber.Value, 
                            note.BlockIdentifier.Value);

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

        private Dictionary<TagTypeAndName, Tag> TagIndexValueFactory()
        {
            return Tags.ToDictionary(tag => new TagTypeAndName(tag.Type, tag.Name));
        }

        private Dictionary<int, Tag> TagIdIndexValueFactory()
        {
            return Tags.ToDictionary(tag => tag.TagId);
        }

        private string GetTagMapNoteKey(int tagId, int noteId)
        {
            return $"{tagId}-{noteId}";
        }

        private string GetTagMapLocationKey(int tagId, int locationId)
        {
            return $"{tagId}-{locationId}";
        }

        private Dictionary<string, TagMap> TagMapNoteIndexValueFactory()
        {
            var result = new Dictionary<string, TagMap>();

            foreach (var tagMap in TagMaps)
            {
                if (tagMap.NoteId != null)
                {
                    string key = GetTagMapNoteKey(tagMap.TagId, tagMap.NoteId.Value);
                    result.Add(key, tagMap);
                }
            }

            return result;
        }

        private Dictionary<string, TagMap> TagMapLocationIndexValueFactory()
        {
            var result = new Dictionary<string, TagMap>();

            foreach (var tagMap in TagMaps)
            {
                if (tagMap.LocationId != null)
                {
                    string key = GetTagMapLocationKey(tagMap.TagId, tagMap.LocationId.Value);
                    result.Add(key, tagMap);
                }
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
                if (tagMap.NoteId != null)
                {
                    if (FindTag(tagMap.TagId) == null)
                    {
                        throw new BackupFileServicesException($"Could not find tag for tag map {tagMap.TagMapId}");
                    }

                    if (FindNote(tagMap.NoteId.Value) == null)
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
            _inputFieldsIndex = new Lazy<Dictionary<int, List<InputField>>>(InputFieldsIndexValueFactory);
            _notesVerseIndex = new Lazy<Dictionary<BibleBookChapterAndVerse, List<Note>>>(NoteVerseIndexValueFactory);
            _userMarksGuidIndex = new Lazy<Dictionary<string, UserMark>>(UserMarkIndexValueFactory);
            _userMarksIdIndex = new Lazy<Dictionary<int, UserMark>>(UserMarkIdIndexValueFactory);
            _userMarksLocationIdIndex = new Lazy<Dictionary<int, List<UserMark>>>(UserMarksLocationIdIndexValueFactory);
            _locationsIdIndex = new Lazy<Dictionary<int, Location>>(LocationsIndexValueFactory);
            _locationsValueIndex = new Lazy<Dictionary<string, Location>>(LocationsByValueIndexValueFactory);
            _locationsBibleChapterIndex = new Lazy<Dictionary<string, Location>>(LocationsByBibleChapterIndexValueFactory);
            _tagsNameIndex = new Lazy<Dictionary<TagTypeAndName, Tag>>(TagIndexValueFactory);
            _tagsIdIndex = new Lazy<Dictionary<int, Tag>>(TagIdIndexValueFactory);
            _tagMapNoteIndex = new Lazy<Dictionary<string, TagMap>>(TagMapNoteIndexValueFactory);
            _tagMapLocationIndex = new Lazy<Dictionary<string, TagMap>>(TagMapLocationIndexValueFactory);
            _blockRangesUserMarkIdIndex = new Lazy<Dictionary<int, List<BlockRange>>>(BlockRangeIndexValueFactory);
            _bookmarksIndex = new Lazy<Dictionary<string, Bookmark>>(BookmarkIndexValueFactory);
        }

        private int FixupUserMarkValidity()
        {
            var fixupCount = 0;

            for (var n = UserMarks.Count - 1; n >= 0; --n)
            {
                var userMark = UserMarks[n];

                if (FindLocation(userMark.LocationId) == null)
                {
                    ++fixupCount;
                    UserMarks.RemoveAt(n);

                    Log.Logger.Error($"Removed invalid user mark {userMark.UserMarkId}");
                }
            }
            
            return fixupCount;
        }

        private int FixupTagMapValidity()
        {
            var fixupCount = 0;

            for (var n = TagMaps.Count - 1; n >= 0; --n)
            {
                var tagMap = TagMaps[n];

                if (tagMap.NoteId != null && 
                    (FindTag(tagMap.TagId) == null || FindNote(tagMap.NoteId.Value) == null))
                {
                    ++fixupCount;
                    TagMaps.RemoveAt(n);

                    Log.Logger.Error($"Removed invalid tag map {tagMap.TagMapId}");
                }
            }

            return fixupCount;
        }

        private int FixupNoteValidity()
        {
            var fixupCount = 0;

            for (var n = Notes.Count - 1; n >= 0; --n)
            {
                var note = Notes[n];
                
                if (note.UserMarkId != null && FindUserMark(note.UserMarkId.Value) == null)
                {
                    ++fixupCount;
                    note.UserMarkId = null;

                    Log.Logger.Error($"Cleared invalid user mark ID for note {note.NoteId}");
                }

                if (note.LocationId != null && FindLocation(note.LocationId.Value) == null)
                {
                    ++fixupCount;
                    note.LocationId = null;

                    Log.Logger.Error($"Cleared invalid location ID for note {note.NoteId}");
                }
            }

            return fixupCount;
        }

        private int FixupBookmarkValidity()
        {
            var fixupCount = 0;

            for (var n = Bookmarks.Count - 1; n >= 0; --n)
            {
                var bookmark = Bookmarks[n];

                if (FindLocation(bookmark.LocationId) == null ||
                    FindLocation(bookmark.PublicationLocationId) == null)
                {
                    ++fixupCount;
                    Bookmarks.RemoveAt(n);

                    Log.Logger.Error($"Removed invalid bookmark {bookmark.BookmarkId}");
                }
            }

            return fixupCount;
        }

        private int FixupBlockRangeValidity()
        {
            var fixupCount = 0;

            for (var n = BlockRanges.Count - 1; n >= 0; --n)
            {
                var range = BlockRanges[n];

                if (FindUserMark(range.UserMarkId) == null)
                {
                    ++fixupCount;
                    BlockRanges.RemoveAt(n);

                    Log.Logger.Error($"Removed invalid block range {range.BlockRangeId}");
                }
            }
            
            return fixupCount;
        }
    }
}
