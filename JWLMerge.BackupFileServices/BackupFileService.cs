namespace JWLMerge.BackupFileServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using JWLMerge.BackupFileServices.Events;
    using JWLMerge.BackupFileServices.Exceptions;
    using JWLMerge.BackupFileServices.Helpers;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;
    using JWLMerge.BackupFileServices.Models.ManifestFile;
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

        public event EventHandler<ProgressEventArgs> ProgressEvent;

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

            var filename = Path.GetFileName(backupFilePath);
            ProgressMessage($"Loading {filename}");
            
            using (var archive = new ZipArchive(File.OpenRead(backupFilePath), ZipArchiveMode.Read))
            {
                var manifest = ReadManifest(filename, archive);

                var database = ReadDatabase(archive, manifest.UserDataBackup.DatabaseName);

                return new BackupFile
                {
                    Manifest = manifest,
                    Database = database,
                };
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
        public void WriteNewDatabase(
            BackupFile backup, 
            string newDatabaseFilePath, 
            string originalJwlibraryFilePathForSchema)
        {
            if (backup == null)
            {
                throw new ArgumentNullException(nameof(backup));
            }

            ProgressMessage("Writing merged database file");
            
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    Log.Logger.Debug("Created ZipArchive");

                    var tmpDatabaseFileName = ExtractDatabaseToFile(originalJwlibraryFilePathForSchema);
                    try
                    {
                        backup.Manifest.UserDataBackup.Hash = GenerateDatabaseHash(tmpDatabaseFileName);

                        var manifestEntry = archive.CreateEntry(ManifestEntryName);
#pragma warning disable S3966 // Objects should not be disposed more than once
                        using (var entryStream = manifestEntry.Open())
#pragma warning restore S3966 // Objects should not be disposed more than once
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
                
                using (var fileStream = new FileStream(newDatabaseFilePath, FileMode.Create))
                {
                    ProgressMessage("Finishing");
                    
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }
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

            int countRemoved = 0;
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

            int fileNumber = 1;
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

        private static bool SupportDatabaseVersion(int version)
        {
            return version == DatabaseVersionSupported;
        }

        private static bool SupportManifestVersion(int version)
        {
            return version == ManifestVersionSupported;
        }

        private Manifest UpdateManifest(Manifest manifestToBaseOn)
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
            int rowsRemoved = cleaner.Clean();
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
            var tmpFile = Path.GetTempFileName();
            try
            {
                Log.Logger.Debug("Extracting database to {tmpFile}", tmpFile);
                databaseEntry.ExtractToFile(tmpFile, overwrite: true);

                DataAccessLayer dataAccessLayer = new DataAccessLayer(tmpFile);
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
            
            using (var archive = new ZipArchive(File.OpenRead(jwlibraryFile), ZipArchiveMode.Read))
            {
                var manifest = ReadManifest(Path.GetFileName(jwlibraryFile), archive);

                var databaseEntry = archive.Entries.FirstOrDefault(x => x.Name.Equals(manifest.UserDataBackup.DatabaseName, StringComparison.OrdinalIgnoreCase));
                var tmpFile = Path.GetTempFileName();
                databaseEntry.ExtractToFile(tmpFile, overwrite: true);

                Log.Logger.Information("Created temp file: {tmpDatabaseFileName}", tmpFile);
                return tmpFile;
            }
        }

        private Manifest ReadManifest(string filename, ZipArchive archive)
        {
            ProgressMessage("Reading manifest");
            
            var manifestEntry = archive.Entries.FirstOrDefault(x => x.Name.Equals(ManifestEntryName, StringComparison.OrdinalIgnoreCase));
            if (manifestEntry == null)
            {
                throw new BackupFileServicesException($"Could not find manifest entry in jwlibrary file: {filename}");
            }
            
            using (StreamReader stream = new StreamReader(manifestEntry.Open()))
            {
                var fileContents = stream.ReadToEnd();

                Log.Logger.Debug("Parsing manifest");
                dynamic data = JObject.Parse(fileContents);
                
                int manifestVersion = data.version ?? 0;
                if (!SupportManifestVersion(manifestVersion))
                {
                    throw new WrongManifestVersionException(filename, ManifestVersionSupported, manifestVersion);
                }

                int databaseVersion = data.userDataBackup.schemaVersion ?? 0;
                if (!SupportDatabaseVersion(databaseVersion))
                {
                    throw new WrongDatabaseVersionException(filename, DatabaseVersionSupported, databaseVersion);
                }

                var result = JsonConvert.DeserializeObject<Manifest>(fileContents);

                var prettyJson = JsonConvert.SerializeObject(result, Formatting.Indented);
                Log.Logger.Debug("Parsed manifest {manifestJson}", prettyJson);

                return result;
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

            using (FileStream fs = new FileStream(databaseFilePath, FileMode.Open))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                using (SHA256Managed sha1 = new SHA256Managed())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    StringBuilder sb = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        sb.Append($"{b:x2}");
                    }

                    return sb.ToString();
                }
            }
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

        private string CreateTemporaryDatabaseFile(
            Database backupDatabase, 
            string originalDatabaseFilePathForSchema)
        {
            string tmpFile = Path.GetTempFileName();

            Log.Logger.Debug("Creating temporary database file {tmpFile}", tmpFile);

            new DataAccessLayer(originalDatabaseFilePathForSchema).CreateEmptyClone(tmpFile);
            new DataAccessLayer(tmpFile).PopulateTables(backupDatabase);

            return tmpFile;
        }

        private void OnProgressEvent(ProgressEventArgs e)
        {
            ProgressEvent?.Invoke(this, e);
        }

        private void OnProgressEvent(string message)
        {
            OnProgressEvent(new ProgressEventArgs { Message = message });
        }
        
        private void ProgressMessage(string logMessage)
        {
            Log.Logger.Information(logMessage);
            OnProgressEvent(logMessage);
        }
    }
}
