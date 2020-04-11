namespace JWLMerge.BackupFileServices.Models.ManifestFile
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// The manifest file.
    /// </summary>
    /// <remarks>We implement INotifyPropertyChanged to prevent the common "WPF binding leak".</remarks>
    public sealed class Manifest : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
