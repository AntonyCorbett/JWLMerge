namespace JWLMerge.BackupFileServices.Models.ManifestFile
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// The user data backup.
    /// Part of the manifest.
    /// </summary>
    /// <remarks>We implement INotifyPropertyChanged to prevent the common "WPF binding leak".</remarks>
    public sealed class UserDataBackup : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The last modified date of the database in ISO 8601, e.g. "2018-01-17T14:37:27+00:00"
        /// Corresponds to the value in the LastModifiedDate table.
        /// </summary>
        public string LastModifiedDate { get; set; }

        /// <summary>
        /// The name of the source device (e.g. the name of the PC).
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// The database name (always "userData.db"?)
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// A sha256 hash of the associated database file.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// The database schema version.
        /// Note that the database records its own schema version in the user_version header pragma.
        /// </summary>
        public int SchemaVersion { get; set; }

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable S1144 // Remove unused private members
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
#pragma warning restore S1144 // Remove unused private members
#pragma warning restore IDE0051 // Remove unused private members
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
