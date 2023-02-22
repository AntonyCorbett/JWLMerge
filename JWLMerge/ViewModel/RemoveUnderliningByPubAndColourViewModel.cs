using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using JWLMerge.Models;
using MaterialDesignThemes.Wpf;

namespace JWLMerge.ViewModel;

internal sealed class RemoveUnderliningByPubAndColourViewModel : ObservableObject
{
    private bool _removeAssociatedNotes;
    private PublicationDef? _selectedPublication;
    private ColourListItem? _selectedColour;

    public RemoveUnderliningByPubAndColourViewModel()
    {
        OkCommand = new RelayCommand(Ok);
        CancelCommand = new RelayCommand(Cancel);
    }

    public RelayCommand OkCommand { get; }

    public RelayCommand CancelCommand { get; }

    public ObservableCollection<PublicationDef> PublicationList { get; } = new();

    public ObservableCollection<ColourListItem> ColourItems { get; } = new();

    public PublicationDef? SelectedPublication
    {
        get => _selectedPublication;
        set
        {
            if (_selectedPublication != value)
            {
                _selectedPublication = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectionMade));
            }
        }
    }

    public ColourListItem? SelectedColour
    {
        get => _selectedColour;
        set
        {
            if (_selectedColour != value)
            {
                _selectedColour = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectionMade));
            }
        }
    }

    public bool SelectionMade => SelectedPublication != null && SelectedColour != null;

    public PubColourResult? Result { get; private set; }

    public bool RemoveAssociatedNotes
    {
        get => _removeAssociatedNotes;
        set => SetProperty(ref _removeAssociatedNotes, value);
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
        Result = new PubColourResult
        {
            PublicationSymbol = SelectedPublication?.KeySymbol,
            AnyPublication = SelectedPublication?.IsAllPublicationsSymbol ?? false,
            ColorIndex = SelectedColour?.Id ?? 0,
            AnyColor = SelectedColour?.Id == 0,
            RemoveAssociatedNotes = RemoveAssociatedNotes,
        };

#pragma warning disable CA1416
        DialogHost.CloseDialogCommand.Execute(null, null);
#pragma warning restore CA1416
    }
}