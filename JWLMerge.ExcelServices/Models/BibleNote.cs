namespace JWLMerge.ExcelServices.Models
{
    public class BibleNote
    {
        public int BookNumber { get; set; }

        public string BookName { get; set; }

        public int? ChapterNumber { get; set; }

        public int? VerseNumber { get; set; }

        public string NoteTitle { get; set; }

        public string NoteContent { get; set; }

        public string TagsCsv { get; set; }

        public int? ColorCode { get; set; }

        public string PubSymbol { get; set; }

        public string ChapterAndVerseString
        {
            get
            {
                if (ChapterNumber == null)
                {
                    return string.Empty;
                }

                if (VerseNumber == null)
                {
                    return ChapterNumber.ToString();
                }

                return $"{ChapterNumber}:{VerseNumber}";
            }
        }

        public string BookNameChapterAndVerseString => $"{BookName} {ChapterAndVerseString}";
    }
}
