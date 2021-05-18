namespace JWLMerge.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using JWLMerge.Models;
    using MaterialDesignThemes.Wpf;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class RemoveNotesByTagViewModel : ViewModelBase
    {
        private bool _removeAssociatedUnderlining;

        public RemoveNotesByTagViewModel()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);

            TagItems.CollectionChanged += TagItemsCollectionChanged;
        }

        public RelayCommand OkCommand { get; set; }

        public RelayCommand CancelCommand { get; set; }

        public int[] Result { get; private set; }

        public ObservableCollection<TagListItem> TagItems { get; } = new ObservableCollection<TagListItem>();

        public bool SelectionMade => TagItems.Any(x => x.IsChecked);

        public bool RemoveAssociatedUnderlining
        {
            get => _removeAssociatedUnderlining;
            set => Set(ref _removeAssociatedUnderlining, value);
        }

        private void TagItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TagListItem item in e.NewItems)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(SelectionMade));
        }

        private void Cancel()
        {
            Result = null;
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void Ok()
        {
            Result = TagItems.Where(x => x.IsChecked).Select(x => x.Id).ToArray();
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
