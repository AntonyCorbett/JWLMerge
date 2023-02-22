using JWLMerge.ImportExportServices.Models;
using System;
using System.Collections.Generic;
using JWLMerge.BackupFileServices.Events;
using JWLMerge.BackupFileServices.Models;
using JWLMerge.BackupFileServices.Models.DatabaseModels;
using JWLMerge.ImportExportServices;

namespace JWLMerge.BackupFileServices;

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
    /// Merges the specified backup files.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <returns>Merged file</returns>
    BackupFile Merge(IReadOnlyCollection<BackupFile> files);

    /// <summary>
    /// Creates a blank backup file.
    /// </summary>
    /// <returns>
    /// A <see cref="BackupFile"/>.
    /// </returns>
    BackupFile CreateBlank();

    /// <summary>
    /// Removes all favourites from the specified backup file.
    /// </summary>
    /// <param name="backup">The target backup file.</param>
    void RemoveFavourites(BackupFile backup);

    /// <summary>
    /// Redacts all notes.
    /// </summary>
    /// <param name="backup">The target backup file.</param>
    /// <returns>Number of notes redacted.</returns>
    int RedactNotes(BackupFile backup);

    /// <summary>
    /// Remove notes by tag.
    /// </summary>
    /// <param name="backup">The target backup file.</param>
    /// <param name="tagIds">Tag IDs to match.</param>
    /// <param name="removeUntaggedNotes">Whether to remove notes that have no tag.</param>
    /// <param name="removeAssociatedUnderlining">Whether to remove any associated underlining.</param>
    /// <param name="removeAssociatedTags">Whether to remove any associated tags.</param>
    /// <returns>Number of notes removed.</returns>
    int RemoveNotesByTag(
        BackupFile backup,
        int[]? tagIds,
        bool removeUntaggedNotes,
        bool removeAssociatedUnderlining,
        bool removeAssociatedTags);

    /// <summary>
    /// Removes underlining by colour.
    /// </summary>
    /// <param name="backup">The target backup.</param>
    /// <param name="colorIndexes">The color indexes to target.</param>
    /// <param name="removeAssociatedNotes">Whether associated notes should also be removed.</param>
    /// <returns>Number of underlined items removed.</returns>
    int RemoveUnderliningByColour(BackupFile backup, int[]? colorIndexes, bool removeAssociatedNotes);

    /// <summary>
    /// Removes underlining by publication and colour.
    /// </summary>
    /// <param name="backup">The target backup.</param>
    /// <param name="colorIndex">The colour to match.</param>
    /// <param name="anyColor">Whether any colour matches.</param>
    /// <param name="publicationSymbol">The publication symbol to match.</param>
    /// <param name="anyPublication">Whether any publication matches.</param>
    /// <param name="removeAssociatedNotes">Whether associated notes should also be removed.</param>
    /// <returns>Number of underlined items removed.</returns>
    int RemoveUnderliningByPubAndColor(
        BackupFile backup,
        int colorIndex,
        bool anyColor,
        string? publicationSymbol,
        bool anyPublication,
        bool removeAssociatedNotes);

    /// <summary>
    /// Imports bible notes.
    /// </summary>
    /// <param name="originalBackupFile">Backup file.</param>
    /// <param name="notes">The notes.</param>
    /// <param name="bibleKeySymbol">Bible key symbol, e.g. "nwtsty", "nwt" or "Rbi8"</param>
    /// <param name="mepsLanguageId">Meps Language Id.</param>
    /// <param name="options">User-specified import parameters.</param>
    /// <returns>
    /// A <see cref="BackupFile"/>.
    /// </returns>
    BackupFile ImportBibleNotes(
        BackupFile originalBackupFile,
        IEnumerable<BibleNote> notes,
        string bibleKeySymbol,
        int mepsLanguageId, 
        ImportBibleNotesParams options);

    ExportBibleNotesResult ExportBibleNotes(BackupFile backupFile, string bibleNotesExportFilePath, IExportToFileService exportService);

    /// <summary>
    /// Cleans the database (ensuring integrity), then writes the specified backup to a "jwlibrary" file.
    /// </summary>
    /// <param name="backup">The backup data.</param>
    /// <param name="newDatabaseFilePath">The new database file path.</param>
    /// <param name="originalJwlibraryFilePathForSchema">The original jwlibrary file path on which to base the new schema.</param>
    void WriteNewDatabaseWithClean(
        BackupFile backup,
        string newDatabaseFilePath,
        string originalJwlibraryFilePathForSchema);

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

    /// <summary>
    /// Removes all the tags from the specified database.
    /// </summary>
    /// <param name="database">The database.</param>
    /// <returns>Number of items removed</returns>
    int RemoveTags(Database database);

    /// <summary>
    /// Removes bookmarks from the specified database.
    /// </summary>
    /// <param name="database">The database.</param>
    /// <returns>Number of items removed</returns>
    int RemoveBookmarks(Database database);

    /// <summary>
    /// Removes input fields from the specified database.
    /// </summary>
    /// <param name="database">The database.</param>
    /// <returns>Number of items removed</returns>
    int RemoveInputFields(Database database);

    /// <summary>
    /// Removes notes from the specified database.
    /// </summary>
    /// <param name="database">The database.</param>
    /// <returns>Number of items removed</returns>
    int RemoveNotes(Database database);

    /// <summary>
    /// Removes underlining (user marks) from the specified database
    /// </summary>
    /// <param name="database">The database.</param>
    /// <returns>Number of items removed</returns>
    int RemoveUnderlining(Database database);
}