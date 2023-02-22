using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using JWLMerge.Models;
using MaterialDesignThemes.Wpf;

namespace JWLMerge.ViewModel;

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

        OnPropertyChanged(nameof(SelectionMade));
    }

    private void ItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectionMade));
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
        Result = ColourItems.Where(x => x.IsChecked).Select(x => x.Id).ToArray();
#pragma warning disable CA1416
        DialogHost.CloseDialogCommand.Execute(null, null);
#pragma warning restore CA1416
    }
}