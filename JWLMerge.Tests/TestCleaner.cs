namespace JWLMerge.Tests
{
    using BackupFileServices.Helpers;
    using BackupFileServices.Models.ManifestFile;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestCleaner : TestBase
    {
        [TestMethod]
        public void TestAllClean()
        {
            var file = CreateMockBackup();
            //file.Manifest = new Manifest();
            
            var cleaner = new Cleaner(file.Database);
            var removedRows = cleaner.Clean();
            
            Assert.AreEqual(0, removedRows);
        }
    }
}
