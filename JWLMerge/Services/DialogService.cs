namespace JWLMerge.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices.Exceptions;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;
    using JWLMerge.Dialogs;
    using JWLMerge.Models;
    using JWLMerge.ViewModel;
    using MaterialDesignThemes.Wpf;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DialogService : IDialogService
    {
        private bool _isDialogVisible;

        public async Task ShowFileFormatErrorsAsync(AggregateException ex)
        {
            _isDialogVisible = true;

            var dialog = new FileFormatErrorDialog();
            var dc = (BackupFileFormatErrorViewModel)dialog.DataContext;

            dc.Errors.Clear();

            foreach (var e in ex.InnerExceptions)
            {
                if (e is BackupFileServicesException bex)
                {
                    switch (e)
                    {
                        case WrongDatabaseVersionException dbVerEx:
                            dc.Errors.Add(new FileFormatErrorListItem
                                { Filename = dbVerEx.Filename, ErrorMsg = dbVerEx.Message });
                            break;
                        case WrongManifestVersionException mftVerEx:
                            dc.Errors.Add(new FileFormatErrorListItem
                                { Filename = mftVerEx.Filename, ErrorMsg = mftVerEx.Message });
                            break;
                        default:
                            dc.Errors.Add(new FileFormatErrorListItem
                                { Filename = "Error", ErrorMsg = bex.Message });
                            break;
                    }
                }
            }

            await DialogHost.Show(
                dialog,
                (object sender, DialogClosingEventArgs args) =>
                {
                    _isDialogVisible = false;
                }).ConfigureAwait(false);
        }

        public async Task<bool> ShouldRemoveFavouritesAsync()
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

        public async Task<bool> ShouldRedactNotesAsync()
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

        public async Task<ImportBibleNotesParams> GetImportBibleNotesParamsAsync(IReadOnlyCollection<Tag> databaseTags)
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
