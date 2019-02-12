using JWLMerge.BackupFileServices;
using JWLMerge.BackupFileServices.Helpers;
using JWLMerge.Services;

namespace JWLMerge.ViewModel
{
    using System.Collections;
    using System.Collections.Generic;
    using BackupFileServices.Models;
    using BackupFileServices.Models.Database;
    using BackupFileServices.Models.ManifestFile;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using Models;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DetailViewModel : ViewModelBase
    {
        private readonly IBackupFileService _backupFileService;
        private readonly IFileOpenSaveService _fileOpenSaveService;

        public string FilePath { get; set; }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public BackupFile BackupFile { get; set; }

        private DataTypeListItem _selectedDataType;

        public RelayCommand ImportBibleNotesCommand { get; set; }

        public DataTypeListItem SelectedDataType
        {
            get => _selectedDataType;
            set
            {
                if (_selectedDataType != value)
                {
                    _selectedDataType = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DataItemsSource));
                }
            }
        }

        public IEnumerable DataItemsSource
        {
            get
            {
                switch (SelectedDataType.DataType)
                {
                    case JwLibraryFileDataTypes.BlockRange:
                        return BackupFile?.Database.BlockRanges;

                    case JwLibraryFileDataTypes.Location:
                        return BackupFile?.Database.Locations;

                    case JwLibraryFileDataTypes.Bookmark:
                        return BackupFile?.Database.Bookmarks;

                    case JwLibraryFileDataTypes.Note:
                        return BackupFile?.Database.Notes;

                    case JwLibraryFileDataTypes.LastModified:
                        return BackupFile != null
                            ? new List<LastModified> { BackupFile.Database.LastModified }
                            : null;

                    case JwLibraryFileDataTypes.Tag:
                        return BackupFile?.Database.Tags;

                    case JwLibraryFileDataTypes.TagMap:
                        return BackupFile?.Database.TagMaps;

                    case JwLibraryFileDataTypes.UserMark:
                        return BackupFile?.Database.UserMarks;

                    case JwLibraryFileDataTypes.Manifest:
                        return ManifestAsItemsSource(BackupFile?.Manifest);
                }

                return null;
            }
        }

        private IEnumerable ManifestAsItemsSource(Manifest manifest)
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            if (manifest != null)
            {
                result.Add(new KeyValuePair<string, string>("Name", manifest.Name));
                result.Add(new KeyValuePair<string, string>("Created", manifest.CreationDate));
                result.Add(new KeyValuePair<string, string>("Version", manifest.Version.ToString()));
                result.Add(new KeyValuePair<string, string>("Type", manifest.Type.ToString()));
                result.Add(new KeyValuePair<string, string>("LastModified", manifest.UserDataBackup.LastModifiedDate));
                result.Add(new KeyValuePair<string, string>("Device", manifest.UserDataBackup.DeviceName));
                result.Add(new KeyValuePair<string, string>("Database", manifest.UserDataBackup.DatabaseName));
                result.Add(new KeyValuePair<string, string>("Hash", manifest.UserDataBackup.Hash));
                result.Add(new KeyValuePair<string, string>("SchemaVersion", manifest.UserDataBackup.SchemaVersion.ToString()));
            }

            return result;
        }

        public List<DataTypeListItem> ListItems { get; }

        public DetailViewModel(IBackupFileService backupFileService, IFileOpenSaveService fileOpenSaveService)
        {
            _backupFileService = backupFileService;
            _fileOpenSaveService = fileOpenSaveService;

            ListItems = CreateListItems();

            ImportBibleNotesCommand = new RelayCommand(ImportBibleNotes);
        }

        private void ImportBibleNotes()
        {
            var path = _fileOpenSaveService.GetBibleNotesImportFilePath("Bible Notes File");
            if (!string.IsNullOrWhiteSpace(path))
            {
                var file = new BibleNotesFile(path);
                _backupFileService.ImportBibleNotes(BackupFile, file.GetNotes(), file.GetBibleKeySymbol(), file.GetMepsLanguageId());
            }
        }

        private List<DataTypeListItem> CreateListItems()
        {
            return new List<DataTypeListItem>
            {
                new DataTypeListItem { Caption = "Manifest", DataType = JwLibraryFileDataTypes.Manifest },
                new DataTypeListItem { Caption = "Block Range", DataType = JwLibraryFileDataTypes.BlockRange },
                new DataTypeListItem { Caption = "Bookmark", DataType = JwLibraryFileDataTypes.Bookmark },
                new DataTypeListItem { Caption = "Last Modified", DataType = JwLibraryFileDataTypes.LastModified },
                new DataTypeListItem { Caption = "Location", DataType = JwLibraryFileDataTypes.Location },
                new DataTypeListItem { Caption = "Note", DataType = JwLibraryFileDataTypes.Note },
                new DataTypeListItem { Caption = "Tag", DataType = JwLibraryFileDataTypes.Tag },
                new DataTypeListItem { Caption = "Tag Map", DataType = JwLibraryFileDataTypes.TagMap },
                new DataTypeListItem { Caption = "User Mark", DataType = JwLibraryFileDataTypes.UserMark }
            };
        }
    }
}
