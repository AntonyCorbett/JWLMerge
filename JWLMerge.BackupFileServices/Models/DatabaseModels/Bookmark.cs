namespace JWLMerge.BackupFileServices.Models.DatabaseModels
{
    public class Bookmark
    {
        /// <summary>
        /// The bookmark identifier.
        /// </summary>
        public int BookmarkId { get; set; }

        /// <summary>
        /// The location identifier.
        /// Refers to Location.LocationId
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// The publication location identifier.
        /// Refers to Location.LocationId (with Location.Type = 1)
        /// </summary>
        public int PublicationLocationId { get; set; }

        /// <summary>
        /// The slot in which the bookmark appears.
        /// i.e. the zero-based order in which it is listed in the UI.
        /// </summary>
        public int Slot { get; set; }

        /// <summary>
        /// The title text.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A snippet of the bookmarked text (can be null)
        /// </summary>
        public string Snippet { get; set; }

        /// <summary>
        /// The block type.
        /// 0 = Bible chapter?
        /// 1 = Publication paragraph
        /// 2 = Bible verse
        /// </summary>
        public int BlockType { get; set; }

        /// <summary>
        /// The block identifier.
        /// The paragraph or verse identifier.
        /// i.e. the one-based paragraph (or verse if a Bible chapter) within the document.
        /// </summary>
        public int? BlockIdentifier { get; set; }

        public Bookmark Clone()
        {
            return (Bookmark)MemberwiseClone();
        }
    }
}
