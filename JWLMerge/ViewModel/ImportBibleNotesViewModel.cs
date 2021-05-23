namespace JWLMerge.ViewModel
{
    using System.Collections.Generic;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;
    using MaterialDesignThemes.Wpf;
    using Microsoft.Toolkit.Mvvm.ComponentModel;
    using Microsoft.Toolkit.Mvvm.Input;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ImportBibleNotesViewModel : ObservableObject
    {
        private IReadOnlyCollection<Tag> _tags;

        public ImportBibleNotesViewModel()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        public ImportBibleNotesParams Result { get; private set; }

        public RelayCommand OkCommand { get; set; }

        public RelayCommand CancelCommand { get; set; }

        public IReadOnlyCollection<Tag> Tags
        {
            get => _tags;
            set
            {
                _tags = value;
                OnPropertyChanged();
            }
        }

        public int SelectedTagId { get; set; }

        private void Cancel()
        {
            Result = null;
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void Ok()
        {
            Result = new ImportBibleNotesParams
            {
                TagId = SelectedTagId,
            };

            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
