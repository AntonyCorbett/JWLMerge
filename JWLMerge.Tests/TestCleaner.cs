namespace JWLMerge.Tests
{
    using BackupFileServices.Helpers;
    using BackupFileServices.Models;
    using BackupFileServices.Models.ManifestFile;
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
            
            Assert.AreEqual(removedRows, 0);
        }

        [TestMethod]
        public void TestRemoveRedundantLocations()
        {
            BackupFile file = CreateMockBackup();
            file.Manifest = new Manifest();

            Cleaner cleaner = new Cleaner(file.Database);
            int removedRows = cleaner.Clean();

            Assert.AreEqual(removedRows, 0);
        }
    }
}
