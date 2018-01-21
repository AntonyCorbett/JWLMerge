namespace JWLMerge.BackupFileServices.Models.ManifestFile
{
    /// <summary>
    /// The manifest file.
    /// </summary>
    public class Manifest
    {
        /// <summary>
        /// The name of the backup file (without the "jwlibrary" extension).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The local creation date in the form "YYYY-MM-DD"
        /// </summary>
        public string CreationDate { get; set; }

        /// <summary>
        /// The manifest schema version.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The type. Semantics unknown!
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Details of the backup database.
        /// </summary>
        public UserDataBackup UserDataBackup { get; set; }

        public Manifest Clone()
        {
            return (Manifest)MemberwiseClone();
        }
    }
}
