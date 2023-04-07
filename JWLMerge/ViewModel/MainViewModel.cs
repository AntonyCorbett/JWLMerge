using JWLMerge.ImportExportServices.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JWLMerge.BackupFileServices.Helpers;
using JWLMerge.BackupFileServices;
using MaterialDesignThemes.Wpf;
using Serilog;
using JWLMerge.EventTracking;
using JWLMerge.ExcelServices;
using JWLMerge.Helpers;
using JWLMerge.ImportExportServices;
using JWLMerge.Messages;
using JWLMerge.Models;
using JWLMerge.Services;
using Tag = JWLMerge.BackupFileServices.Models.DatabaseModels.Tag;

namespace JWLMerge.ViewModel;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class MainViewModel : ObservableObject
{
    private readonly string _latestReleaseUrl = Properties.Resources.LATEST_RELEASE_URL;
    private readonly IDragDropService _dragDropService;
    private readonly IBackupFileService _backupFileService;
    private readonly IWindowService _windowService;
    private readonly IFileOpenSaveService _fileOpenSaveService;
    private readonly IDialogService _dialogService;
    private readonly ISnackbarService _snackbarService;
    private bool _isBusy;

    public MainViewModel(
        IDragDropService dragDropService, 
        IBackupFileService backupFileService,
        IWindowService windowService,
        IFileOpenSaveService fileOpenSaveService,
        IDialogService dialogService,
        ISnackbarService snackbarService)
    {
        _dragDropService = dragDropService;
        _backupFileService = backupFileService;
        _windowService = windowService;
        _fileOpenSaveService = fileOpenSaveService;
        _dialogService = dialogService;
        _snackbarService = snackbarService;
        
        Files.CollectionChanged += FilesCollectionChanged;

        SetTitle();

        // subscriptions...
        WeakReferenceMessenger.Default.Register<DragOverMessage>(this, OnDragOver);
        WeakReferenceMessenger.Default.Register<DragDropMessage>(this, OnDragDrop);
        WeakReferenceMessenger.Default.Register<MainWindowClosingMessage>(this, OnMainWindowClosing);
            
        AddDesignTimeItems();

        InitCommands();

        GetVersionData();
    }

    public ObservableCollection<JwLibraryFile> Files { get; } = new();

    public string Title { get; private set; } = null!;

    public bool FileListEmpty => Files.Count == 0;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));

                MergeCommand.NotifyCanExecuteChanged();
                CloseCardCommand.NotifyCanExecuteChanged();
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
    public RelayCommand<string> CloseCardCommand { get; private set; } = null!;

    public RelayCommand<string> ShowDetailsCommand { get; private set; } = null!;

    public RelayCommand MergeCommand { get; private set; } = null!;

    public RelayCommand HomepageCommand { get; private set; } = null!;

    public RelayCommand UpdateCommand { get; private set; } = null!;

    public RelayCommand<string> RemoveFavouritesCommand { get; private set; } = null!;

    public RelayCommand<string> RedactNotesCommand { get; private set; } = null!;

    public RelayCommand<string> ImportBibleNotesCommand { get; private set; } = null!;

    public RelayCommand<string> ExportBibleNotesCommand { get; private set; } = null!;

    public RelayCommand<string> RemoveNotesByTagCommand { get; private set; } = null!;

    public RelayCommand<string> RemoveUnderliningByColourCommand { get; private set; } = null!;

    public RelayCommand<string> RemoveUnderliningByPubAndColourCommand { get; private set; } = null!;

    private JwLibraryFile? GetFile(string? filePath)
    {
        var file = Files.SingleOrDefault(x => x.FilePath.Equals(filePath, StringComparison.Ordinal));
        if (file == null)
        {
            Log.Logger.Error($"Could not find file: {filePath}");
        }

        return file;
    }

    private void OnMainWindowClosing(object recipient, MainWindowClosingMessage message)
    {
        message.CancelEventArgs.Cancel = IsBusy || _dialogService.IsDialogVisible();
        if (!message.CancelEventArgs.Cancel)
        {
            _windowService.CloseAll();
        }
    }

    private void FilesCollectionChanged(
        object? sender, 
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(FileListEmpty));
        OnPropertyChanged(nameof(MergeCommandCaption));
        MergeCommand.NotifyCanExecuteChanged();
    }

    private void InitCommands()
    {
        CloseCardCommand = new RelayCommand<string>(RemoveCard, _ => !IsBusy && !_dialogService.IsDialogVisible());
        ShowDetailsCommand = new RelayCommand<string>(ShowDetails, _ => !IsBusy);
        MergeCommand = new RelayCommand(MergeFiles, () => GetMergeableFileCount() > 0 && !IsBusy && !_dialogService.IsDialogVisible());
        HomepageCommand = new RelayCommand(LaunchHomepage);
        UpdateCommand = new RelayCommand(LaunchLatestReleasePage);

        // ReSharper disable AsyncVoidLambda
        RemoveFavouritesCommand = new RelayCommand<string>(async filePath => await RemoveFavouritesAsync(filePath), _ => !IsBusy);
        RedactNotesCommand = new RelayCommand<string>(async filePath => await RedactNotesAsync(filePath), _ => !IsBusy);
        ImportBibleNotesCommand = new RelayCommand<string>(async filePath => await ImportBibleNotesAsync(filePath), _ => !IsBusy);
        ExportBibleNotesCommand = new RelayCommand<string>(async filePath => await ExportBibleNotesAsync(filePath), _ => !IsBusy);
        RemoveNotesByTagCommand = new RelayCommand<string>(async filePath => await RemoveNotesByTagAsync(filePath), _ => !IsBusy);
        RemoveUnderliningByColourCommand = new RelayCommand<string>(async filePath => await RemoveUnderliningByColourAsync(filePath), _ => !IsBusy);
        RemoveUnderliningByPubAndColourCommand = new RelayCommand<string>(async filePath => await RemoveUnderliningByPubAndColourAsync(filePath), _ => !IsBusy);
        // ReSharper restore AsyncVoidLambda
    }

    private async Task ExportBibleNotesAsync(string? filePath)
    {
        var file = GetFile(filePath);
        if (file == null)
        {
            return;
        }

        var bibleNotesExportFilePath = _fileOpenSaveService.GetBibleNotesExportFilePath("Bible Notes File");
        if (string.IsNullOrWhiteSpace(bibleNotesExportFilePath))
        {
            return;
        }

        var exportFileType = _fileOpenSaveService.GetFileType(bibleNotesExportFilePath);
        if (exportFileType == ImportExportFileType.Unknown)
        {
            return;
        }

        IsBusy = true;
        try
        {
            EventTracker.Track(EventName.ExportNotes);
            
            IExportToFileService? exportService = null;

            await Task.Run(() =>
            {
                switch (exportFileType)
                {
                    case ImportExportFileType.Text:
                        exportService = new TextFileService();
                        break;

                    case ImportExportFileType.Excel:
                        exportService = new ExcelService();
                        break;

                    default:
                        throw new NotSupportedException();
                }
            });

            var result = _backupFileService.ExportBibleNotes(
                file.BackupFile, bibleNotesExportFilePath, exportService!);
            
            switch (result)
            {
                case { NoNotesFound: true }:
                    _snackbarService.Enqueue("No Bible notes found!");
                    break;

                case { SomeNotesTooLarge: true }:
                    _snackbarService.EnqueueWithOk("Some notes were too large to store in a spreadsheet cell and were truncated!");
                    break;

                default:
                    _snackbarService.Enqueue("Bible notes exported successfully");
                    break;
            }
        }
        catch (Exception ex)
        {
            _snackbarService.Enqueue("Error exporting Bible notes!");
            Log.Logger.Error(ex, "Could not export Bible notes from file: {filePath}", file.FilePath);
            EventTracker.Error(ex, "Exporting notes");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ImportBibleNotesAsync(string? filePath)
    {
        var file = GetFile(filePath);
        if (file == null)
        {
            return;
        }
            
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
            EventTracker.Track(EventName.ImportNotes);

            await Task.Run(() =>
            {
                var notesFile = new BibleNotesFile(bibleNotesImportFilePath);
                
                foreach (var section in notesFile.GetPubSymbolsAndLanguages())
                {
                    _backupFileService.ImportBibleNotes(
                        file.BackupFile,
                        notesFile.GetNotes(section),
                        section.PubSymbol,
                        section.LanguageId,
                        options);
                }

                _backupFileService.WriteNewDatabaseWithClean(file.BackupFile, file.FilePath, file.FilePath);
            });

            _windowService.Close(filePath!);
            _snackbarService.Enqueue("Bible notes imported successfully");

            file.RefreshTooltipSummary();
        }
        catch (IOException ex) when ((uint)ex.HResult == 0x80070020)
        {
            _snackbarService.Enqueue("Error - Bible notes file is in use by another process!");
            Log.Logger.Error(ex, "Bible notes file is in use: {filePath}", filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _snackbarService.Enqueue("Error - could not gain access to create file!");
            Log.Logger.Error(ex, "Could not gain access to create file: {filePath}", filePath);
        }
        catch (Exception ex)
        {
            _snackbarService.Enqueue("Error importing Bible notes!");
            Log.Logger.Error(ex, "Could not import Bible notes from file: {filePath}", bibleNotesImportFilePath);
            EventTracker.Error(ex, "Importing notes");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveUnderliningByPubAndColourAsync(string? filePath)
    {
        var file = GetFile(filePath);
        if (file == null)
        {
            return;
        }
            
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
            EventTracker.Track(EventName.RemoveUnderliningByPubColour);

            var underliningRemoved = 0;
            await Task.Run(() =>
            {
                underliningRemoved = _backupFileService.RemoveUnderliningByPubAndColor(
                    file.BackupFile, result.ColorIndex, result.AnyColor, result.PublicationSymbol, result.AnyPublication, result.RemoveAssociatedNotes);

                if (underliningRemoved > 0)
                {
                    _backupFileService.WriteNewDatabaseWithClean(file.BackupFile, filePath!, filePath!);
                }
            });

            _windowService.Close(filePath!);

            _snackbarService.Enqueue(underliningRemoved == 0
                ? "There was no underlining to remove!"
                : $"{underliningRemoved} items of underlining removed successfully");

            file.RefreshTooltipSummary();
        }
        catch (UnauthorizedAccessException ex)
        {
            _snackbarService.Enqueue("Error - could not gain access to create file!");
            Log.Logger.Error(ex, "Could not gain access to create file: {filePath}", filePath);
            EventTracker.Error(ex, "Removing underlining by Pub/Colour");
        }
        catch (Exception ex)
        {
            _snackbarService.Enqueue("Error removing underlining by Publication/Colour!");
            Log.Logger.Error(ex, "Could not remove underlining by Publication/Colour from file: {filePath}", filePath);
            EventTracker.Error(ex, "Removing underlining by Pub/Colour");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveUnderliningByColourAsync(string? filePath)
    {
        var file = GetFile(filePath);
        if (file == null)
        {
            return;
        }
            
        if (!file.BackupFile.Database.UserMarks.Any())
        {
            _snackbarService.Enqueue("There is no underlining in the backup file!");
            return;
        }
            
        var colors = ColourHelper.GetHighlighterColoursInUse(file.BackupFile.Database.UserMarks, false);

        var result = await _dialogService.GetColourSelectionForUnderlineRemovalAsync(colors);
        if (result.ColourIndexes == null || !result.ColourIndexes.Any())
        {
            return;
        }

        IsBusy = true;
        try
        {
            EventTracker.Track(EventName.RemoveUnderliningByColour);

            var underliningRemoved = 0;

            await Task.Run(() =>
            {
                underliningRemoved = _backupFileService.RemoveUnderliningByColour(
                    file.BackupFile, result.ColourIndexes, result.RemoveNotes);

                if (underliningRemoved > 0)
                {
                    _backupFileService.WriteNewDatabaseWithClean(file.BackupFile, filePath!, filePath!);
                }
            });

            _windowService.Close(filePath!);

            _snackbarService.Enqueue(underliningRemoved == 0
                ? "There was no underlining to remove!"
                : $"{underliningRemoved} items of underlining removed successfully");

            file.RefreshTooltipSummary();
        }
        catch (UnauthorizedAccessException ex)
        {
            _snackbarService.Enqueue("Error - could not gain access to create file!");
            Log.Logger.Error(ex, "Could not gain access to create file: {filePath}", filePath);
            EventTracker.Error(ex, "Removing underlining by Colour");
        }
        catch (Exception ex)
        {
            _snackbarService.Enqueue("Error removing underlining by Colour!");
            Log.Logger.Error(ex, "Could not remove underlining by Colour from file: {filePath}", filePath);
            EventTracker.Error(ex, "Removing underlining by Colour");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveNotesByTagAsync(string? filePath)
    {
        var file = GetFile(filePath);
        if (file == null)
        {
            return;
        }

        var tags = TagHelper.GetTagsInUseByNotes(file.BackupFile.Database);

        var includeUntaggedNotes = TagHelper.AnyNotesHaveNoTag(file.BackupFile.Database);

        var result = await _dialogService.GetTagSelectionForNotesRemovalAsync(tags, includeUntaggedNotes);

        if (!result.RemoveUntaggedNotes && (result.TagIds == null || !result.TagIds.Any()))
        {
            return;
        }

        IsBusy = true;
        try
        {
            EventTracker.Track(EventName.RemoveNotesByTag);

            var notesRemovedCount = 0;

            await Task.Run(() =>
            {
                notesRemovedCount = _backupFileService.RemoveNotesByTag(
                    file.BackupFile, 
                    result.TagIds, 
                    result.RemoveUntaggedNotes, 
                    result.RemoveAssociatedUnderlining,
                    result.RemoveAssociatedTags);

                if (notesRemovedCount > 0)
                {
                    _backupFileService.WriteNewDatabaseWithClean(file.BackupFile, filePath!, filePath!);
                }
            });

            _windowService.Close(filePath!);

            _snackbarService.Enqueue(notesRemovedCount == 0 
                ? "There were no notes to remove!"
                : $"{notesRemovedCount} notes removed successfully");

            file.RefreshTooltipSummary();
        }
        catch (UnauthorizedAccessException ex)
        {
            _snackbarService.Enqueue("Error - could not gain access to create file!");
            Log.Logger.Error(ex, "Could not gain access to create file: {filePath}", filePath);
            EventTracker.Error(ex, "Removing notes by Tag");
        }
        catch (Exception ex)
        {
            _snackbarService.Enqueue("Error removing notes by Tag!");
            Log.Logger.Error(ex, "Could not remove notes by Tag from file: {filePath}", filePath);
            EventTracker.Error(ex, "Removing notes by Tag");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RedactNotesAsync(string? filePath)
    {
        var file = GetFile(filePath);

        var notes = file?.BackupFile.Database.Notes;
        if (notes == null || !notes.Any())
        {
            _snackbarService.Enqueue("No notes found");
            return;
        }

        if (file!.NotesRedacted)
        {
            _snackbarService.Enqueue("Notes already obfuscated");
            return;
        }

        if (await _dialogService.ShouldRedactNotesAsync().ConfigureAwait(true))
        {
            IsBusy = true;
            try
            {
                EventTracker.Track(EventName.RedactNotes);

                var count = 0;
                await Task.Run(() =>
                {
                    count = _backupFileService.RedactNotes(file.BackupFile);
                    _backupFileService.WriteNewDatabaseWithClean(file.BackupFile, filePath!, filePath!);
                });

                _windowService.Close(filePath!);
                file.NotesRedacted = true;
                _snackbarService.Enqueue($"{count} Notes obfuscated successfully");
            }
            catch (UnauthorizedAccessException ex)
            {
                _snackbarService.Enqueue("Error - could not gain access to create file!");
                Log.Logger.Error(ex, "Could not gain access to create file: {filePath}", filePath);
                EventTracker.Error(ex, "Redacting notes");
            }
            catch (Exception ex)
            {
                _snackbarService.Enqueue("Error obfuscating notes!");
                Log.Logger.Error(ex, "Could not obfuscate notes from file: {filePath}", filePath);
                EventTracker.Error(ex, "Redacting notes");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task RemoveFavouritesAsync(string? filePath)
    {
        var file = GetFile(filePath);
            
        var favourites = file?.BackupFile.Database.TagMaps.Where(x => x.TagId == 1);
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
                EventTracker.Track(EventName.RemoveFavs);

                await Task.Run(() =>
                {
                    _backupFileService.RemoveFavourites(file!.BackupFile);
                    _backupFileService.WriteNewDatabaseWithClean(file.BackupFile, filePath!, filePath!);
                });

                _snackbarService.Enqueue("Favourites removed successfully");
                _windowService.Close(filePath!);

                file!.RefreshTooltipSummary();
            }
            catch (UnauthorizedAccessException ex)
            {
                _snackbarService.Enqueue("Error - could not gain access to create file!");
                Log.Logger.Error(ex, "Could not gain access to create file: {filePath}", filePath);
                EventTracker.Error(ex, "Removing Favs");
            }
            catch (Exception ex)
            {
                _snackbarService.Enqueue("Error removing favourites!");
                Log.Logger.Error(ex, "Could not remove favourites from file: {filePath}", filePath);
                EventTracker.Error(ex, "Removing Favs");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private void LaunchLatestReleasePage()
    {
        var psi = new ProcessStartInfo
        {
            FileName = _latestReleaseUrl,
            UseShellExecute = true
        };

        Process.Start(psi);
    }

#pragma warning disable U2U1003 // Avoid declaring methods used in delegate constructors static
    private static void LaunchHomepage()
#pragma warning restore U2U1003 // Avoid declaring methods used in delegate constructors static
    {
        var psi = new ProcessStartInfo
        {
            FileName = Properties.Resources.HOMEPAGE,
            UseShellExecute = true
        };

        Process.Start(psi);
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

        EventTracker.TrackMerge(Files.Count);

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
            catch (UnauthorizedAccessException ex)
            {
                _snackbarService.Enqueue("Error - could not gain access to create file!");
                Log.Logger.Error(ex, "Could not gain access to create file: {filePath}", destPath);
                EventTracker.Error(ex, "Merging");
            }
            catch (Exception ex)
            {
                _snackbarService.Enqueue("Could not merge. See log file for more information");
                Log.Logger.Error(ex, "Could not merge");
                EventTracker.Error(ex, "Merging");
            }
            finally
            {
                // we need to ensure the files are back to normal after 
                // applying any merge parameters.
                ReloadFiles();
            }
        }).ContinueWith(_ => Application.Current.Dispatcher.BeginInvoke(new Action(() => IsBusy = false)));
    }

    private void ApplyMergeParameters()
    {
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
        Parallel.ForEach(Files, file =>
        {
            if (File.Exists(file.FilePath))
            {
                file.BackupFile = _backupFileService.Load(file.FilePath);
            }
        });
    }

    private string? GetSuitableFilePathForSchema()
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

    private void RemoveCard(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

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

    private void ShowDetails(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        var file = GetFile(filePath);
        if (file != null)
        {
            EventTracker.Track(EventName.ShowDetails);

            _windowService.ShowDetailWindow(
                _backupFileService,
                filePath,
                file.NotesRedacted);
        }
    }
        
    private void AddDesignTimeItems()
    {
        if (IsInDesignMode())
        {
            for (int n = 0; n < 3; ++n)
            {
                Files.Add(DesignTimeFileCreation.CreateMockJwLibraryFile(_backupFileService, n));
            }
        }
    }
        
    private void SetTitle()
    {
        Title = IsInDesignMode()
            ? "JWL Merge (design mode)"
            : "JWL Merge";
    }

    private void OnDragOver(object recipient, DragOverMessage message)
    {
        message.DragEventArgs.Effects = !IsBusy && _dragDropService.CanAcceptDrop(message.DragEventArgs)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        message.DragEventArgs.Handled = true;
    }

    private void OnDragDrop(object recipient, DragDropMessage message)
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
                EventTracker.Error(ex, "Dropping file");
            }
        }

        message.DragEventArgs.Handled = true;
    }

    private void HandleDroppedFiles(DragEventArgs dragEventArgs)
    {
        var tmpFilesCollection = new ConcurrentBag<JwLibraryFile>();

        var jwLibraryFiles = _dragDropService.GetDroppedFiles(dragEventArgs);
            
        Parallel.ForEach(jwLibraryFiles, file =>
        {
            var backupFile = _backupFileService.Load(file);
            tmpFilesCollection.Add(new JwLibraryFile(file, backupFile));
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

    private void FilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // when the merge params are modified it can leave the number of mergeable items at less than 2.
        MergeCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(MergeCommandCaption));
    }

    private static bool IsSameFile(string path1, string path2)
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
        if (IsInDesignMode())
        {
            IsNewVersionAvailable = true;
            OnPropertyChanged(nameof(IsNewVersionAvailable));
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
                    OnPropertyChanged(nameof(IsNewVersionAvailable));
                }
            });
        }
    }

    private static bool IsInDesignMode()
    {
#if DEBUG
        DependencyObject dep = new();
        return DesignerProperties.GetIsInDesignMode(dep);
#else
            return false;
#endif
    }
}