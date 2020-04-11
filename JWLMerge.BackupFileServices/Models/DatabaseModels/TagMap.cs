namespace JWLMerge.BackupFileServices.Models.DatabaseModels
{
    /// <summary>
    /// Note that the db schema for the TagMap table was updated in db v7.
    /// In v5 (the previous public schema) we had a Type column where
    /// 0 = tag on a Location
    /// 1 = tag on a Note
    /// and a corresponding TypeId column. These are now replaced with
    /// mutually exclusive PlaylistItemId, LocationId and NoteId columns.
    /// </summary>
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
        /// <remarks>added in in db v7, Apr 2020.</remarks>
        public int? NoteId { get; set; }

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
