namespace JWLMerge.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;

    internal interface IDialogService
    {
        Task ShowFileFormatErrorsAsync(AggregateException ex);

        Task<bool> ShouldRedactNotesAsync();

        Task<bool> ShouldRemoveFavouritesAsync();

        Task<ImportBibleNotesParams> GetImportBibleNotesParamsAsync(IReadOnlyCollection<Tag> databaseTags);

        bool IsDialogVisible();
    }
}
