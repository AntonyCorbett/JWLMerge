using System;
using System.Collections.Generic;
using System.Linq;
using JWLMerge.BackupFileServices.Helpers;
using JWLMerge.BackupFileServices.Models;
using JWLMerge.BackupFileServices.Models.DatabaseModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JWLMerge.Tests;

[TestClass]
public class TestImportNotes : TestBase
{
    [TestMethod]
    public void TestBibleNotesFile()
    {
        const string lines =
            @"[BibleKeySymbol=nwtsty]
[MepsLanguageId=0]

[1:3:15]
A note

[1:3:15:4:4:2]
Another note.

Another paragraph
in the note.

[BibleKeySymbol=nwtstyAnother]
[MepsLanguageId=1]

[1:3:15]
A note in another language

[1:3:15:4:4:0]
Another note in the second language.

Another paragraph
in the note.";

        var file = new BibleNotesFile(lines.Split(Environment.NewLine));

        var sections = file.GetPubSymbolsAndLanguages();
        Assert.AreEqual(2, sections.Length);
        
        Assert.AreEqual("nwtsty", sections[0].PubSymbol);
        Assert.AreEqual(0, sections[0].LanguageId);

        Assert.AreEqual("nwtstyAnother", sections[1].PubSymbol);
        Assert.AreEqual(1, sections[1].LanguageId);

        var notes = file.GetNotes(new PubSymbolAndLanguage("nwtsty", 0)).ToArray();
        Assert.AreEqual(2, notes.Length);

        Assert.AreEqual(new BibleBookChapterAndVerse(1, 3, 15), notes[0].BookChapterAndVerse);
        Assert.AreEqual("A note", notes[0].NoteTitle);
        Assert.AreEqual(string.Empty, notes[0].NoteContent);

        Assert.AreEqual(new BibleBookChapterAndVerse(1, 3, 15), notes[1].BookChapterAndVerse);
        Assert.AreEqual(4, notes[1].StartTokenInVerse);
        Assert.AreEqual(4, notes[1].EndTokenInVerse);
        Assert.AreEqual(2, notes[1].ColourIndex);
        Assert.AreEqual("Another note.", notes[1].NoteTitle);
        Assert.AreEqual("Another paragraph\r\nin the note.", notes[1].NoteContent);

        notes = file.GetNotes(new PubSymbolAndLanguage("nwtstyAnother", 1)).ToArray();
        Assert.AreEqual(2, notes.Length);

        Assert.AreEqual(new BibleBookChapterAndVerse(1, 3, 15), notes[0].BookChapterAndVerse);
    }

    [TestMethod]
    public void TestImport1()
    {
        const int numRecords = 100;
            
        var file1 = CreateMockBackup(numRecords);
        var notes = CreateMockBibleNotes().ToArray();
        var mockImportOptions = new ImportBibleNotesParams();

        const int mepsLanguageId = 0;

        var importer = new NotesImporter(
            file1.Database, 
            "nwtsty", 
            mepsLanguageId,
            mockImportOptions);

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

    private static IEnumerable<BibleNote> CreateMockBibleNotes()
    {
        return new List<BibleNote>
        {
            new()
            {
                BookChapterAndVerse = new BibleBookChapterAndVerse(1, 1, 1),
                NoteTitle = "A note 1",
                NoteContent = "My notes go here",
            },
            new()
            {
                BookChapterAndVerse = new BibleBookChapterAndVerse(2, 2, 2),
                NoteTitle = "A note 2",
                NoteContent = "My notes go here",
            },
        };
    }
}