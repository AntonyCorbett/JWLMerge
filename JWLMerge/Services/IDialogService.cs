namespace JWLMerge.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.Database;
    
    internal interface IDialogService
    {
        Task<bool> ShouldRedactNotes();

        Task<ImportBibleNotesParams> GetImportBibleNotesParams(IReadOnlyCollection<Tag> databaseTags);

        bool IsDialogVisible();
    }
}
