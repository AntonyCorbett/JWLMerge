namespace JWLMerge.ViewModel
{
    using System.Collections.ObjectModel;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using JWLMerge.Models;
    using MaterialDesignThemes.Wpf;

    internal class RemoveUnderliningByPubAndColourViewModel : ViewModelBase
    {
        private bool _removeAssociatedNotes;

        public RemoveUnderliningByPubAndColourViewModel()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        public RelayCommand OkCommand { get; set; }

        public RelayCommand CancelCommand { get; set; }

        public ObservableCollection<PublicationDef> PublicationList { get; } = new ObservableCollection<PublicationDef>();

        public ObservableCollection<ColourDef> ColourItems { get; } = new ObservableCollection<ColourDef>();

        public PublicationDef SelectedPublication { get; set; }

        public ColourDef SelectedColour { get; set; }

        public bool SelectionMade => SelectedPublication != null && SelectedColour != null;

        public bool RemoveAssociatedNotes
        {
            get => _removeAssociatedNotes;
            set => Set(ref _removeAssociatedNotes, value);
        }

        private void Cancel()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void Ok()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
