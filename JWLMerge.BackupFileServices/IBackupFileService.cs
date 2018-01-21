namespace JWLMerge.BackupFileServices
{
    using System;
    using System.Collections.Generic;
    using Events;
    using Models;

    /// <summary>
    /// The BackupFileService interface.
    /// </summary>
    public interface IBackupFileService
    {
        event EventHandler<ProgressEventArgs> ProgressEvent;
        
        /// <summary>
        /// Loads the specified backup file.
        /// </summary>
        /// <param name="backupFilePath">
        /// The backup file path.
        /// </param>
        /// <returns>
        /// The <see cref="BackupFile"/>.
        /// </returns>
        BackupFile Load(string backupFilePath);

        /// <summary>
        /// Merges the specified backup files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns>Merged file</returns>
        BackupFile Merge(IReadOnlyCollection<string> files);

        /// <summary>
        /// Creates a blank backup file.
        /// </summary>
        /// <returns>
        /// A <see cref="BackupFile"/>.
        /// </returns>
        BackupFile CreateBlank();

        /// <summary>
        /// Writes the specified backup to a "jwlibrary" file.
        /// </summary>
        /// <param name="backup">The backup data.</param>
        /// <param name="newDatabaseFilePath">The new database file path.</param>
        /// <param name="originalJwlibraryFilePathForSchema">The original jwlibrary file path on which to base the new schema.</param>
        void WriteNewDatabase(
            BackupFile backup, 
            string newDatabaseFilePath,
            string originalJwlibraryFilePathForSchema);
    }
}
