namespace JWLMerge.ViewModel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;
    using JWLMerge.BackupFileServices;
    using JWLMerge.Helpers;
    using JWLMerge.Messages;
    using JWLMerge.Models;
    using JWLMerge.Services;
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
        private bool _isBusy;

        public MainViewModel(
            IDragDropService dragDropService, 
            IBackupFileService backupFileService,
            IWindowService windowService,
            IFileOpenSaveService fileOpenSaveService,
            IDialogService dialogService)
        {
            _dragDropService = dragDropService;
            _backupFileService = backupFileService;
            _windowService = windowService;
            _fileOpenSaveService = fileOpenSaveService;
            _dialogService = dialogService;

            Files.CollectionChanged += FilesCollectionChanged;

            SetTitle();

            // subscriptions...
            Messenger.Default.Register<DragOverMessage>(this, OnDragOver);
            Messenger.Default.Register<DragDropMessage>(this, OnDragDrop);
            Messenger.Default.Register<MainWindowClosingMessage>(this, OnMainWindowClosing);
            Messenger.Default.Register<NotesRedactedMessage>(this, OnNotesRedacted);

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

        // commands...
        public RelayCommand<string> CloseCardCommand { get; set; }

        public RelayCommand<string> ShowDetailsCommand { get; set; }

        public RelayCommand MergeCommand { get; set; }

        public RelayCommand HomepageCommand { get; set; }

        public RelayCommand UpdateCommand { get; set; }

        private JwLibraryFile GetFile(string filePath)
        {
            var file = Files.SingleOrDefault(x => x.FilePath.Equals(filePath));
            if (file == null)
            {
                Log.Logger.Error($"Could not find file: {filePath}");
            }

            return file;
        }

        private void OnNotesRedacted(NotesRedactedMessage message)
        {
            var file = GetFile(message.FilePath);
            if (file != null)
            {
                file.NotesRedacted = true;
            }
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
            if (!string.IsNullOrWhiteSpace(destPath))
            {
                IsBusy = true;
                
                Task.Run(() =>
                {
                    DebugSleep();
                    PrepareForMerge();
                    try
                    {
                        var schemaFilePath = GetSuitableFilePathForSchema();

                        if (schemaFilePath != null)
                        {
                            var mergedFile = _backupFileService.Merge(Files.Select(x => x.BackupFile).ToArray());
                            _backupFileService.WriteNewDatabase(mergedFile, destPath, schemaFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
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
        }

        [Conditional("DEBUG")]
        private void DebugSleep()
        {
            Thread.Sleep(4000);
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