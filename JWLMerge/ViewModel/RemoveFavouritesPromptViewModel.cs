using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JWLMerge.ViewModel
{
    using MaterialDesignThemes.Wpf;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class RemoveFavouritesPromptViewModel : ObservableObject
    {
        public RemoveFavouritesPromptViewModel()
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
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void Yes()
        {
            Result = true;
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
