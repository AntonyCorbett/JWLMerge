namespace JWLMerge.BackupFileServices.Models.DatabaseModels
{
    public class BibleNote
    {
        public BibleBookChapterAndVerse BookChapterAndVerse { get; set; }

        public int? StartTokenInVerse { get; set; }

        public int? EndTokenInVerse { get; set; }

        public int ColourIndex { get; set; }

        public string NoteTitle { get; set; }

        public string NoteContent { get; set; }
    }
}
