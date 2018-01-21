namespace JWLMerge.BackupFileServices.Models.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Database
    {
        private Lazy<Dictionary<string, Note>> _noteIndex;
        private Lazy<Dictionary<string, UserMark>> _userMarksIndex;
        private Lazy<Dictionary<int, Location>> _locationsIndex;
        private Lazy<Dictionary<string, Tag>> _tagsIndex;
        private Lazy<Dictionary<string, TagMap>> _tagMapIndex;
        private Lazy<Dictionary<int, BlockRange>> _blockRangeIndex;

        public Database()
        {
            ReinitializeIndexes();
        }

        public void ReinitializeIndexes()
        {
            _noteIndex = new Lazy<Dictionary<string, Note>>(NoteIndexValueFactory);
            _userMarksIndex = new Lazy<Dictionary<string, UserMark>>(UserMarkIndexValueFactory);
            _locationsIndex = new Lazy<Dictionary<int, Location>>(LocationsIndexValueFactory);
            _tagsIndex = new Lazy<Dictionary<string, Tag>>(TagIndexValueFactory);
            _tagMapIndex = new Lazy<Dictionary<string, TagMap>>(TagMapIndexValueFactory);
            _blockRangeIndex = new Lazy<Dictionary<int, BlockRange>>(BlockRangeIndexValueFactory);
        }

        public void InitBlank()
        {
            LastModified = new LastModified();
            Locations = new List<Location>();
            Notes = new List<Note>();
            Tags = new List<Tag>();
            TagMaps = new List<TagMap>();
            BlockRanges = new List<BlockRange>();
            UserMarks = new List<UserMark>();
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
            return _noteIndex.Value.TryGetValue(guid, out var note) ? note : null;
        }

        public UserMark FindUserMark(string guid)
        {
            return _userMarksIndex.Value.TryGetValue(guid, out var userMark) ? userMark : null;
        }

        public Tag FindTag(string tagName)
        {
            return _tagsIndex.Value.TryGetValue(tagName, out var tag) ? tag : null;
        }

        public TagMap FindTagMap(int tagId, int noteId)
        {
            return _tagMapIndex.Value.TryGetValue(GetTagMapKey(tagId, noteId), out var tag) ? tag : null;
        }

        public Location FindLocation(int locationId)
        {
            return _locationsIndex.Value.TryGetValue(locationId, out var location) ? location : null;
        }

        public BlockRange FindBlockRange(int userMarkId)
        {
            // note that we find a block range by userMarkId. The BlockRange.UserMarkId column 
            // isn't marked as a unique index, but we assume it should be.
            return _blockRangeIndex.Value.TryGetValue(userMarkId, out var range) ? range : null;
        }

        private Dictionary<string, Note> NoteIndexValueFactory()
        {
            return Notes.ToDictionary(note => note.Guid);
        }

        private Dictionary<string, UserMark> UserMarkIndexValueFactory()
        {
            return UserMarks.ToDictionary(userMark => userMark.UserMarkGuid);
        }

        private Dictionary<int, Location> LocationsIndexValueFactory()
        {
            return Locations.ToDictionary(location => location.LocationId);
        }

        private Dictionary<int, BlockRange> BlockRangeIndexValueFactory()
        {
            return BlockRanges.ToDictionary(range => range.UserMarkId);
        }

        private Dictionary<string, Tag> TagIndexValueFactory()
        {
            return Tags.ToDictionary(tag => tag.Name);
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
    }
}
