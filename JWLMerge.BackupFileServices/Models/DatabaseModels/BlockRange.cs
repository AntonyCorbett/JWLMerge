namespace JWLMerge.BackupFileServices.Models.DatabaseModels
{
    public class BlockRange
    {
        /// <summary>
        /// The block range identifier.
        /// </summary>
        public int BlockRangeId { get; set; }

        /// <summary>
        /// The block type (1 or 2).
        /// 1 = Publication
        /// 2 = Bible
        /// </summary>
        public int BlockType { get; set; }

        /// <summary>
        /// The paragraph or verse identifier.
        /// i.e. the one-based paragraph (or verse if a Bible chapter) within the document.
        /// </summary>
        public int Identifier { get; set; }

        /// <summary>
        /// The start token.
        /// i.e. the zero-based word in a sentence that marks the start of the highlight.
        /// </summary>
        public int? StartToken { get; set; }

        /// <summary>
        /// The end token.
        /// i.e. the zero-based word in a sentence that marks the end of the highlight (inclusive).
        /// </summary>
        public int? EndToken { get; set; }

        /// <summary>
        /// The user mark identifier.
        /// Refers to userMark.UserMarkId
        /// </summary>
        public int UserMarkId { get; set; }

        public BlockRange Clone()
        {
            return (BlockRange)MemberwiseClone();
        }
    }
}
