namespace JWLMerge.BackupFileServices.Models.Database
{
    public class TagMap
    {
        /// <summary>
        /// The tag map identifier.
        /// </summary>
        public int TagMapId { get; set; }

        /// <summary>
        /// The type of data that the tag is attached to.
        /// Currently it looks like there is only 1 'type' - a Note (value = 1).
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// The identifier of the data that the tag is attached to.
        /// Currently it looks like this always refers to Note.NoteId
        /// </summary>
        public int TypeId { get; set; }

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
