namespace JWLMerge.Services
{
    using System.Collections.Generic;
    using System.Windows;

    internal interface IDragDropService
    {
        bool CanAcceptDrop(DragEventArgs e);

        IEnumerable<string> GetDroppedFiles(DragEventArgs e);
    }
}
