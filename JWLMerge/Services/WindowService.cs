using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JWLMerge.BackupFileServices;
using JWLMerge.ViewModel;

namespace JWLMerge.Services;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class WindowService : IWindowService
{
    private readonly List<DetailWindow> _detailWindows;

    public WindowService()
    {
        _detailWindows = new List<DetailWindow>();
    }

    public void ShowDetailWindow(
        IBackupFileService backupFileService, 
        string filePath,
        bool notesRedacted)
    {
        var existingWindow = GetDetailWindow(filePath);
        if (existingWindow != null)
        {
            existingWindow.Activate();
        }
        else
        {
            var window = CreateDetailWindow(backupFileService, filePath, notesRedacted);
                
            var viewModel = (DetailViewModel)window.DataContext;
            viewModel.SelectedDataType = viewModel.ListItems.FirstOrDefault();
            window.Show();
        }
    }

    public void Close(string filePath)
    {
        var window = GetDetailWindow(filePath);
        window?.Close();
    }

    public void CloseAll()
    {
        foreach (var window in Enumerable.Reverse(_detailWindows))
        {
            window.Close();
        }
    }

    private DetailWindow CreateDetailWindow(
        IBackupFileService backupFileService, 
        string filePath,
        bool notesRedacted)
    {
        var window = new DetailWindow();
        var viewModel = (DetailViewModel)window.DataContext;
        viewModel.FilePath = filePath;
        viewModel.BackupFile = backupFileService.Load(filePath);
        viewModel.NotesRedacted = notesRedacted;

        window.Closed += DetailWindowClosed;
        _detailWindows.Add(window);
        return window;
    }

    private void DetailWindowClosed(object? sender, EventArgs e)
    {
        var window = (DetailWindow?)sender;
        if (window != null)
        {
            _detailWindows.Remove(window);
            window.Closed -= DetailWindowClosed;
        }
    }

    private DetailWindow? GetDetailWindow(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return null;
        }

        foreach (var window in _detailWindows)
        {
            var path = ((DetailViewModel)window.DataContext).FilePath;
            if (!string.IsNullOrEmpty(path) && IsSameFile(path, filePath))
            {
                return window;
            }
        }

        return null;
    }

    private static bool IsSameFile(string path1, string path2)
    {
        return Path.GetFullPath(path1).Equals(Path.GetFullPath(path2), StringComparison.OrdinalIgnoreCase);
    }
}