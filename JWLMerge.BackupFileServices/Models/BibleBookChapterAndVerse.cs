namespace JWLMerge.BackupFileServices.Models
{
    using System;
    
    public struct BibleBookChapterAndVerse : IEquatable<BibleBookChapterAndVerse>
    {
        public BibleBookChapterAndVerse(int bookNum, int chapterNum, int verseNum)
        {
            BookNumber = bookNum;
            ChapterNumber = chapterNum;
            VerseNumber = verseNum;
        }

        public int BookNumber { get; }

        public int ChapterNumber { get; }

        public int VerseNumber { get; }

        public static bool operator ==(BibleBookChapterAndVerse lhs, BibleBookChapterAndVerse rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(BibleBookChapterAndVerse lhs, BibleBookChapterAndVerse rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BookNumber;
                hashCode = (hashCode * 397) ^ ChapterNumber;
                hashCode = (hashCode * 397) ^ VerseNumber;
                return hashCode;
            }
        }
        
        public override bool Equals(object obj)
        {
            return obj is BibleBookChapterAndVerse other && Equals(other);
        }

        public bool Equals(BibleBookChapterAndVerse other)
        {
            return BookNumber == other.BookNumber && ChapterNumber == other.ChapterNumber && VerseNumber == other.VerseNumber;
        }
    }
}
