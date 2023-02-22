using JWLMerge.BackupFileServices.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JWLMerge.Tests;

[TestClass]
public class TestCleaner : TestBase
{
    [TestMethod]
    public void TestAllClean()
    {
        var file = CreateMockBackup();
            
        var cleaner = new Cleaner(file.Database);
        var removedRows = cleaner.Clean();
            
        Assert.AreEqual(0, removedRows);
    }
}