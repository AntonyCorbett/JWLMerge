using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace JWLMerge.ViewModel;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class RedactNotesPromptViewModel : ObservableObject
{
    public RedactNotesPromptViewModel()
    {
        YesCommand = new RelayCommand(Yes);
        NoCommand = new RelayCommand(No);
    }

    public RelayCommand YesCommand { get; }

    public RelayCommand NoCommand { get; }

    public bool Result { get; private set; }

    private void No()
    {
        Result = false;
#pragma warning disable CA1416
        DialogHost.CloseDialogCommand.Execute(null, null);
#pragma warning restore CA1416
    }

    private void Yes()
    {
        Result = true;
#pragma warning disable CA1416
        DialogHost.CloseDialogCommand.Execute(null, null);
#pragma warning restore CA1416
    }
}