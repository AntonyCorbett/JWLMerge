using System.Diagnostics;

namespace JWLMerge.BackupFileServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Events;
    using Exceptions;
    using Helpers;
    using Models;
    using Models.DatabaseModels;
    using Models.ManifestFile;
    using ExcelServices;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using Serilog;

    public sealed class BackupFileService : IBackupFileService
    {
        private const int ManifestVersionSupported = 1;
        private const int DatabaseVersionSupported = 8;
        private const string ManifestEntryName = "manifest.json";
        private const string DatabaseEntryName = "userData.db";

        private readonly Merger _merger = new Merger();
        
        public BackupFileService()
        {
            _merger.ProgressEvent += MergerProgressEvent;
        }

        public event EventHandler<ProgressEventArgs>? ProgressEvent;

        /// <inheritdoc />
        public BackupFile Load(string backupFilePath)
        {
            if (string.IsNullOrEmpty(backupFilePath))
            {
                throw new ArgumentNullException(nameof(backupFilePath));
            }

            if (!File.Exists(backupFilePath))
            {
                throw new BackupFileServicesException($"File does not exist: {backupFilePath}");
            }

            try
            {
                var filename = Path.GetFileName(backupFilePath);
                ProgressMessage($"Loading {filename}");

                using var archive = new ZipArchive(File.OpenRead(backupFilePath), ZipArchiveMode.Read);
                var manifest = ReadManifest(filename, archive);

                var database = ReadDatabase(archive, manifest.UserDataBackup.DatabaseName);

                return new BackupFile
                {
                    Manifest = manifest,
                    Database = database,
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw new BackupFileServicesException($"Unauthorized access to file: {backupFilePath}");
            }
        }

        /// <inheritdoc />
        public BackupFile CreateBlank()
        {
            ProgressMessage("Creating blank file");

            var database = new Database();
            database.InitBlank();
            
            return new BackupFile
            {
                Manifest = new Manifest(),
                Database = database,
            };
        }

        /// <inheritdoc />
        public void RemoveFavourites(BackupFile backup)
        {
            if (backup == null)
            {
                throw new ArgumentNullException(nameof(backup));
            }

            backup.Database.TagMaps.RemoveAll(x => x.TagId == 1);
        }

        /// <inheritdoc />
        public int RedactNotes(BackupFile backup)
        {
            if (backup == null)
            {
                throw new ArgumentNullException(nameof(backup));
            }

            var redactService = new RedactService();

            var count = 0;
            foreach (var note in backup.Database.Notes)
            {
                var redacted = false;

                if (!string.IsNullOrEmpty(note.Title))
                {
                    note.Title = redactService.GetNoteTitle(note.Title.Length);
                    redacted = true;
                }

                if (!string.IsNullOrEmpty(note.Content))
                {
                    note.Content = redactService.GenerateNoteContent(note.Content.Length);
                    redacted = true;
                }

                if (redacted)
                {
                    ++count;
                }
            }

            return count;
        }

        /// <inheritdoc />
        public int RemoveNotesByTag(
            BackupFile backup, 
            int[]? tagIds, 
            bool removeUntaggedNotes, 
            bool removeAssociatedUnderlining,
            bool removeAssociatedTags)
        {
            if (backup == null)
            {
                throw new ArgumentNullException(nameof(backup));
            }

            tagIds ??= Array.Empty<int>();

            var tagIdsHash = tagIds.ToHashSet();

            var tagMapIdsToRemove = new HashSet<int>();
            var noteIdsToRemove = new HashSet<int>();
            var candidateUserMarks = new HashSet<int>();

            if (removeUntaggedNotes)
            {
                // notes without a tag
                foreach (var i in GetNotesWithNoTag(backup))
                {
                    noteIdsToRemove.Add(i);
                }
            }

            foreach (var tagMap in backup.Database.TagMaps)
            {
                if (tagIdsHash.Contains(tagMap.TagId) && tagMap.NoteId != null && tagMap.NoteId > 0)
                {
                    tagMapIdsToRemove.Add(tagMap.TagMapId);
                    noteIdsToRemove.Add(tagMap.NoteId.Value);

                    var note = backup.Database.FindNote(tagMap.NoteId.Value);
                    if (note?.UserMarkId != null)
                    {
                        candidateUserMarks.Add(note.UserMarkId.Value);
                    }
                }
            }

            backup.Database.TagMaps.RemoveAll(x => tagMapIdsToRemove.Contains(x.TagMapId));
            backup.Database.TagMaps.RemoveAll(x => noteIdsToRemove.Contains(x.NoteId ?? 0));
            backup.Database.Notes.RemoveAll(x => noteIdsToRemove.Contains(x.NoteId));

            if (removeAssociatedUnderlining)
            {
                RemoveUnderlining(backup.Database, candidateUserMarks);
            }

            if (removeAssociatedTags)
            {
                RemoveSelectedTags(backup.Database, tagIdsHash);
            }

            return noteIdsToRemove.Count;
        }

        /// <inheritdoc />
        public int RemoveUnderliningByColour(BackupFile backup, int[]? colorIndexes, bool removeAssociatedNotes)
        {
            if (backup == null)
            {
                throw new ArgumentNullException(nameof(backup));
            }

            colorIndexes ??= Array.Empty<int>();

            var userMarkIdsToRemove = new HashSet<int>();
            
            foreach (var mark in backup.Database.UserMarks)
            {
                if (colorIndexes.Contains(mark.ColorIndex))
                {
                    userMarkIdsToRemove.Add(mark.UserMarkId);
                }
            }

            return RemoveUnderlining(backup.Database, userMarkIdsToRemove, removeAssociatedNotes);
        }

        /// <inheritdoc />
        public int RemoveUnderliningByPubAndColor(
            BackupFile backup, 
            int colorIndex, 
            bool anyColor, 
            string? publicationSymbol,
            bool anyPublication, 
            bool removeAssociatedNotes)
        {
            if (backup == null)
            {
                throw new ArgumentNullException(nameof(backup));
            }

            if (string.IsNullOrEmpty(publicationSymbol) && !anyPublication)
            {
                throw new ArgumentNullException(nameof(publicationSymbol));
            }

            var userMarkIdsToRemove = new HashSet<int>();
            
            foreach (var mark in backup.Database.UserMarks)
            {
                if (ShouldRemoveUnderlining(mark, backup.Database, colorIndex, anyColor, publicationSymbol, anyPublication))
                {
                    userMarkIdsToRemove.Add(mark.UserMarkId);
                }
            }

            return RemoveUnderlining(backup.Database, userMarkIdsToRemove, removeAssociatedNotes);
        }

        public void WriteNewDatabaseWithClean(
            BackupFile backup,
            string newDatabaseFilePath,
            string originalJwlibraryFilePathForSchema)
        {
            Clean(backup);
            WriteNewDatabase(backup, newDatabaseFilePath, originalJwlibraryFilePathForSchema);
        }

        /// <inheritdoc />
        public void WriteNewDatabase(
            BackupFile backup, 
            string newDatabaseFilePath, 
            string originalJwlibraryFilePathForSchema)
        {
            if (backup == null)
            {
                throw new ArgumentNullException(nameof(backup));
            }

            ProgressMessage(" Checking validity");
            backup.Database.CheckValidity();

            ProgressMessage("Writing merged database file");

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                Log.Logger.Debug("Created ZipArchive");

                var tmpDatabaseFileName = ExtractDatabaseToFile(originalJwlibraryFilePathForSchema);
                try
                {
                    backup.Manifest.UserDataBackup.Hash = GenerateDatabaseHash(tmpDatabaseFileName);

                    var manifestEntry = archive.CreateEntry(ManifestEntryName);
                    using (var entryStream = manifestEntry.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(
                            JsonConvert.SerializeObject(
                                backup.Manifest,
                                new JsonSerializerSettings
                                {
                                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                }));
                    }
                    
                    AddDatabaseEntryToArchive(archive, backup.Database, tmpDatabaseFileName);
                }
                finally
                {
                    Log.Logger.Debug("Deleting {tmpDatabaseFileName}", tmpDatabaseFileName);
                    File.Delete(tmpDatabaseFileName);
                }
            }

            using var fileStream = new FileStream(newDatabaseFilePath, FileMode.Create);

            ProgressMessage("Finishing");
                    
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.CopyTo(fileStream);
        }
        
        /// <inheritdoc />
        public int RemoveTags(Database database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            // clear all but the first tag (which will be the "favourites")...
            var tagCount = database.Tags.Count;
            if (tagCount > 2)
            {
                database.Tags.RemoveRange(1, tagCount - 1);
            }

            database.TagMaps.Clear();
            
            return tagCount > 1 
                ? tagCount - 1 
                : tagCount;
        }

        /// <inheritdoc />
        public int RemoveBookmarks(Database database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            var count = database.Bookmarks.Count;
            database.Bookmarks.Clear();
            return count;
        }

        /// <inheritdoc />
        public int RemoveInputFields(Database database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            var count = database.InputFields.Count;
            database.InputFields.Clear();
            return count;
        }

        /// <inheritdoc />
        public int RemoveNotes(Database database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            var count = database.Notes.Count;
            database.Notes.Clear();
            return count;
        }

        /// <inheritdoc />
        public int RemoveUnderlining(Database database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (!database.Notes.Any())
            {
                var count = database.UserMarks.Count;
                database.UserMarks.Clear();
                return count;
            }

            // we must retain user marks that are associated with notes...
            HashSet<int> userMarksToRetain = new HashSet<int>();
            foreach (var note in database.Notes)
            {
                if (note.UserMarkId != null)
                {
                    userMarksToRetain.Add(note.UserMarkId.Value);
                }
            }

            var countRemoved = 0;
            foreach (var userMark in Enumerable.Reverse(database.UserMarks))
            {
                if (!userMarksToRetain.Contains(userMark.UserMarkId))
                {
                    database.UserMarks.Remove(userMark);
                    ++countRemoved;
                }
            }

            return countRemoved;
        }

        /// <inheritdoc />
        public BackupFile Merge(IReadOnlyCollection<BackupFile> files)
        {
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            ProgressMessage($"Merging {files.Count} backup files");

            int fileNumber = 1;
            foreach (var file in files)
            {
                Log.Logger.Debug("Merging backup file {fileNumber} = {fileName}", fileNumber++, file.Manifest.Name);
                Log.Logger.Debug("===================");

                Clean(file);
            }

            // just pick the first manifest as the basis for the 
            // manifest in the final merged file...
            var newManifest = UpdateManifest(files.First().Manifest);

            var mergedDatabase = MergeDatabases(files);
            return new BackupFile { Manifest = newManifest, Database = mergedDatabase };
        }

        /// <inheritdoc />
        public BackupFile Merge(IReadOnlyCollection<string> files)
        {
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            ProgressMessage($"Merging {files.Count} backup files");

            var fileNumber = 1;
            var originals = new List<BackupFile>();
            foreach (var file in files)
            {
                Log.Logger.Debug("Merging file {fileNumber} = {fileName}", fileNumber++, file);
                Log.Logger.Debug("============");

                var backupFile = Load(file);
                Clean(backupFile);
                originals.Add(backupFile);
            }

            // just pick the first manifest as the basis for the 
            // manifest in the final merged file...
            var newManifest = UpdateManifest(originals.First().Manifest);
            
            var mergedDatabase = MergeDatabases(originals);
            return new BackupFile { Manifest = newManifest, Database = mergedDatabase };
        }

        /// <inheritdoc />
        public BackupFile ImportBibleNotes(
            BackupFile originalBackupFile,
            IEnumerable<BibleNote> notes,
            string bibleKeySymbol,
            int mepsLanguageId, 
            ImportBibleNotesParams options)
        {
            if (originalBackupFile == null)
            {
                throw new ArgumentNullException(nameof(originalBackupFile));
            }

            if (notes == null)
            {
                throw new ArgumentNullException(nameof(notes));
            }
            
            ProgressMessage("Importing Bible notes");

            var newManifest = UpdateManifest(originalBackupFile.Manifest);
            var notesImporter = new NotesImporter(
                originalBackupFile.Database, 
                bibleKeySymbol, 
                mepsLanguageId,
                options);

            notesImporter.Import(notes);

            return new BackupFile { Manifest = newManifest, Database = originalBackupFile.Database };
        }

        public void ExportBibleNotesToExcel(
            BackupFile backupFile, string bibleNotesExportFilePath, IExcelService excelService)
        {
            File.Delete(bibleNotesExportFilePath);
            if (File.Exists(bibleNotesExportFilePath))
            {
                throw new BackupFileServicesException(
                    $"Could not delete existing Excel file: {bibleNotesExportFilePath}");
            }

            var notesToWrite = new List<ExcelServices.Models.BibleNote>();

            var tags = backupFile.Database.TagMaps.Where(x => x.NoteId != null).ToLookup(map => map.NoteId, map => map);

            foreach (var note in backupFile.Database.Notes)
            {
                if (note.LocationId == null)
                {
                    continue;
                }

                var location = backupFile.Database.FindLocation(note.LocationId.Value);

                if (location?.BookNumber == null)
                {
                    continue;
                }

                int? colorCode = null;
                if (note.UserMarkId != null)
                {
                    var userMark = backupFile.Database.FindUserMark(note.UserMarkId.Value);
                    if (userMark != null)
                    {
                        colorCode = userMark.ColorIndex;
                    }
                }
                
                notesToWrite.Add(new ExcelServices.Models.BibleNote(
                    location.BookNumber.Value, BibleBookNames.GetName(location.BookNumber.Value))
                {
                    ChapterNumber = location.ChapterNumber,
                    VerseNumber = note.BlockIdentifier,
                    NoteTitle = note.Title?.Trim(),
                    NoteContent = note.Content?.Trim(),
                    PubSymbol = location.KeySymbol,
                    ColorCode = colorCode,
                    TagsCsv = GetTagsAsCsv(tags, note.NoteId, backupFile.Database),
                });
            }

            notesToWrite.Sort(SortBibleNotes);

            excelService.AppendToBibleNotesFile(bibleNotesExportFilePath, notesToWrite, 0, bibleNotesExportFilePath);
        }

        private static void RemoveSelectedTags(Database database, HashSet<int> tagIds)
        {
            database.Tags.RemoveAll(x => tagIds.Contains(x.TagId));
            database.TagMaps.RemoveAll(x => tagIds.Contains(x.TagMapId));
        }

        private static string GetTagsAsCsv(ILookup<int?, TagMap> tags, int noteId, Database database)
        {
            var t = tags[noteId].ToArray();
            if (!t.Any())
            {
                return string.Empty;
            }

            var tagNames = t.Select(x => database.FindTag(x.TagId)?.Name).Where(x => !string.IsNullOrEmpty(x));
            return string.Join(", ", tagNames);
        }

        private static bool SupportDatabaseVersion(int version)
        {
            return version == DatabaseVersionSupported;
        }

        private static bool SupportManifestVersion(int version)
        {
            return version == ManifestVersionSupported;
        }

        private static string CreateTemporaryDatabaseFile(
            Database backupDatabase,
            string originalDatabaseFilePathForSchema)
        {
#pragma warning disable S5445 // Insecure temporary file creation methods should not be used
            string tmpFile = Path.GetTempFileName();
#pragma warning restore S5445 // Insecure temporary file creation methods should not be used

            Log.Logger.Debug("Creating temporary database file {tmpFile}", tmpFile);

            new DataAccessLayer(originalDatabaseFilePathForSchema).CreateEmptyClone(tmpFile);
            new DataAccessLayer(tmpFile).PopulateTables(backupDatabase);

            return tmpFile;
        }

        private static Manifest UpdateManifest(Manifest manifestToBaseOn)
        {
            Log.Logger.Debug("Updating manifest");

            Manifest result = manifestToBaseOn.Clone();

            DateTime now = DateTime.Now;
            string simpleDateString = $"{now.Year}-{now.Month:D2}-{now.Day:D2}";

            result.Name = $"merged_{simpleDateString}";
            result.CreationDate = simpleDateString;
            result.UserDataBackup.DeviceName = "JWLMerge";
            result.UserDataBackup.DatabaseName = DatabaseEntryName;

            Log.Logger.Debug("Updated manifest");

            return result;
        }

        private static IEnumerable<int> GetNotesWithNoTag(BackupFile backup)
        {
            var notesWithTags = backup.Database.TagMaps.Select(x => x.NoteId).ToHashSet();

            foreach (var note in backup.Database.Notes)
            {
                if (!notesWithTags.Contains(note.NoteId))
                {
                    yield return note.NoteId;
                }
            }
        }

        private static bool ShouldRemoveUnderlining(
            UserMark mark, Database database, int colorIndex, bool anyColor, string? publicationSymbol, bool anyPublication)
        {
            if (!anyColor && mark.ColorIndex != colorIndex)
            {
                return false;
            }

            if (anyPublication)
            {
                return true;
            }

            var location = database.FindLocation(mark.LocationId);
            return location != null && location.KeySymbol == publicationSymbol;
        }

        private static int RemoveUnderlining(Database database, HashSet<int> userMarkIdsToRemove, bool removeAssociatedNotes)
        {
            var noteIdsToRemove = new HashSet<int>();
            var tagMapIdsToRemove = new HashSet<int>();

            if (userMarkIdsToRemove.Any())
            {
                foreach (var note in database.Notes)
                {
                    if (note.UserMarkId == null)
                    {
                        continue;
                    }

                    if (userMarkIdsToRemove.Contains(note.UserMarkId.Value))
                    {
                        if (removeAssociatedNotes)
                        {
                            noteIdsToRemove.Add(note.NoteId);
                        }
                        else
                        {
                            note.UserMarkId = null;
                        }
                    }
                }

                foreach (var tagMap in database.TagMaps)
                {
                    if (tagMap.NoteId == null)
                    {
                        continue;
                    }

                    if (noteIdsToRemove.Contains(tagMap.NoteId.Value))
                    {
                        tagMapIdsToRemove.Add(tagMap.TagMapId);
                    }
                }
            }

            database.UserMarks.RemoveAll(x => userMarkIdsToRemove.Contains(x.UserMarkId));
            database.Notes.RemoveAll(x => noteIdsToRemove.Contains(x.NoteId));
            database.TagMaps.RemoveAll(x => tagMapIdsToRemove.Contains(x.TagMapId));

            return userMarkIdsToRemove.Count;
        }

        private static void RemoveUnderlining(Database database, HashSet<int> userMarksToRemove)
        {
            foreach (var note in database.Notes)
            {
                if (note.UserMarkId != null && userMarksToRemove.Contains(note.UserMarkId.Value))
                {
                    // we can't delete this user mark because it is still in use (a user mark
                    // may have multiple associated notes).
                    userMarksToRemove.Remove(note.UserMarkId.Value);
                }
            }

            database.UserMarks.RemoveAll(x => userMarksToRemove.Contains(x.UserMarkId));
        }

        private int SortBibleNotes(ExcelServices.Models.BibleNote x, ExcelServices.Models.BibleNote y)
        {
            if (x.BookNumber != y.BookNumber)
            {
                return x.BookNumber.CompareTo(y.BookNumber);
            }

            if (x.ChapterNumber != y.ChapterNumber)
            {
                return x.ChapterNumber ?? 0.CompareTo(y.ChapterNumber ?? 0);
            }

            if (x.VerseNumber != y.VerseNumber)
            {
                return x.VerseNumber ?? 0.CompareTo(y.VerseNumber ?? 0);
            }

            return 0;
        }

        private Database MergeDatabases(IEnumerable<BackupFile> jwlibraryFiles)
        {
            ProgressMessage("Merging databases");
            return _merger.Merge(jwlibraryFiles.Select(x => x.Database));
        }

        private void MergerProgressEvent(object sender, ProgressEventArgs e)
        {
            OnProgressEvent(e);
        }

        private void Clean(BackupFile backupFile)
        {
            Log.Logger.Debug("Cleaning backup file {backupFile}", backupFile.Manifest.Name);

            var cleaner = new Cleaner(backupFile.Database);
            var rowsRemoved = cleaner.Clean();
            if (rowsRemoved > 0)
            {
                ProgressMessage($"Removed {rowsRemoved} inaccessible rows");
            }
        }

        private Database ReadDatabase(ZipArchive archive, string databaseName)
        {
            ProgressMessage($"Reading database {databaseName}");
            
            var databaseEntry = archive.Entries.FirstOrDefault(x => x.Name.Equals(databaseName, StringComparison.OrdinalIgnoreCase));
            if (databaseEntry == null)
            {
                throw new BackupFileServicesException("Could not find database entry in jwlibrary file");
            }

            Database result;
#pragma warning disable S5445 // Insecure temporary file creation methods should not be used
            var tmpFile = Path.GetTempFileName();
#pragma warning restore S5445 // Insecure temporary file creation methods should not be used
            try
            {
                Log.Logger.Debug("Extracting database to {tmpFile}", tmpFile);
                databaseEntry.ExtractToFile(tmpFile, overwrite: true);

                var dataAccessLayer = new DataAccessLayer(tmpFile);
                result = dataAccessLayer.ReadDatabase();
            }
            finally
            {
                Log.Logger.Debug("Deleting {tmpFile}", tmpFile);
                File.Delete(tmpFile);
            }

            return result;
        }

        private string ExtractDatabaseToFile(string jwlibraryFile)
        {
            Log.Logger.Debug("Opening ZipArchive {jwlibraryFile}", jwlibraryFile);

            using var archive = new ZipArchive(File.OpenRead(jwlibraryFile), ZipArchiveMode.Read);
            var manifest = ReadManifest(Path.GetFileName(jwlibraryFile), archive);

            var databaseEntry = archive.Entries.FirstOrDefault(x => x.Name.Equals(manifest.UserDataBackup.DatabaseName, StringComparison.OrdinalIgnoreCase));
#pragma warning disable S5445 // Insecure temporary file creation methods should not be used
            var tmpFile = Path.GetTempFileName();
#pragma warning restore S5445 // Insecure temporary file creation methods should not be used
            databaseEntry.ExtractToFile(tmpFile, overwrite: true);

            Log.Logger.Information("Created temp file: {tmpDatabaseFileName}", tmpFile);
            return tmpFile;
        }

        private Manifest ReadManifest(string filename, ZipArchive archive)
        {
            ProgressMessage("Reading manifest");
            
            var manifestEntry = archive.Entries.FirstOrDefault(x => x.Name.Equals(ManifestEntryName, StringComparison.OrdinalIgnoreCase));
            if (manifestEntry == null)
            {
                throw new BackupFileServicesException($"Could not find manifest entry in jwlibrary file: {filename}");
            }

            using var stream = new StreamReader(manifestEntry.Open());

            var fileContents = stream.ReadToEnd();

            Log.Logger.Debug("Parsing manifest");
            dynamic data = JObject.Parse(fileContents);
                
            int manifestVersion = data.version ?? 0;
            if (!SupportManifestVersion(manifestVersion))
            {
                throw new WrongManifestVersionException(filename, ManifestVersionSupported, manifestVersion);
            }

            int databaseVersion = data.userDataBackup?.schemaVersion ?? 0;
            if (!SupportDatabaseVersion(databaseVersion))
            {
                throw new WrongDatabaseVersionException(filename, DatabaseVersionSupported, databaseVersion);
            }

            var result = JsonConvert.DeserializeObject<Manifest>(fileContents);

            ValidateManifest(filename, result);

            var prettyJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            Log.Logger.Debug("Parsed manifest {manifestJson}", prettyJson);

            return result!;
        }

        private static void ValidateManifest(string filename, Manifest? result)
        {
            if (result == null)
            {
                throw new BackupFileServicesException($"Could not deserialize manifest entry from jwlibrary file: {filename}");
            }

            if (string.IsNullOrEmpty(result.Name))
            {
                throw new BackupFileServicesException($"Could not retrieve manifest name from jwlibrary file: {filename}");
            }

            if (string.IsNullOrEmpty(result.CreationDate))
            {
                throw new BackupFileServicesException($"Could not retrieve manifest creation date from jwlibrary file: {filename}");
            }

            ValidateUserDataBackup(filename, result.UserDataBackup);
        }

        private static void ValidateUserDataBackup(string filename, UserDataBackup userDataBackup)
        {
            if (userDataBackup == null)
            {
                throw new BackupFileServicesException($"Could not retrieve UserDataBackup element from jwlibrary file: {filename}");
            }

            if (string.IsNullOrEmpty(userDataBackup.DatabaseName))
            {
                throw new BackupFileServicesException($"DatabaseName element empty in UserDataBackup from jwlibrary file: {filename}");
            }

            if (string.IsNullOrEmpty(userDataBackup.DeviceName))
            {
                throw new BackupFileServicesException($"DeviceName element empty in UserDataBackup from jwlibrary file: {filename}");
            }

            if (string.IsNullOrEmpty(userDataBackup.Hash))
            {
                throw new BackupFileServicesException($"Hash element empty in UserDataBackup from jwlibrary file: {filename}");
            }

            if (string.IsNullOrEmpty(userDataBackup.LastModifiedDate))
            {
                throw new BackupFileServicesException($"LastModifiedDate element empty in UserDataBackup from jwlibrary file: {filename}");
            }
        }

        /// <summary>
        /// Generates the sha256 database hash that is required in the manifest.json file.
        /// </summary>
        /// <param name="databaseFilePath">
        /// The database file path.
        /// </param>
        /// <returns>The hash.</returns>
        private string GenerateDatabaseHash(string databaseFilePath)
        {
            ProgressMessage("Generating database hash");

            using var fs = new FileStream(databaseFilePath, FileMode.Open);
            using var bs = new BufferedStream(fs);
            using var sha1 = new SHA256Managed();

            byte[] hash = sha1.ComputeHash(bs);
            var sb = new StringBuilder(2 * hash.Length);
            foreach (byte b in hash)
            {
                sb.Append($"{b:x2}");
            }

            return sb.ToString();
        }

        private void AddDatabaseEntryToArchive(
            ZipArchive archive, 
            Database database, 
            string originalDatabaseFilePathForSchema)
        {
            ProgressMessage("Adding database to archive");
            
            var tmpDatabaseFile = CreateTemporaryDatabaseFile(database, originalDatabaseFilePathForSchema);
            try
            {
                archive.CreateEntryFromFile(tmpDatabaseFile, DatabaseEntryName);
            }
            finally
            {
                File.Delete(tmpDatabaseFile);
            }
        }

        private void OnProgressEvent(ProgressEventArgs e)
        {
            ProgressEvent?.Invoke(this, e);
        }

        private void OnProgressEvent(string message)
        {
            OnProgressEvent(new ProgressEventArgs(message));
        }
        
        private void ProgressMessage(string logMessage)
        {
            Log.Logger.Information(logMessage);
            OnProgressEvent(logMessage);
        }
    }
}
