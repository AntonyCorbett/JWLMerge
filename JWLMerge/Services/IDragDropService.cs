using System.Collections.Generic;
using System.Windows;

namespace JWLMerge.Services;

internal interface IDragDropService
{
    bool CanAcceptDrop(DragEventArgs e);

    IEnumerable<string> GetDroppedFiles(DragEventArgs e);
}