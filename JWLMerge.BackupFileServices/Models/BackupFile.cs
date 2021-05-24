namespace JWLMerge.BackupFileServices.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using ManifestFile;

    /// <summary>
    /// The Backup file.
    /// </summary>
    /// <remarks>We implement INotifyPropertyChanged to prevent the common "WPF binding leak".</remarks>
    public sealed class BackupFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Manifest Manifest { get; set; } = null!;
        
        public DatabaseModels.Database Database { get; set; } = null!;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable S1144 // Remove unused private members
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
#pragma warning restore S1144 // Remove unused private members
#pragma warning restore IDE0051 // Remove unused private members
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
