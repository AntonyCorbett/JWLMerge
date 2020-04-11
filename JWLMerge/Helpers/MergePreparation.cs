namespace JWLMerge.Helpers
{
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;
    using JWLMerge.Models;

    internal static class MergePreparation
    {
        public static int ApplyMergeParameters(
            IBackupFileService backupFileService,
            Database database, 
            MergeParameters fileMergeParameters)
        {
            int changeCount = 0;
            
            if (!fileMergeParameters.IncludeTags)
            {
                changeCount += backupFileService.RemoveTags(database);
            }

            if (!fileMergeParameters.IncludeBookmarks)
            {
                changeCount += backupFileService.RemoveBookmarks(database);
            }

            // NB - notes must be removed before underlining
            // since if notes are retained, some of the underlining 
            // must also be!
            if (!fileMergeParameters.IncludeNotes)
            {
                changeCount += backupFileService.RemoveNotes(database);
            }

            if (!fileMergeParameters.IncludeUnderlining)
            {
                changeCount += backupFileService.RemoveUnderlining(database);
            }

            return changeCount;
        }
    }
}
