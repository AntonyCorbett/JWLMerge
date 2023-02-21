using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JWLMerge.Models
{
    using System.Text;
    using JWLMerge.BackupFileServices.Models;
    
    internal sealed class JwLibraryFile : ObservableObject
    {
        public JwLibraryFile(string filePath, BackupFile backupFile)
        {
            FilePath = filePath;
            BackupFile = backupFile;

            MergeParameters = new MergeParameters();
            MergeParameters.PropertyChanged += MergeParametersPropertyChanged;
        }

        public string FilePath { get; }
        
        [DisallowNull]
        public BackupFile BackupFile { get; set; }

        public MergeParameters MergeParameters { get; }

        public bool NotesRedacted { get; set; }

        public string TooltipSummaryText
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(CultureInfo.InvariantCulture, $"{BackupFile.Database.Notes.Count} notes");
                sb.AppendLine(CultureInfo.InvariantCulture, $"{BackupFile.Database.Bookmarks.Count} bookmarks");
                sb.AppendLine(CultureInfo.InvariantCulture, $"{BackupFile.Database.InputFields.Count} input fields");
                sb.AppendLine(CultureInfo.InvariantCulture, $"{BackupFile.Database.UserMarks.Count} underlining");
                sb.Append(CultureInfo.InvariantCulture, $"{BackupFile.Database.Tags.Count} tags");

                return sb.ToString();
            }
        }

        public void RefreshTooltipSummary()
        {
            OnPropertyChanged(nameof(TooltipSummaryText));
        }

        private void MergeParametersPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(MergeParameters));
        }
    }
}
