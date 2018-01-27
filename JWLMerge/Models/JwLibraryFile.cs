namespace JWLMerge.Models
{
    using System.Text;
    using BackupFileServices.Models;
    using GalaSoft.MvvmLight;

    internal class JwLibraryFile : ViewModelBase
    {
        public string FilePath { get; set; }
        
        public BackupFile BackupFile { get; set; }

        public MergeParameters MergeParameters { get; }

        public string TooltipSummaryText
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{BackupFile.Database.Notes.Count} notes");
                sb.AppendLine($"{BackupFile.Database.Bookmarks.Count} bookmarks");
                sb.AppendLine($"{BackupFile.Database.UserMarks.Count} underlining");
                sb.Append($"{BackupFile.Database.Tags.Count} tags");

                return sb.ToString();
            }
        }

        public JwLibraryFile()
        {
            MergeParameters = new MergeParameters();
            MergeParameters.PropertyChanged += MergeParametersPropertyChanged;
        }
        
        private void MergeParametersPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(MergeParameters));
        }
    }
}
