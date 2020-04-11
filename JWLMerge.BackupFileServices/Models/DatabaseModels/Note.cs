namespace JWLMerge.BackupFileServices.Models.DatabaseModels
{
    using System;

    public class Note
    {
        /// <summary>
        /// The note identifier.
        /// </summary>
        public int NoteId { get; set; }

        /// <summary>
        /// A Guid (that should assist in merging notes).
        /// </summary>
#pragma warning disable CA1720 // Identifier contains type name
        public string Guid { get; set; }
#pragma warning restore CA1720 // Identifier contains type name

        /// <summary>
        /// The user mark identifier (if the note is associated with user-highlighting).
        /// A reference to UserMark.UserMarkId
        /// </summary>
        public int? UserMarkId { get; set; }

        /// <summary>
        /// The location identifier (if the note is associated with a location - which it usually is)
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// The user-defined note title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The user-defined note content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Time stamp when the note was last edited. ISO 8601 format.
        /// </summary>
        public string LastModified { get; set; }

        /// <summary>
        /// The type of block associated with the note. 
        /// Valid values are possibly 0, 1, and 2. 
        /// Best guess at semantics:
        ///     0 = The note is associated with the document rather than a block of text within it.
        ///     1 = The note is associated with a paragraph in a publication.
        ///     2 = The note is associated with a verse in the Bible.
        /// In all cases, see also the UserMarkId which may better define the associated block of text.
        /// </summary>
        public int BlockType { get; set; }

        /// <summary>
        /// The block identifier. Helps to locate the block of text associated with the note. 
        /// If the BlockType is 1 (a publication), then BlockIdentifier denotes the paragraph number.
        /// If the BlockType is 2 (the Bible), then BlockIdentifier denotes the verse number.
        /// </summary>
        public int? BlockIdentifier { get; set; }

        public DateTime GetLastModifiedDateTime()
        {
            return DateTime.Parse(LastModified);
        }

        public Note Clone()
        {
            return (Note)MemberwiseClone();
        }
    }
}
