namespace JWLMerge.BackupFileServices.Models
{
    using JWLMerge.BackupFileServices.Models.ManifestFile;

    /// <summary>
    /// The Backup file.
    /// </summary>
    public class BackupFile
    {
        public Manifest Manifest { get; set; }
        
        public DatabaseModels.Database Database { get; set; }
    }
}
