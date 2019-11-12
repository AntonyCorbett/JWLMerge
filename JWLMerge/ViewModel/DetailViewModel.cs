namespace JWLMerge.ViewModel
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using GalaSoft.MvvmLight.Messaging;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Helpers;
    using JWLMerge.BackupFileServices.Models;
    using JWLMerge.BackupFileServices.Models.Database;
    using JWLMerge.BackupFileServices.Models.ManifestFile;
    using JWLMerge.Messages;
    using JWLMerge.Models;
    using JWLMerge.Services;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DetailViewModel : ViewModelBase
    {
        private readonly IBackupFileService _backupFileService;
        private readonly IFileOpenSaveService _fileOpenSaveService;
        private readonly IRedactService _redactService;
        private bool _isBusy;
        private DataTypeListItem _selectedDataType;
        private bool _notesRedacted;

        public DetailViewModel(
            IBackupFileService backupFileService, 
            IFileOpenSaveService fileOpenSaveService,
            IRedactService redactService)
        {
            _backupFileService = backupFileService;
            _fileOpenSaveService = fileOpenSaveService;
            _redactService = redactService;

            ListItems = CreateListItems();

            ImportBibleNotesCommand = new RelayCommand(ImportBibleNotes);
            RedactNotesCommand = new RelayCommand(RedactNotes);
        }

        public string FilePath { get; set; }

        public BackupFile BackupFile { get; set; }

        public RelayCommand ImportBibleNotesCommand { get; set; }

        public RelayCommand RedactNotesCommand { get; set; }

        public List<DataTypeListItem> ListItems { get; }

        public bool NotesRedacted
        {
            get => _notesRedacted;
            set
            {
                if (_notesRedacted != value)
                {
                    _notesRedacted = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(NotesNotRedacted));
                }
            }
        }

        public bool NotesNotRedacted => !NotesRedacted;

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
                    RaisePropertyChanged(nameof(IsNotesItemSelected));
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged();
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

        public bool IsNotesItemSelected => SelectedDataType.DataType == JwLibraryFileDataTypes.Note;
        
        private IEnumerable ManifestAsItemsSource(Manifest manifest)
        {
            var result = new List<KeyValuePair<string, string>>();

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

        private void RedactNotes()
        {
            var notes = BackupFile?.Database.Notes;
            if (notes != null)
            {
                foreach (var note in notes)
                {
                    if (!string.IsNullOrEmpty(note.Title))
                    {
                        note.Title = _redactService.GetNoteTitle(note.Title.Length);
                    }

                    if (!string.IsNullOrEmpty(note.Content))
                    {
                        note.Content = _redactService.GenerateNoteContent(note.Content.Length);
                    }
                }

                _backupFileService.WriteNewDatabase(BackupFile, FilePath, FilePath);

                SelectedDataType = ListItems.Single(x => x.DataType == JwLibraryFileDataTypes.Manifest);

                Messenger.Default.Send(new NotesRedactedMessage { FilePath = FilePath });

                NotesRedacted = true;
            }
        }

        private void ImportBibleNotes()
        {
            var path = _fileOpenSaveService.GetBibleNotesImportFilePath("Bible Notes File");
            if (!string.IsNullOrWhiteSpace(path))
            {
                IsBusy = true;
                Task.Run(() =>
                {
                    var file = new BibleNotesFile(path);

                    _backupFileService.ImportBibleNotes(
                        BackupFile, file.GetNotes(), file.GetBibleKeySymbol(), file.GetMepsLanguageId());

                    _backupFileService.WriteNewDatabase(BackupFile, FilePath, FilePath);

                    if (SelectedDataType.DataType == JwLibraryFileDataTypes.Note)
                    {
                        SelectedDataType = ListItems.Single(x => x.DataType == JwLibraryFileDataTypes.Manifest);
                    }
                }).ContinueWith(t =>
                {
                    IsBusy = false;
                });
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
                new DataTypeListItem { Caption = "User Mark", DataType = JwLibraryFileDataTypes.UserMark },
            };
        }
    }
}
