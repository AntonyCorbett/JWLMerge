namespace JWLMerge.Tests
{
    using BackupFileServices.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestMerge : TestBase
    {
        [TestMethod]
        public void TestMerge1()
        {
            const int numRecords = 100;
            
            var file1 = CreateMockBackup(numRecords);
            var file2 = CreateMockBackup(numRecords);
            var file3 = CreateMockBackup(numRecords);
            
            var merger = new Merger();
            var mergedDatabase = merger.Merge(new[] { file1.Database, file2.Database, file3.Database });
            
            mergedDatabase.CheckValidity();
        }
    }
}
