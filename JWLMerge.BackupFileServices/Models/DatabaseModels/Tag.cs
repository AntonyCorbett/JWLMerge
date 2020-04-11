namespace JWLMerge.BackupFileServices.Models.DatabaseModels
{
    public class Tag
    {
        /// <summary>
        /// The tag identifier.
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// The tag type.
        /// There appear to be 3 tag types (0 = Favourite, 1 = User-defined, 2 = ?).
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The optional image file name.
        /// </summary>
        /// <remarks>Added in db ver 7 April 2020.</remarks>
        public string ImageFileName { get; set; }

        public Tag Clone()
        {
            return (Tag)MemberwiseClone();
        }
    }
}
