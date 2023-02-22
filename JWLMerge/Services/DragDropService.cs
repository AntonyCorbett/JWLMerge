using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace JWLMerge.Services;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DragDropService : IDragDropService
{
    /// <summary>
    /// Determines whether we can accept the drag and drop operation
    /// </summary>
    /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
    /// <returns>
    ///   <c>true</c> if we accept; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Note that we accept multiple files so long as at least one is a ".jwlibrary" file.
    /// </remarks>
    public bool CanAcceptDrop(DragEventArgs e)
    {
        if (e.Data is DataObject dataObject && dataObject.ContainsFileDropList())
        {
            foreach (var filePath in dataObject.GetFileDropList())
            {
                if (IsJwLibraryFile(filePath))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public IEnumerable<string> GetDroppedFiles(DragEventArgs e)
    {
        var result = new List<string>();
            
        if (e.Data is DataObject dataObject && dataObject.ContainsFileDropList())
        {
            foreach (var filePath in dataObject.GetFileDropList())
            {
                if (IsJwLibraryFile(filePath) && !string.IsNullOrEmpty(filePath))
                {
                    result.Add(filePath);
                }
            }
        }

        return result;
    }

    private static bool IsJwLibraryFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }

        var ext = Path.GetExtension(filePath);
        return ext.Equals(".jwlibrary", StringComparison.OrdinalIgnoreCase);
    }
}