using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using JWLMerge.Models;
using JWLMerge.Services;
using MaterialDesignThemes.Wpf;

namespace JWLMerge.ViewModel;

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

        OnPropertyChanged(nameof(SelectionMade));
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
#pragma warning disable CA1416
        DialogHost.CloseDialogCommand.Execute(null, null);
#pragma warning restore CA1416
    }

    private void Ok()
    {
        Result = TagItems.Where(x => x.IsChecked).Select(x => x.Id).ToArray();
#pragma warning disable CA1416
        DialogHost.CloseDialogCommand.Execute(null, null);
#pragma warning restore CA1416
    }
}