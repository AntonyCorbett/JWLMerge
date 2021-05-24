namespace JWLMerge.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using Models;
    using Services;
    using MaterialDesignThemes.Wpf;
    using Microsoft.Toolkit.Mvvm.ComponentModel;
    using Microsoft.Toolkit.Mvvm.Input;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class RemoveNotesByTagViewModel : ObservableObject
    {
        private bool _removeAssociatedUnderlining;
        private bool _removeAssociatedTags;

        public RemoveNotesByTagViewModel()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);

            TagItems.CollectionChanged += TagItemsCollectionChanged;
        }

        public RelayCommand OkCommand { get; }

        public RelayCommand CancelCommand { get; }

        public int[]? Result { get; private set; }

        public ObservableCollection<TagListItem> TagItems { get; } = new();

        public bool SelectionMade => TagItems.Any(x => x.IsChecked);

        public bool RemoveAssociatedUnderlining
        {
            get => _removeAssociatedUnderlining;
            set => SetProperty(ref _removeAssociatedUnderlining, value);
        }

        public bool RemoveAssociatedTags
        {
            get => _removeAssociatedTags;
            set => SetProperty(ref _removeAssociatedTags, value);
        }

        public string RemoveTagsCaption => NumTagsSelectedExcludingFirst() > 1 
            ? "Remove associated Tags" 
            : "Remove associated Tag";

        private void TagItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (TagListItem item in e.NewItems)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        private int NumTagsSelectedExcludingFirst()
        {
            return TagItems.Count(x => x.IsChecked && x.Id != DialogService.UntaggedItemId);
        }

        private void ItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SelectionMade));
            OnPropertyChanged(nameof(RemoveTagsCaption));
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
