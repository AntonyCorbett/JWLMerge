namespace JWLMerge.ViewModel
{
    using MaterialDesignThemes.Wpf;
    using Microsoft.Toolkit.Mvvm.ComponentModel;
    using Microsoft.Toolkit.Mvvm.Input;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class RemoveFavouritesPromptViewModel : ObservableObject
    {
        public RemoveFavouritesPromptViewModel()
        {
            YesCommand = new RelayCommand(Yes);
            NoCommand = new RelayCommand(No);
        }

        public RelayCommand YesCommand { get; set; }

        public RelayCommand NoCommand { get; set; }

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
