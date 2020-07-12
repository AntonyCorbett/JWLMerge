namespace JWLMerge.ViewModel
{
    using System.Collections.Generic;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using JWLMerge.Models;
    using MaterialDesignThemes.Wpf;

    internal class BackupFileFormatErrorViewModel : ViewModelBase
    {
        public BackupFileFormatErrorViewModel()
        {
            OkCommand = new RelayCommand(Ok);
        }

        public List<FileFormatErrorListItem> Errors { get; } = new List<FileFormatErrorListItem>();

        public RelayCommand OkCommand { get; }

        private void Ok()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
