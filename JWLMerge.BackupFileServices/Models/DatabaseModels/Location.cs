namespace JWLMerge.BackupFileServices.Models.DatabaseModels
{
    /// <summary>
    /// The Location table row.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// The location identifier.
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// The Bible book number (or null if not Bible).
        /// </summary>
        public int? BookNumber { get; set; }

        /// <summary>
        /// The Bible chapter number (or null if not Bible).
        /// </summary>
        public int? ChapterNumber { get; set; }

        /// <summary>
        /// The JWL document identifier.
        /// </summary>
        public int? DocumentId { get; set; }

        /// <summary>
        /// The track. Semantics unknown!
        /// </summary>
        public int? Track { get; set; }

        /// <summary>
        /// A reference to the publication issue (if applicable), e.g. "20171100"
        /// </summary>
        public int IssueTagNumber { get; set; }

        /// <summary>
        /// The JWL publication key symbol.
        /// </summary>
        public string KeySymbol { get; set; }

        /// <summary>
        /// The MEPS identifier for the publication language.
        /// </summary>
        public int MepsLanguage { get; set; }

        /// <summary>
        /// The type. 
        /// 0 = standard location entry
        /// 1 = reference to a publication (see Bookmark.PublicationLocationId)
        /// 2 = ?
        /// 3 = ?
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// A location title (nullable).
        /// </summary>
        public string Title { get; set; }

        public Location Clone()
        {
            return (Location)MemberwiseClone();
        }
    }
}
