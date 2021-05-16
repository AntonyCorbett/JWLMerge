namespace JWLMerge.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;
    using JWLMerge.Models;

    internal interface IDialogService
    {
        Task ShowFileFormatErrorsAsync(AggregateException ex);

        Task<bool> ShouldRedactNotesAsync();

        Task<bool> ShouldRemoveFavouritesAsync();

        Task<ImportBibleNotesParams> GetImportBibleNotesParamsAsync(IReadOnlyCollection<Tag> databaseTags);

        Task<(int[] tagIds, bool removeUntaggedNotes, bool removeUnderlining)> GetTagSelectionForNotesRemovalAsync(Tag[] tags, bool includeUntaggedNotes);

        Task<(int[] colourIndexes, bool removeNotes)> GetColourSelectionForUnderlineRemovalAsync(ColourDef[] colours);

        Task<PubColourResult> GetPubAndColourSelectionForUnderlineRemovalAsync(PublicationDef[] pubs, ColourDef[] colors);

        bool IsDialogVisible();
    }
}
