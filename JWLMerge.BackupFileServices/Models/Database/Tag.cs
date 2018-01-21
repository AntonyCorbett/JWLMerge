namespace JWLMerge.BackupFileServices.Models.Database
{
    public class Tag
    {
        /// <summary>
        /// The tag identifier.
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// The tag type.
        /// There appear to be 2 tag types (0 = Favourite, 1 = User-defined).
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string Name { get; set; }

        public Tag Clone()
        {
            return (Tag)MemberwiseClone();
        }
    }
}
