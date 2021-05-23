namespace JWLMerge.Models
{
    using System.Text;
    using JWLMerge.BackupFileServices.Models;
    using Microsoft.Toolkit.Mvvm.ComponentModel;

    internal class JwLibraryFile : ObservableObject
    {
        public JwLibraryFile()
        {
            MergeParameters = new MergeParameters();
            MergeParameters.PropertyChanged += MergeParametersPropertyChanged;
        }

        public string FilePath { get; set; }
        
        public BackupFile BackupFile { get; set; }

        public MergeParameters MergeParameters { get; }

        public bool NotesRedacted { get; set; }

        public string TooltipSummaryText
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{BackupFile.Database.Notes.Count} notes");
                sb.AppendLine($"{BackupFile.Database.Bookmarks.Count} bookmarks");
                sb.AppendLine($"{BackupFile.Database.UserMarks.Count} underlining");
                sb.AppendLine($"{BackupFile.Database.InputFields.Count} input fields");
                sb.Append($"{BackupFile.Database.Tags.Count} tags");

                return sb.ToString();
            }
        }

        public void RefreshTooltipSummary()
        {
            OnPropertyChanged(nameof(TooltipSummaryText));
        }

        private void MergeParametersPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(MergeParameters));
        }
    }
}
