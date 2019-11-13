namespace JWLMerge.Services
{
    using System.Threading.Tasks;
    using JWLMerge.Dialogs;
    using JWLMerge.ViewModel;
    using MaterialDesignThemes.Wpf;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DialogService : IDialogService
    {
        private bool _isDialogVisible;

        public async Task<bool> ShouldRedactNotes()
        {
            _isDialogVisible = true;

            var dialog = new RedactNotesPromptDialog();
            var dc = (RedactNotesPromptViewModel)dialog.DataContext;

            await DialogHost.Show(
                dialog, 
                "DetailDialogHost",
                (object sender, DialogClosingEventArgs args) => 
                { _isDialogVisible = false; }).ConfigureAwait(false);

            return dc.Result;
        }

        public bool IsDialogVisible()
        {
            return _isDialogVisible;
        }
    }
}
