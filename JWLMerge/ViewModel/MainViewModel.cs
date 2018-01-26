namespace JWLMerge.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using BackupFileServices;
    using BackupFileServices.Models;
    using BackupFileServices.Models.ManifestFile;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Helpers;
    using Messages;
    using Services;

    internal class MainViewModel : ViewModelBase
    {
        private readonly IDragDropService _dragDropService;
        private readonly IBackupFileService _backupFileService;
        private readonly ObservableCollection<BackupFile> _files = new ObservableCollection<BackupFile>();

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDragDropService dragDropService, IBackupFileService backupFileService)
        {
            _dragDropService = dragDropService;
            _backupFileService = backupFileService;

            _files.CollectionChanged += FilesCollectionChanged;

            SetTitle();

            // subscriptions...
            Messenger.Default.Register<DragOverMessage>(this, OnDragOver);
            Messenger.Default.Register<DragDropMessage>(this, OnDragDrop);

            AddDesignTimeItems();

            InitCommands();
        }

        private void FilesCollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(FileListEmpty));
            MergeCommand.RaiseCanExecuteChanged();
        }

        private void InitCommands()
        {
            CloseCardCommand = new RelayCommand<Manifest>(RemoveCard);
            MergeCommand = new RelayCommand(MergeFiles, () => _files.Count > 1);
        }

        private void MergeFiles()
        {
            // todo:
            // _backupFileService.Merge();
        }

        private void RemoveCard(Manifest manifest)
        {
            foreach (var file in _files)
            {
                if (file.Manifest.Equals(manifest))
                {
                    _files.Remove(file);
                    break;
                }
            }
        }

        public ObservableCollection<BackupFile> Files => _files;

        private void AddDesignTimeItems()
        {
            if (IsInDesignMode)
            {
                for (int n = 0; n < 3; ++n)
                {
                    _files.Add(DesignTimeFileCreation.CreateMockBackupFile(_backupFileService, n));
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
            message.DragEventArgs.Effects = _dragDropService.CanAcceptDrop(message.DragEventArgs)
                ? DragDropEffects.Copy
                : DragDropEffects.None;

            message.DragEventArgs.Handled = true;
        }

        private void OnDragDrop(DragDropMessage message)
        {
            // ReSharper disable once StyleCop.SA1305
            var jwLibraryFiles = _dragDropService.GetDroppedFiles(message.DragEventArgs);
            foreach (var file in jwLibraryFiles)
            {
                var backupFile = _backupFileService.Load(file);
                _files.Add(backupFile);
            }

            message.DragEventArgs.Handled = true;
        }

        public string Title { get; set; }
        
        public bool FileListEmpty => _files.Count == 0;

        public RelayCommand<Manifest> CloseCardCommand { get; set; }
        
        public RelayCommand MergeCommand { get; set; }
    }
}