namespace JWLMerge.BackupFileServices.Models
{
    using ManifestFile;

    /// <summary>
    /// The Backup file.
    /// </summary>
    public class BackupFile
    {
        public Manifest Manifest { get; set; }
        
        public Database.Database Database { get; set; }
    }
}
