namespace JWLMerge.ViewModel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;
    using JWLMerge.BackupFileServices;
    using JWLMerge.BackupFileServices.Helpers;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;
    using JWLMerge.ExcelServices;
    using JWLMerge.Helpers;
    using JWLMerge.Messages;
    using JWLMerge.Models;
    using JWLMerge.Services;
    using MaterialDesignThemes.Wpf;
    using Serilog;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MainViewModel : ViewModelBase
    {
        private readonly string _latestReleaseUrl = Properties.Resources.LATEST_RELEASE_URL;
        private readonly IDragDropService _dragDropService;
        private readonly IBackupFileService _backupFileService;
        private readonly IWindowService _windowService;
        private readonly IFileOpenSaveService _fileOpenSaveService;
        private readonly IDialogService _dialogService;
        private readonly ISnackbarService _snackbarService;
        private readonly IExcelService _excelService;
        private bool _isBusy;

        public MainViewModel(
            IDragDropService dragDropService, 
            IBackupFileService backupFileService,
            IWindowService windowService,
            IFileOpenSaveService fileOpenSaveService,
            IDialogService dialogService,
            ISnackbarService snackbarService,
            IExcelService excelService)
        {
            _dragDropService = dragDropService;
            _backupFileService = backupFileService;
            _windowService = windowService;
            _fileOpenSaveService = fileOpenSaveService;
            _dialogService = dialogService;
            _snackbarService = snackbarService;
            _excelService = excelService;

            Files.CollectionChanged += FilesCollectionChanged;

            SetTitle();

            // subscriptions...
            Messenger.Default.Register<DragOverMessage>(this, OnDragOver);
            Messenger.Default.Register<DragDropMessage>(this, OnDragDrop);
            Messenger.Default.Register<MainWindowClosingMessage>(this, OnMainWindowClosing);
            
            AddDesignTimeItems();

            InitCommands();

            GetVersionData();
        }

        public ObservableCollection<JwLibraryFile> Files { get; } = new ObservableCollection<JwLibraryFile>();

        public string Title { get; set; }

        public bool FileListEmpty => Files.Count == 0;

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsNotBusy));

                    MergeCommand?.RaiseCanExecuteChanged();
                    CloseCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsNotBusy => !IsBusy;

        public bool IsNewVersionAvailable { get; private set; }

        public string MergeCommandCaption
        {
            get
            {
                int fileCount = GetMergeableFileCount();
                if (fileCount == 1)
                {
                    return "SAVE AS";
                }

                return "MERGE";
            }
        }

        public ISnackbarMessageQueue TheSnackbarMessageQueue => _snackbarService.TheSnackbarMessageQueue;

        // commands...
        public RelayCommand<string> CloseCardCommand { get; set; }

        public RelayCommand<string> ShowDetailsCommand { get; set; }

        public RelayCommand MergeCommand { get; set; }

        public RelayCommand HomepageCommand { get; set; }

        public RelayCommand UpdateCommand { get; set; }

        public RelayCommand<string> RemoveFavouritesCommand { get; set; }

        public RelayCommand<string> RedactNotesCommand { get; set; }

        public RelayCommand<string> ImportBibleNotesCommand { get; set; }

        public RelayCommand<string> ExportBibleNotesCommand { get; set; }

        public RelayCommand<string> RemoveNotesByTagCommand { get; set; }

        public RelayCommand<string> RemoveUnderliningByColourCommand { get; set; }

        public RelayCommand<string> RemoveUnderliningByPubAndColourCommand { get; set; }

        private JwLibraryFile GetFile(string filePath)
        {
            var file = Files.SingleOrDefault(x => x.FilePath.Equals(filePath));
            if (file == null)
            {
                Log.Logger.Error($"Could not find file: {filePath}");
            }

            return file;
        }

        private void OnMainWindowClosing(MainWindowClosingMessage message)
        {
            message.CancelEventArgs.Cancel = IsBusy || _dialogService.IsDialogVisible();
            if (!message.CancelEventArgs.Cancel)
            {
                _windowService.CloseAll();
            }
        }

        private void FilesCollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(FileListEmpty));
            RaisePropertyChanged(nameof(MergeCommandCaption));
            MergeCommand?.RaiseCanExecuteChanged();
        }

        private void InitCommands()
        {
            CloseCardCommand = new RelayCommand<string>(RemoveCard, filePath => !IsBusy && !_dialogService.IsDialogVisible());
            ShowDetailsCommand = new RelayCommand<string>(ShowDetails, filePath => !IsBusy);
            MergeCommand = new RelayCommand(MergeFiles, () => GetMergeableFileCount() > 0 && !IsBusy && !_dialogService.IsDialogVisible());
            HomepageCommand = new RelayCommand(LaunchHomepage);
            UpdateCommand = new RelayCommand(LaunchLatestReleasePage);

            RemoveFavouritesCommand = new RelayCommand<string>(async (filePath) => await RemoveFavouritesAsync(filePath), filePath => !IsBusy);
            RedactNotesCommand = new RelayCommand<string>(async (filePath) => await RedactNotesAsync(filePath), filePath => !IsBusy);
            ImportBibleNotesCommand = new RelayCommand<string>(async (filePath) => await ImportBibleNotesAsync(filePath), filePath => !IsBusy);
            ExportBibleNotesCommand = new RelayCommand<string>(async (filePath) => await ExportBibleNotesAsync(filePath), filePath => !IsBusy);
            RemoveNotesByTagCommand = new RelayCommand<string>(async (filePath) => await RemoveNotesByTagAsync(filePath), filePath => !IsBusy);
            RemoveUnderliningByColourCommand = new RelayCommand<string>(async (filePath) => await RemoveUnderliningByColourAsync(filePath), filePath => !IsBusy);
            RemoveUnderliningByPubAndColourCommand = new RelayCommand<string>(async (filePath) => await RemoveUnderliningByPubAndColourAsync(filePath), filePath => !IsBusy);
        }
        
        private async Task ExportBibleNotesAsync(string filePath)
        {
            var file = GetFile(filePath);

            var bibleNotesExportFilePath = _fileOpenSaveService.GetBibleNotesExportFilePath("Bible Notes File");
            if (string.IsNullOrWhiteSpace(bibleNotesExportFilePath))
            {
                return;
            }

            IsBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    _backupFileService.ExportBibleNotesToExcel(
                        file.BackupFile,
                        bibleNotesExportFilePath,
                        _excelService);
                });

                _snackbarService.Enqueue("Bible notes exported successfully");
            }
            catch (Exception ex)
            {
                _snackbarService.Enqueue("Error exporting Bible notes!");
                Log.Logger.Error(ex, "Could not export Bible notes from file: {filePath}", file.FilePath);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ImportBibleNotesAsync(string filePath)
        {
            var file = GetFile(filePath);

            var bibleNotesImportFilePath = _fileOpenSaveService.GetBibleNotesImportFilePath("Bible Notes File");
            if (string.IsNullOrWhiteSpace(bibleNotesImportFilePath))
            {
                return;
            }

            var userDefinedTags = file.BackupFile.Database.Tags.Where(x => x.Type != 0)
                .OrderBy(x => x.Name)
                .ToList();

            userDefinedTags.Insert(0, new Tag { TagId = 0, Type = 0, Name = "** No Tag **" });

            var options = await _dialogService.GetImportBibleNotesParamsAsync(userDefinedTags).ConfigureAwait(true);
            if (options == null)
            {
                return;
            }

            IsBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    var notesFile = new BibleNotesFile(bibleNotesImportFilePath);

                    _backupFileService.ImportBibleNotes(
                        file.BackupFile,
                        notesFile.GetNotes(),
                        notesFile.GetBibleKeySymbol(),
                        notesFile.GetMepsLanguageId(),
                        options);

                    _backupFileService.WriteNewDatabaseWithClean(file.BackupFile, file.FilePath, file.FilePath);
                });

                _windowService.Close(filePath);
                _snackbarService.Enqueue("Bible notes imported successfully");

                file.RefreshTooltipSummary();
            }
            catch (Exception ex)
            {
                _snackbarService.Enqueue("Error importing Bible notes!");
                Log.Logger.Error(ex, "Could not import Bible notes from file: {filePath}", bibleNotesImportFilePath);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RemoveUnderliningByPubAndColourAsync(string filePath)
        {
            var file = GetFile(filePath);
            
            var colors = ColourHelper.GetHighlighterColoursInUse(file.BackupFile.Database.UserMarks, true);
            var pubs = PublicationHelper.GetPublications(file.BackupFile.Database.Locations, file.BackupFile.Database.UserMarks, true);
            
            var result = await _dialogService.GetPubAndColourSelectionForUnderlineRemovalAsync(pubs, colors);
            if (result == null || result.IsInvalid)
            {
                return;
            }

            IsBusy = true;
            try
            {
                var underliningRemoved = 0;
                await Task.Run(() =>
                {
                    underliningRemoved = _backupFileService.RemoveUnderliningByPubAndColor(
                        file.BackupFile, result.ColorIndex, result.AnyColor, result.PublicationSymbol, result.AnyPublication, result.RemoveAssociatedNotes);

                    if (underliningRemoved > 0)
                    {
                        _backupFileService.WriteNewDatabaseWithClean(file.BackupFile, filePath, filePath);
                    }
                });

                _windowService.Close(filePath);

                _snackbarService.Enqueue(underliningRemoved == 0
                    ? "There was no underlining to remove!"
                    : $"{underliningRemoved} items of underlining removed successfully");

                file.RefreshTooltipSummary();
            }
            catch (Exception ex)
            {
                _snackbarService.Enqueue("Error removing underlining by Publication/Colour!");
                Log.Logger.Error(ex, "Could not remove underlining by Publication/Colour from file: {filePath}", file.FilePath);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RemoveUnderliningByColourAsync(string filePath)
        {
            var file = GetFile(filePath);
            
            if (!file.BackupFile.Database.UserMarks.Any())
            {
                _snackbarService.Enqueue("There is no underlining in the backup file!");
                return;
            }
            
            var colors = ColourHelper.GetHighlighterColoursInUse(file.BackupFile.Database.UserMarks, false);

            var (colourIndexes, removeNotes) = await _dialogService.GetColourSelectionForUnderlineRemovalAsync(colors);
            if (colourIndexes == null || !colourIndexes.Any())
            {
                return;
            }

            IsBusy = true;
            try
            {
                var underliningRemoved = 0;

                await Task.Run(() =>
                {
                    underliningRemoved = _backupFileService.RemoveUnderliningByColourAsync(
                        file.BackupFile, colourIndexes, removeNotes);

                    if (underliningRemoved > 0)
                    {
                        _backupFileService.WriteNewDatabaseWithClean(file.BackupFile, filePath, filePath);
                    }
                });

                _windowService.Close(filePath);

                _snackbarService.Enqueue(underliningRemoved == 0
                    ? "There was no underlining to remove!"
                    : $"{underliningRemoved} items of underlining removed successfully");

                file.RefreshTooltipSummary();
            }
            catch (Exception ex)
            {
                _snackbarService.Enqueue("Error removing underlining by Colour!");
                Log.Logger.Error(ex, "Could not remove underlining by Colour from file: {filePath}", file.FilePath);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RemoveNotesByTagAsync(string filePath)
        {
            var file = GetFile(filePath);

            var tags = TagHelper.GetTagsInUseByNotes(file.BackupFile.Database);

            var includeUntaggedNotes = TagHelper.AnyNotesHaveNoTag(file.BackupFile.Database);

            var (tagIds, removeUntaggedNotes, removeUnderlining) = 
                await _dialogService.GetTagSelectionForNotesRemovalAsync(tags, includeUntaggedNotes);

            if (tagIds == null || !tagIds.Any())
            {
                return;
            }

            IsBusy = true;
            try
            {
                var notesRemovedCount = 0;

                await Task.Run(() =>
                {
                    notesRemovedCount = _backupFileService.RemoveNotesByTag(
                        file.BackupFile, tagIds, removeUntaggedNotes, removeUnderlining);

                    if (notesRemovedCount > 0)
                    {
                        _backupFileService.WriteNewDatabaseWithClean(file.BackupFile, filePath, filePath);
                    }
                });

                _windowService.Close(filePath);

                _snackbarService.Enqueue(notesRemovedCount == 0 
                    ? "There were no notes to remove!"
                    : $"{notesRemovedCount} notes removed successfully");

                file.RefreshTooltipSummary();
            }
            catch (Exception ex)
            {
                _snackbarService.Enqueue("Error removing notes by Tag!");
                Log.Logger.Error(ex, "Could not remove notes by Tag from file: {filePath}", file.FilePath);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RedactNotesAsync(string filePath)
        {
            var file = GetFile(filePath);

            var notes = file.BackupFile?.Database.Notes;
            if (notes == null || !notes.Any())
            {
                _snackbarService.Enqueue("No notes found");
                return;
            }

            if (file.NotesRedacted)
            {
                _snackbarService.Enqueue("Notes already obfuscated");
                return;
            }

            if (await _dialogService.ShouldRedactNotesAsync().ConfigureAwait(true))
            {
                IsBusy = true;
                try
                {
                    var count = 0;
                    await Task.Run(() =>
                    {
                        count = _backupFileService.RedactNotes(file.BackupFile);
                        _backupFileService.WriteNewDatabase(file.BackupFile, filePath, filePath);
                    });
                    
                    _windowService.Close(filePath);
                    file.NotesRedacted = true;
                    _snackbarService.Enqueue($"{count} Notes obfuscated successfully");
                }
                catch (Exception ex)
                {
                    _snackbarService.Enqueue("Error obfuscating notes!");
                    Log.Logger.Error(ex, "Could not obfuscate notes from file: {filePath}", filePath);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task RemoveFavouritesAsync(string filePath)
        {
            var file = GetFile(filePath);
            
            var favourites = file.BackupFile?.Database.TagMaps.Where(x => x.TagId == 1);
            if (favourites == null || !favourites.Any())
            {
                _snackbarService.Enqueue("No favourites found");
                return;
            }

            if (await _dialogService.ShouldRemoveFavouritesAsync().ConfigureAwait(true))
            {
                IsBusy = true;
                try
                {
                    await Task.Run(() =>
                    {
                        _backupFileService.RemoveFavourites(file.BackupFile);
                        _backupFileService.WriteNewDatabase(file.BackupFile, filePath, filePath);
                    });

                    _snackbarService.Enqueue("Favourites removed successfully");
                    _windowService.Close(filePath);

                    file.RefreshTooltipSummary();
                }
                catch (Exception ex)
                {
                    _snackbarService.Enqueue("Error removing favourites!");
                    Log.Logger.Error(ex, "Could not remove favourites from file: {filePath}", filePath);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private void LaunchLatestReleasePage()
        {
            Process.Start(_latestReleaseUrl);
        }

        private void LaunchHomepage()
        {
            Process.Start(Properties.Resources.HOMEPAGE);
        }

        private void PrepareForMerge()
        {
            ReloadFiles();
            ApplyMergeParameters();
        }

        private string GetSaveDialogTitle()
        {
            return GetMergeableFileCount() == 1
                ? "Save Modified File"
                : "Save Merged File";
        }

        private void MergeFiles()
        {
            var destPath = _fileOpenSaveService.GetSaveFilePath(GetSaveDialogTitle());
            if (string.IsNullOrWhiteSpace(destPath))
            {
                return;
            }
            
            IsBusy = true;
            
            Task.Run(() =>
            {
                PrepareForMerge();
                try
                {
                    var schemaFilePath = GetSuitableFilePathForSchema();

                    if (schemaFilePath != null)
                    {
                        var mergedFile = _backupFileService.Merge(Files.Select(x => x.BackupFile).ToArray());
                        _backupFileService.WriteNewDatabase(mergedFile, destPath, schemaFilePath);
                        _snackbarService.Enqueue("Merged successfully");
                    }
                }
                catch (Exception ex)
                {
                    _snackbarService.Enqueue("Could not merge. See log file for more information");
                    Log.Logger.Error(ex, "Could not merge");
                }
                finally
                {
                    // we need to ensure the files are back to normal after 
                    // applying any merge parameters.
                    ReloadFiles();
                }
            }).ContinueWith(previousTask =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    IsBusy = false;
                });
            });
        }

        private void ApplyMergeParameters()
        {
            // ReSharper disable once StyleCop.SA1116
            // ReSharper disable once StyleCop.SA1115
            Parallel.ForEach(Files, file =>
            {
                if (File.Exists(file.FilePath))
                {
                    MergePreparation.ApplyMergeParameters(
                        _backupFileService,
                        file.BackupFile.Database, 
                        file.MergeParameters);
                }
            });
        }

        private void ReloadFiles()
        {
            // ReSharper disable once StyleCop.SA1116
            // ReSharper disable once StyleCop.SA1115
            Parallel.ForEach(Files, file =>
            {
                if (File.Exists(file.FilePath))
                {
                    file.BackupFile = _backupFileService.Load(file.FilePath);
                }
            });
        }

        private string GetSuitableFilePathForSchema()
        {
            foreach (var file in Files)
            {
                if (File.Exists(file.FilePath))
                {
                    return file.FilePath;
                }
            }
                
            return null;
        }

        private void RemoveCard(string filePath)
        {
            foreach (var file in Files)
            {
                if (IsSameFile(file.FilePath, filePath))
                {
                    Files.Remove(file);
                    _windowService.Close(filePath);
                    break;
                }
            }
        }

        private void ShowDetails(string filePath)
        {
            var file = GetFile(filePath);
            if (file != null)
            {
                _windowService.ShowDetailWindow(
                    _backupFileService,
                    filePath,
                    file.NotesRedacted);
            }
        }
        
        private void AddDesignTimeItems()
        {
            if (IsInDesignMode)
            {
                for (int n = 0; n < 3; ++n)
                {
                    Files.Add(DesignTimeFileCreation.CreateMockJwLibraryFile(_backupFileService, n));
                }
            }
        }
        
        private void SetTitle()
        {
            Title = IsInDesignMode
                ? "JWL Merge (design mode)"
                : "JWL Merge";
        }

        private void OnDragOver(DragOverMessage message)
        {
            message.DragEventArgs.Effects = !IsBusy && _dragDropService.CanAcceptDrop(message.DragEventArgs)
                ? DragDropEffects.Copy
                : DragDropEffects.None;

            message.DragEventArgs.Handled = true;
        }

        private void OnDragDrop(DragDropMessage message)
        {
            if (!IsBusy)
            {
                try
                {
                    HandleDroppedFiles(message.DragEventArgs);
                }
                catch (AggregateException ex)
                {
                    Log.Logger.Information(ex, "Unable to accept file");

                    _dialogService.ShowFileFormatErrorsAsync(ex);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Unexpected error");
                }
            }

            message.DragEventArgs.Handled = true;
        }

        private void HandleDroppedFiles(DragEventArgs dragEventArgs)
        {
            var jwLibraryFiles = _dragDropService.GetDroppedFiles(dragEventArgs);

            var tmpFilesCollection = new ConcurrentBag<JwLibraryFile>();

            Parallel.ForEach(jwLibraryFiles, file =>
            {
                var backupFile = _backupFileService.Load(file);

                tmpFilesCollection.Add(new JwLibraryFile
                {
                    BackupFile = backupFile,
                    FilePath = file,
                });
            });

            foreach (var file in tmpFilesCollection)
            {
                if (Files.FirstOrDefault(x => IsSameFile(file.FilePath, x.FilePath)) == null)
                {
                    file.PropertyChanged += FilePropertyChanged;
                    Files.Add(file);
                }
            }
        }

        private void FilePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // when the merge params are modified it can leave the number of mergeable items at less than 2.
            MergeCommand?.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(MergeCommandCaption));
        }

        private bool IsSameFile(string path1, string path2)
        {
            return Path.GetFullPath(path1).Equals(Path.GetFullPath(path2), StringComparison.OrdinalIgnoreCase);
        }

        private int GetMergeableFileCount()
        {
            if (Files.Count == 1)
            {
                var file = Files.First();
                return file.MergeParameters.AnyExcludes() ? 1 : 0;
            }
            
            var count = Files.Count(file => file.MergeParameters.AnyIncludes());
            return count == 1 ? 0 : count;
        }
       
        private void GetVersionData()
        {
            if (IsInDesignMode)
            {
                IsNewVersionAvailable = true;
                RaisePropertyChanged(nameof(IsNewVersionAvailable));
            }
            else
            {
                Task.Delay(2000).ContinueWith(_ =>
                {
                    var latestVersion = VersionDetection.GetLatestReleaseVersion(_latestReleaseUrl);
                    if (latestVersion != null && VersionDetection.GetCurrentVersion().CompareTo(latestVersion) < 0)
                    {
                        // there is a new version....
                        IsNewVersionAvailable = true;
                        RaisePropertyChanged(nameof(IsNewVersionAvailable));
                    }
                });
            }
        }
    }
}