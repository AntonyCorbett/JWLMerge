namespace JWLMerge.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using Models;
    using MaterialDesignThemes.Wpf;
    using Microsoft.Toolkit.Mvvm.ComponentModel;
    using Microsoft.Toolkit.Mvvm.Input;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class RemoveUnderliningByColourViewModel : ObservableObject
    {
        private bool _removeAssociatedNotes;

        public RemoveUnderliningByColourViewModel()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);

            ColourItems.CollectionChanged += ItemsCollectionChanged;
        }

        public RelayCommand OkCommand { get; }

        public RelayCommand CancelCommand { get; }

        public ObservableCollection<ColourListItem> ColourItems { get; } = new();

        public bool SelectionMade => ColourItems.Any(x => x.IsChecked);

        public int[]? Result { get; private set; }

        public bool RemoveAssociatedNotes
        {
            get => _removeAssociatedNotes;
            set => SetProperty(ref _removeAssociatedNotes, value);
        }

        private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (ColourListItem item in e.NewItems)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SelectionMade));
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
