namespace JWLMerge.ViewModel
{
    using System.Collections.Generic;
    using JWLMerge.Models;
    using MaterialDesignThemes.Wpf;
    using Microsoft.Toolkit.Mvvm.ComponentModel;
    using Microsoft.Toolkit.Mvvm.Input;

    internal class BackupFileFormatErrorViewModel : ObservableObject
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
