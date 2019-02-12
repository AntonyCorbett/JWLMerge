using System.Linq;
using JWLMerge.BackupFileServices.Models;

namespace JWLMerge.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using JWLMerge.BackupFileServices.Helpers;
    using JWLMerge.BackupFileServices.Models.Database;

    [TestClass]
    public class TestImportNotes : TestBase
    {
        [TestMethod]
        public void TestImport1()
        {
            int numRecords = 100;
            
            var file1 = CreateMockBackup(numRecords);
            var notes = CreateMockBibleNotes().ToArray();

            int mepsLanguageId = 0;
            var importer = new NotesImporter(file1.Database, "nwtsty", mepsLanguageId);
            var result = importer.Import(notes);

            file1.Database.CheckValidity();

            Assert.AreEqual(notes.Length, result.BibleNotesAdded);
            Assert.IsTrue(result.BibleNotesUnchanged == 0);
            Assert.IsTrue(result.BibleNotesUpdated == 0);

            result = importer.Import(notes);

            Assert.AreEqual(notes.Length, result.BibleNotesUnchanged);
            Assert.IsTrue(result.BibleNotesAdded == 0);
            Assert.IsTrue(result.BibleNotesUpdated == 0);
        }

        private IEnumerable<BibleNote> CreateMockBibleNotes()
        {
            var result = new List<BibleNote>();

            result.Add(new BibleNote
            {
                BookChapterAndVerse = new BibleBookChapterAndVerse {BookNumber = 1, ChapterNumber = 1, VerseNumber = 1},
                NoteTitle = "A note 1",
                NoteContent = "My notes go here"
            });

            result.Add(new BibleNote
            {
                BookChapterAndVerse = new BibleBookChapterAndVerse { BookNumber = 2, ChapterNumber = 2, VerseNumber = 2 },
                NoteTitle = "A note 2",
                NoteContent = "My notes go here"
            });

            return result;
        }
    }
}
