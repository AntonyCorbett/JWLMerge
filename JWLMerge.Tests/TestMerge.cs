using JWLMerge.BackupFileServices.Helpers;

namespace JWLMerge.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestMerge : TestBase
    {
        [TestMethod]
        public void TestMerge1()
        {
            int numNotes = 100;
            
            var file1 = CreateMockBackup(numNotes);
            var file2 = CreateMockBackup(numNotes);
            var file3 = CreateMockBackup(numNotes);
            
            Merger merger = new Merger();
            var mergedDatabase = merger.Merge(new[] { file1.Database, file2.Database, file3.Database });
            
            Assert.AreEqual(mergedDatabase.Locations.Count, numNotes * 3);

            mergedDatabase.CheckValidity();
        }
    }
}
