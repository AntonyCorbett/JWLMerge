namespace JWLMerge.Tests
{
    using JWLMerge.BackupFileServices.Helpers;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.ManifestFile;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestCleaner : TestBase
    {
        [TestMethod]
        public void TestAllClean()
        {
            BackupFile file = CreateMockBackup();
            file.Manifest = new Manifest();
            
            Cleaner cleaner = new Cleaner(file.Database);
            int removedRows = cleaner.Clean();
            
            Assert.AreEqual(0, removedRows);
        }

        [TestMethod]
        public void TestRemoveRedundantLocations()
        {
            BackupFile file = CreateMockBackup();
            file.Manifest = new Manifest();

            Cleaner cleaner = new Cleaner(file.Database);
            int removedRows = cleaner.Clean();

            Assert.AreEqual(0, removedRows);
        }
    }
}
