using System.Linq;
using JWLMerge.BackupFileServices.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JWLMerge.Tests;

[TestClass]
public class TestMerge : TestBase
{
    [TestMethod]
    public void TestMerge1()
    {
        const int numRecords = 100;
        const int numFilesToMerge = 3;

        var files = Enumerable.Range(1, numFilesToMerge).Select(_ => CreateMockBackup(numRecords))?.ToArray();
        Assert.IsNotNull(files);
            
        var merger = new Merger();
        var mergedDatabase = merger.Merge(files.Select(x => x.Database));
            
        mergedDatabase.CheckValidity();
        
        Assert.AreEqual(numRecords * numFilesToMerge, mergedDatabase.UserMarks.Count);
        Assert.IsTrue(mergedDatabase.Locations.Count > numRecords);
        Assert.AreEqual(numRecords * numFilesToMerge, mergedDatabase.Notes.Count);
        Assert.AreEqual(numRecords * numFilesToMerge, mergedDatabase.BlockRanges.Count);
    }
}