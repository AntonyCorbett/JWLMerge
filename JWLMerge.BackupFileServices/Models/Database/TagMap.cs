namespace JWLMerge.BackupFileServices.Models.Database
{
    public class TagMap
    {
        /// <summary>
        /// The tag map identifier.
        /// </summary>
        public int TagMapId { get; set; }
        
        /// <summary>
        /// Playlist Item Id.
        /// </summary>
        /// <remarks>added in in db v7, Apr 2020</remarks>
        public int? PlaylistItemId { get; set; }

        /// <summary>
        /// Location Id. 
        /// </summary>
        /// <remarks>added in in db v7, Apr 2020</remarks>
        public int? LocationId { get; set; }

        /// <summary>
        /// Note Id.
        /// </summary>
        /// <remarks>added in in db v7, Apr 2020</remarks>
        public int? NoteId { get; set; }

        // removed in db v7
        /// <summary>
        /// The type of data that the tag is attached to.
        /// 0 = tag on a Location
        /// 1 = tag on a Note
        /// </summary>
        //public int Type { get; set; } 

        // removed in db v7
        /// <summary>
        /// The identifier of the data that the tag is attached to.
        /// Currently it looks like this always refers to Note.NoteId
        /// </summary>
        //public int TypeId { get; set; }

        /// <summary>
        /// The tag identifier.
        /// Refers to Tag.TagId.
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// The zero-based position of the tag map entry (among all entries having the same TagId).
        /// (Tagged items can be ordered in the JWL application.)
        /// </summary>
        public int Position { get; set; }

        public TagMap Clone()
        {
            return (TagMap)MemberwiseClone();
        }
    }
}
