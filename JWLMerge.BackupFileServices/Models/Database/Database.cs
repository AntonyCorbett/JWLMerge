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
        private Lazy<Dictionary<string, UserMark>> _userMarksGuidIndex;
        private Lazy<Dictionary<int, UserMark>> _userMarksIdIndex;
        private Lazy<Dictionary<int, Location>> _locationsIdIndex;
        private Lazy<Dictionary<string, Location>> _locationsKeySymbolIndex;
        private Lazy<Dictionary<string, Tag>> _tagsNameIndex;
        private Lazy<Dictionary<int, Tag>> _tagsIdIndex;
        private Lazy<Dictionary<string, TagMap>> _tagMapIndex;
        private Lazy<Dictionary<int, BlockRange>> _blockRangeUserMarkIdIndex;
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

        public Note FindNote(string guid)
        {
            return _notesGuidIndex.Value.TryGetValue(guid, out var note) ? note : null;
        }

        public Note FindNote(int noteId)
        {
            return _notesIdIndex.Value.TryGetValue(noteId, out var note) ? note : null;
        }

        public UserMark FindUserMark(string guid)
        {
            return _userMarksGuidIndex.Value.TryGetValue(guid, out var userMark) ? userMark : null;
        }

        public UserMark FindUserMark(int userMarkId)
        {
            return _userMarksIdIndex.Value.TryGetValue(userMarkId, out var userMark) ? userMark : null;
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

        public Location FindPublicationLocation(string keySymbol)
        {
            return _locationsKeySymbolIndex.Value.TryGetValue(keySymbol, out var location) ? location : null;
        }

        public BlockRange FindBlockRange(int userMarkId)
        {
            // note that we find a block range by userMarkId. The BlockRange.UserMarkId column 
            // isn't marked as a unique index, but we assume it should be.
            return _blockRangeUserMarkIdIndex.Value.TryGetValue(userMarkId, out var range) ? range : null;
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

        private Dictionary<string, UserMark> UserMarkIndexValueFactory()
        {
            return UserMarks.ToDictionary(userMark => userMark.UserMarkGuid);
        }

        private Dictionary<int, UserMark> UserMarkIdIndexValueFactory()
        {
            return UserMarks.ToDictionary(userMark => userMark.UserMarkId);
        }

        private Dictionary<int, Location> LocationsIndexValueFactory()
        {
            return Locations.ToDictionary(location => location.LocationId);
        }

        private Dictionary<string, Location> LocationsKeySymbolIndexValueFactory()
        {
            var result = new Dictionary<string, Location>();

            foreach (var location in Locations)
            {
                if (!result.ContainsKey(location.KeySymbol) && location.Type == 1)
                {
                    result.Add(location.KeySymbol, location);
                }
            }

            return result;
        }

        private Dictionary<int, BlockRange> BlockRangeIndexValueFactory()
        {
            return BlockRanges.ToDictionary(range => range.UserMarkId);
        }

        private string GetBookmarkKey(int locationId, int publicationLocationId)
        {
            return $"{locationId}-{publicationLocationId}";
        }

        private Dictionary<string, Bookmark> BookmarkIndexValueFactory()
        {
            var result = new Dictionary<string, Bookmark>();

            foreach (var bookmark in Bookmarks)
            {
                string key = GetBookmarkKey(bookmark.LocationId, bookmark.PublicationLocationId);
                result.Add(key, bookmark);
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
            _userMarksGuidIndex = new Lazy<Dictionary<string, UserMark>>(UserMarkIndexValueFactory);
            _userMarksIdIndex = new Lazy<Dictionary<int, UserMark>>(UserMarkIdIndexValueFactory);
            _locationsIdIndex = new Lazy<Dictionary<int, Location>>(LocationsIndexValueFactory);
            _locationsKeySymbolIndex = new Lazy<Dictionary<string, Location>>(LocationsKeySymbolIndexValueFactory);
            _tagsNameIndex = new Lazy<Dictionary<string, Tag>>(TagIndexValueFactory);
            _tagsIdIndex = new Lazy<Dictionary<int, Tag>>(TagIdIndexValueFactory);
            _tagMapIndex = new Lazy<Dictionary<string, TagMap>>(TagMapIndexValueFactory);
            _blockRangeUserMarkIdIndex = new Lazy<Dictionary<int, BlockRange>>(BlockRangeIndexValueFactory);
            _bookmarksIndex = new Lazy<Dictionary<string, Bookmark>>(BookmarkIndexValueFactory);
        }
    }
}
