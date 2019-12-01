namespace JWLMerge.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.Database;
    using JWLMerge.Dialogs;
    using JWLMerge.ViewModel;
    using MaterialDesignThemes.Wpf;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DialogService : IDialogService
    {
        private bool _isDialogVisible;

        public async Task<bool> ShouldRemoveFavourites()
        {
            _isDialogVisible = true;

            var dialog = new RemoveFavouritesPromptDialog();
            var dc = (RemoveFavouritesPromptViewModel)dialog.DataContext;

            await DialogHost.Show(
                dialog,
                "DetailDialogHost",
                (object sender, DialogClosingEventArgs args) =>
                {
                    _isDialogVisible = false;
                }).ConfigureAwait(false);

            return dc.Result;
        }

        public async Task<bool> ShouldRedactNotes()
        {
            _isDialogVisible = true;

            var dialog = new RedactNotesPromptDialog();
            var dc = (RedactNotesPromptViewModel)dialog.DataContext;

            await DialogHost.Show(
                dialog, 
                "DetailDialogHost",
                (object sender, DialogClosingEventArgs args) =>
                {
                    _isDialogVisible = false;
                }).ConfigureAwait(false);

            return dc.Result;
        }

        public async Task<ImportBibleNotesParams> GetImportBibleNotesParams(IReadOnlyCollection<Tag> databaseTags)
        {
            _isDialogVisible = true;

            var dialog = new ImportBibleNotesDialog();
            var dc = (ImportBibleNotesViewModel)dialog.DataContext;

            dc.Tags = databaseTags;
            dc.SelectedTagId = 0;

            await DialogHost.Show(
                dialog,
                "DetailDialogHost",
                (object sender, DialogClosingEventArgs args) =>
                {
                    _isDialogVisible = false;
                }).ConfigureAwait(false);

            return dc.Result;
        }

        public bool IsDialogVisible()
        {
            return _isDialogVisible;
        }
    }
}
