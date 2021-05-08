namespace JWLMerge.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using JWLMerge.Models;
    using MaterialDesignThemes.Wpf;

    internal class RemoveUnderliningByColourViewModel : ViewModelBase
    {
        private bool _removeAssociatedNotes;

        public RemoveUnderliningByColourViewModel()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);

            ColourItems.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ColourListItem item in e.NewItems)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(SelectionMade));
        }

        public RelayCommand OkCommand { get; set; }

        public RelayCommand CancelCommand { get; set; }

        public ObservableCollection<ColourListItem> ColourItems { get; } = new ObservableCollection<ColourListItem>();

        public bool SelectionMade => ColourItems.Any(x => x.IsChecked);

        public int[] Result { get; private set; }

        public bool RemoveAssociatedNotes
        {
            get => _removeAssociatedNotes;
            set => Set(ref _removeAssociatedNotes, value);
        }

        private void Cancel()
        {
            Result = null;
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void Ok()
        {
            Result = ColourItems.Where(x => x.IsChecked).Select(x => x.Id).ToArray();
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
