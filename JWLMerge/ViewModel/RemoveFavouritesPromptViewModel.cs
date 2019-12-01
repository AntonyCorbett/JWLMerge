namespace JWLMerge.ViewModel
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using MaterialDesignThemes.Wpf;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class RemoveFavouritesPromptViewModel : ViewModelBase
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
