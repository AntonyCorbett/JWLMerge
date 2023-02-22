using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using JWLMerge.BackupFileServices.Models;
using JWLMerge.BackupFileServices.Models.DatabaseModels;
using MaterialDesignThemes.Wpf;

namespace JWLMerge.ViewModel;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ImportBibleNotesViewModel : ObservableObject
{
    private IReadOnlyCollection<Tag>? _tags;

    public ImportBibleNotesViewModel()
    {
        OkCommand = new RelayCommand(Ok);
        CancelCommand = new RelayCommand(Cancel);
    }

    public ImportBibleNotesParams? Result { get; private set; }

    public RelayCommand OkCommand { get; set; }

    public RelayCommand CancelCommand { get; set; }

    public IReadOnlyCollection<Tag>? Tags
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

#pragma warning disable CA1416
        DialogHost.CloseDialogCommand.Execute(null, null);
#pragma warning restore CA1416
    }

    private void Ok()
    {
        Result = new ImportBibleNotesParams
        {
            TagId = SelectedTagId,
        };

#pragma warning disable CA1416
        DialogHost.CloseDialogCommand.Execute(null, null);
#pragma warning restore CA1416
    }
}