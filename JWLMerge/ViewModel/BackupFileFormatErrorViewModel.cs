using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using JWLMerge.Models;
using MaterialDesignThemes.Wpf;

namespace JWLMerge.ViewModel;

internal sealed class BackupFileFormatErrorViewModel : ObservableObject
{
    public BackupFileFormatErrorViewModel()
    {
        OkCommand = new RelayCommand(Ok);
    }

    public List<FileFormatErrorListItem> Errors { get; } = new();

    public RelayCommand OkCommand { get; }

    private void Ok()
    {
#pragma warning disable CA1416 // Validate platform compatibility
        DialogHost.CloseDialogCommand.Execute(null, null);
#pragma warning restore CA1416 // Validate platform compatibility
    }
}